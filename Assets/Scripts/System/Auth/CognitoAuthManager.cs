using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;

namespace Game.System.Auth
{
    /// <summary>
    /// AWS Cognitoを使用した認証管理
    /// </summary>
    public class CognitoAuthManager : MonoBehaviour
    {
        public static CognitoAuthManager Instance { get; private set; }

        [Header("Cognito Settings")]
        [SerializeField] private string userPoolId = "";
        [SerializeField] private string clientId = "";
        [SerializeField] private string region = "ap-northeast-1";

        // 認証トークン
        public string IdToken { get; private set; }
        public string AccessToken { get; private set; }
        public string RefreshToken { get; private set; }
        public string UserId { get; private set; }
        
        public bool IsAuthenticated => !string.IsNullOrEmpty(IdToken);

        // Cognito クライアント
        private AmazonCognitoIdentityProviderClient _providerClient;
        private CognitoUserPool _userPool;

        // イベント
        public event Action<bool, string> OnAuthenticationComplete;
        public event Action<bool, string> OnSignUpComplete;
        public event Action<bool, string> OnConfirmSignUpComplete;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeCognito();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeCognito()
        {
            try
            {
                var regionEndpoint = RegionEndpoint.GetBySystemName(region);
                _providerClient = new AmazonCognitoIdentityProviderClient(
                    new Amazon.Runtime.AnonymousAWSCredentials(), 
                    regionEndpoint
                );
                _userPool = new CognitoUserPool(userPoolId, clientId, _providerClient);
                Debug.Log("[CognitoAuth] 初期化完了");
            }
            catch (Exception e)
            {
                Debug.LogError($"[CognitoAuth] 初期化エラー: {e.Message}");
            }
        }

        #region Sign Up
        /// <summary>
        /// ユーザー登録（メールアドレス + パスワード）
        /// </summary>
        public async Task<(bool success, string message)> SignUp(string email, string password)
        {
            try
            {
                var signUpRequest = new SignUpRequest
                {
                    ClientId = clientId,
                    Username = email, // emailをusernameとして使用
                    Password = password,
                    UserAttributes = new List<AttributeType>
                    {
                        new AttributeType { Name = "email", Value = email }
                    }
                };

                var response = await _providerClient.SignUpAsync(signUpRequest);
                
                Debug.Log($"[CognitoAuth] サインアップ成功: {email}");
                OnSignUpComplete?.Invoke(true, "確認メールを送信しました。メール内のリンクをクリックして登録を完了してください。");
                return (true, "確認メールを送信しました");
            }
            catch (UsernameExistsException)
            {
                var msg = "このメールアドレスは既に登録されています";
                OnSignUpComplete?.Invoke(false, msg);
                return (false, msg);
            }
            catch (InvalidPasswordException e)
            {
                var msg = $"パスワードが要件を満たしていません: {e.Message}";
                OnSignUpComplete?.Invoke(false, msg);
                return (false, msg);
            }
            catch (Exception e)
            {
                Debug.LogError($"[CognitoAuth] サインアップエラー: {e.Message}");
                OnSignUpComplete?.Invoke(false, e.Message);
                return (false, e.Message);
            }
        }

        /// <summary>
        /// 確認コードの検証
        /// </summary>
        public async Task<(bool success, string message)> ConfirmSignUp(string email, string confirmationCode)
        {
            try
            {
                var request = new ConfirmSignUpRequest
                {
                    ClientId = clientId,
                    Username = email, // emailをusernameとして使用
                    ConfirmationCode = confirmationCode
                };

                await _providerClient.ConfirmSignUpAsync(request);
                
                Debug.Log($"[CognitoAuth] 確認完了: {email}");
                OnConfirmSignUpComplete?.Invoke(true, "アカウントが確認されました");
                return (true, "アカウントが確認されました");
            }
            catch (CodeMismatchException)
            {
                var msg = "確認コードが正しくありません";
                OnConfirmSignUpComplete?.Invoke(false, msg);
                return (false, msg);
            }
            catch (ExpiredCodeException)
            {
                var msg = "確認コードの有効期限が切れています";
                OnConfirmSignUpComplete?.Invoke(false, msg);
                return (false, msg);
            }
            catch (Exception e)
            {
                Debug.LogError($"[CognitoAuth] 確認エラー: {e.Message}");
                OnConfirmSignUpComplete?.Invoke(false, e.Message);
                return (false, e.Message);
            }
        }
        #endregion

