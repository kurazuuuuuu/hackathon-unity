using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Game.Data;
using Game.System.Auth;

namespace Game.Network
{
    /// <summary>
    /// FastAPIバックエンドとの通信を管理するクライアント
    /// </summary>
    public class ApiClient : MonoBehaviour
    {
        [Header("API Settings")]
        [SerializeField] private string baseUrl = "http://localhost:8000";

        // Cognito認証トークン（CognitoAuthManagerから取得）
        public bool IsAuthenticated => CognitoAuthManager.Instance != null && CognitoAuthManager.Instance.IsAuthenticated;
        public string IdToken => CognitoAuthManager.Instance?.IdToken;
        public string UserId => CognitoAuthManager.Instance?.UserId;
        public bool IsServerAvailable { get; private set; } = false;

        // ユーザーデータ（キャッシュ）
        public UserData UserData { get; private set; }

        // サーバー接続確認完了時のイベント
        public event Action<bool> OnServerCheckComplete;

        #region Singleton
        public static ApiClient Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private async void Start()
        {
            await CheckServerConnection();
        }
        #endregion

        #region Health Check
        /// <summary>
        /// サーバー疎通確認
        /// GET /v1/check
        /// </summary>
        public async Task<bool> CheckServerConnection()
        {
            Debug.Log("サーバー接続確認中...");
            var response = await Get<HealthCheckResponse>("/v1/check", false);
            
            IsServerAvailable = response.Success;
            
            if (IsServerAvailable)
            {
                Debug.Log("サーバー接続確認: OK");
            }
            else
            {
                Debug.LogWarning($"サーバー接続確認: 失敗 - {response.Error}");
            }

            OnServerCheckComplete?.Invoke(IsServerAvailable);
            return IsServerAvailable;
        }
        #endregion

        #region Authentication
        /// <summary>
        /// ログイン（CognitoAuthManager経由 - メールアドレス）
        /// </summary>
        public async Task<ApiResponse<LoginResponse>> Login(string email, string password)
        {
            if (CognitoAuthManager.Instance == null)
            {
                return new ApiResponse<LoginResponse> 
                { 
                    Success = false, 
                    Error = "CognitoAuthManager が初期化されていません" 
                };
            }

            var (success, message) = await CognitoAuthManager.Instance.SignIn(email, password);

            if (success)
            {
                return new ApiResponse<LoginResponse>
                {
                    Success = true,
                    Data = new LoginResponse
                    {
                        access_token = CognitoAuthManager.Instance.AccessToken,
                        id_token = CognitoAuthManager.Instance.IdToken,
                        refresh_token = CognitoAuthManager.Instance.RefreshToken
                    }
                };
            }
            else
            {
                return new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Error = message
                };
            }
        }

        /// <summary>
        /// ログアウト
        /// </summary>
        public void Logout()
        {
            CognitoAuthManager.Instance?.SignOut();
        }
        #endregion

        #region User Data
        /// <summary>
        /// ユーザーデータを取得
        /// GET /v1/user
        /// </summary>
        public async Task<UserData> FetchUserData()
        {
            var response = await Get<UserResponseDto>("/v1/user", true);
            if (response.Success && response.Data != null)
            {
                if (UserData == null) UserData = new UserData();
                UserData.FromApiProfile(response.Data.profile);
                return UserData;
            }
            Debug.LogError($"ユーザーデータ取得失敗: {response.Error}");
            return null;
        }

        /// <summary>
        /// ユーザーデータを保存
        /// PUT /v1/user
        /// </summary>
        public async Task<ApiResponse<StatusResponseDto>> SaveUserData(Data.UserData userData)
        {
            var profile = userData.ToApiProfile();
            var request = new UserSaveRequestDto { profile = profile };
            return await Put<StatusResponseDto>("/v1/user", request, true);
        }
        #endregion

        #region HTTP Methods
        private async Task<ApiResponse<T>> Get<T>(string endpoint, bool requireAuth = true)
        {
            string url = baseUrl + endpoint;

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                if (requireAuth) AddAuthHeader(request);
                return await SendRequest<T>(request);
            }
        }

        private async Task<ApiResponse<T>> Post<T>(string endpoint, object body, bool requireAuth = true)
        {
            return await SendJsonRequest<T>(endpoint, "POST", body, requireAuth);
        }

        private async Task<ApiResponse<T>> Put<T>(string endpoint, object body, bool requireAuth = true)
        {
            return await SendJsonRequest<T>(endpoint, "PUT", body, requireAuth);
        }

        private async Task<ApiResponse<T>> SendJsonRequest<T>(string endpoint, string method, object body, bool requireAuth)
        {
            string url = baseUrl + endpoint;
            string json = JsonUtility.ToJson(body);

            Debug.Log($"[ApiClient] {method} {url}\nBody: {json}");

            using (UnityWebRequest request = new UnityWebRequest(url, method))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                if (requireAuth) AddAuthHeader(request);
                return await SendRequest<T>(request);
            }
        }

        private void AddAuthHeader(UnityWebRequest request)
        {
            string email = CognitoAuthManager.Instance?.Email ?? "";
            
            if (!string.IsNullOrEmpty(IdToken))
            {
                // 形式: email:token
                string authHeader = $"{email}:{IdToken}";
                request.SetRequestHeader("Authorization", authHeader);
                Debug.Log($"[ApiClient] Authorization Header: {email}:{IdToken.Substring(0, Math.Min(IdToken.Length, 20))}...");
            }
            else
            {
                Debug.LogWarning("[ApiClient] Authorization Header: IdToken is null or empty");
            }
        }

        private async Task<ApiResponse<T>> SendRequest<T>(UnityWebRequest request)
        {
            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    T data = JsonUtility.FromJson<T>(request.downloadHandler.text);
                    return new ApiResponse<T> { Success = true, Data = data };
                }
                catch (Exception e)
                {
                    Debug.LogError($"JSON Parse Error: {e.Message}");
                    return new ApiResponse<T> { Success = false, Error = e.Message };
                }
            }
            else
            {
                Debug.LogError($"API Error: {request.error}");
                return new ApiResponse<T> 
                { 
                    Success = false, 
                    Error = request.error,
                    StatusCode = (int)request.responseCode
                };
            }
        }
        #endregion
    }

    #region Request/Response Models
    [Serializable]
    public class ApiResponse<T>
    {
        public bool Success;
        public T Data;
        public string Error;
        public int StatusCode;
    }

    [Serializable]
    public class LoginRequest
    {
        public string username;
        public string email;
        public string password;
    }

    [Serializable]
    public class LoginResponse
    {
        public string access_token;
        public string id_token;
        public string refresh_token;
    }

    [Serializable]
    public class HealthCheckResponse
    {
        public string status;
    }
    #endregion
}

