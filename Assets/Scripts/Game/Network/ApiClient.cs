using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Game.Data;

namespace Game.Network
{
    /// <summary>
    /// FastAPIバックエンドとの通信を管理するクライアント
    /// </summary>
    public class ApiClient : MonoBehaviour
    {
        [Header("API Settings")]
        [SerializeField] private string baseUrl = "http://localhost:8000";

        // Cognito認証トークン
        private string accessToken;
        private string idToken;
        private string refreshToken;

        public bool IsAuthenticated => !string.IsNullOrEmpty(idToken);
        public bool IsServerAvailable { get; private set; } = false;

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
        /// ログイン
        /// POST /v1/user/login
        /// </summary>
        public async Task<ApiResponse<LoginResponse>> Login(string username, string email, string password)
        {
            var request = new LoginRequest 
            { 
                username = username,
                email = email, 
                password = password 
            };
            var response = await Post<LoginResponse>("/v1/user/login", request, false);

            if (response.Success && response.Data != null)
            {
                SetTokens(response.Data.access_token, response.Data.id_token, response.Data.refresh_token);
            }

            return response;
        }

        /// <summary>
        /// トークンを設定
        /// </summary>
        public void SetTokens(string access, string id, string refresh)
        {
            accessToken = access;
            idToken = id;
            refreshToken = refresh;
        }

        /// <summary>
        /// ログアウト
        /// </summary>
        public void Logout()
        {
            accessToken = null;
            idToken = null;
            refreshToken = null;
        }
        #endregion

        #region User Data
        /// <summary>
        /// ユーザーデータを保存
        /// PUT /v1/user/save
        /// </summary>
        public async Task<ApiResponse<StatusResponseDto>> SaveUserData(Data.UserData userData)
        {
            var profile = userData.ToApiProfile();
            var request = new UserSaveRequestDto { profile = profile };
            return await Put<StatusResponseDto>("/v1/user/save", request, true);
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
            if (!string.IsNullOrEmpty(idToken))
            {
                request.SetRequestHeader("Authorization", $"Bearer {idToken}");
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