        #region Sign In
        /// <summary>
        /// ログイン（メールアドレス + パスワード）
        /// </summary>
        public async Task<(bool success, string message)> SignIn(string email, string password)
        {
            try
            {
                var request = new InitiateAuthRequest
                {
                    AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
                    ClientId = clientId,
                    AuthParameters = new Dictionary<string, string>
                    {
                        { "USERNAME", email }, // emailをusernameとして使用
                        { "PASSWORD", password }
                    }
                };

                var response = await _providerClient.InitiateAuthAsync(request);

                if (response.AuthenticationResult != null)
                {
                    SetTokens(response.AuthenticationResult);
                    Debug.Log($"[CognitoAuth] ログイン成功: {email}");
                    OnAuthenticationComplete?.Invoke(true, "ログイン成功");
                    return (true, "ログイン成功");
                }
                else if (response.ChallengeName != null)
                {
                    // MFAなどのチャレンジが必要な場合
                    var msg = $"追加の認証が必要です: {response.ChallengeName}";
                    OnAuthenticationComplete?.Invoke(false, msg);
                    return (false, msg);
                }

                return (false, "予期しないレスポンス");
            }
            catch (NotAuthorizedException)
            {
                var msg = "メールアドレスまたはパスワードが正しくありません";
                OnAuthenticationComplete?.Invoke(false, msg);
                return (false, msg);
            }
            catch (UserNotConfirmedException)
            {
                var msg = "アカウントが確認されていません。確認コードを入力してください";
                OnAuthenticationComplete?.Invoke(false, msg);
                return (false, msg);
            }
            catch (UserNotFoundException)
            {
                var msg = "ユーザーが見つかりません";
                OnAuthenticationComplete?.Invoke(false, msg);
                return (false, msg);
            }
            catch (Exception e)
            {
                Debug.LogError($"[CognitoAuth] ログインエラー: {e.Message}");
                OnAuthenticationComplete?.Invoke(false, e.Message);
                return (false, e.Message);
            }
        }

        /// <summary>
        /// トークンをリフレッシュ
        /// </summary>
        public async Task<bool> RefreshTokens()
        {
            if (string.IsNullOrEmpty(RefreshToken))
            {
                Debug.LogWarning("[CognitoAuth] リフレッシュトークンがありません");
                return false;
            }

            try
            {
                var request = new InitiateAuthRequest
                {
                    AuthFlow = AuthFlowType.REFRESH_TOKEN_AUTH,
                    ClientId = clientId,
                    AuthParameters = new Dictionary<string, string>
                    {
                        { "REFRESH_TOKEN", RefreshToken }
                    }
                };

                var response = await _providerClient.InitiateAuthAsync(request);
                
                if (response.AuthenticationResult != null)
                {
                    SetTokens(response.AuthenticationResult);
                    Debug.Log("[CognitoAuth] トークンリフレッシュ成功");
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"[CognitoAuth] トークンリフレッシュエラー: {e.Message}");
                return false;
            }
        }
        #endregion

        #region Sign Out
        /// <summary>
        /// ログアウト
        /// </summary>
        public void SignOut()
        {
            IdToken = null;
            AccessToken = null;
            RefreshToken = null;
            UserId = null;
            Debug.Log("[CognitoAuth] ログアウト完了");
        }
        #endregion

        #region Private Methods
        private void SetTokens(AuthenticationResultType authResult)
        {
            IdToken = authResult.IdToken;
            AccessToken = authResult.AccessToken;
            
            // RefreshToken は初回ログイン時のみ返される
            if (!string.IsNullOrEmpty(authResult.RefreshToken))
            {
                RefreshToken = authResult.RefreshToken;
            }

            // IdトークンからユーザーIDを抽出
            ExtractUserIdFromToken();
        }

        private void ExtractUserIdFromToken()
        {
            if (string.IsNullOrEmpty(IdToken)) return;

            try
            {
                // JWTのペイロード部分をデコード
                var parts = IdToken.Split('.');
                if (parts.Length != 3) return;

                var payload = parts[1];
                // Base64Urlデコード用にパディングを追加
                payload = payload.Replace('-', '+').Replace('_', '/');
                switch (payload.Length % 4)
                {
                    case 2: payload += "=="; break;
                    case 3: payload += "="; break;
                }

                var json = global::System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(payload));
                // 簡易的なパース（sub フィールドを抽出）
                var subIndex = json.IndexOf("\"sub\"");
                if (subIndex >= 0)
                {
                    var colonIndex = json.IndexOf(':', subIndex);
                    var quoteStart = json.IndexOf('"', colonIndex);
                    var quoteEnd = json.IndexOf('"', quoteStart + 1);
                    UserId = json.Substring(quoteStart + 1, quoteEnd - quoteStart - 1);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[CognitoAuth] ユーザーID抽出エラー: {e.Message}");
            }
        }
        #endregion
    }
}
