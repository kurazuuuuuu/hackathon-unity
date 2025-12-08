using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.System;
using Game.System.Auth;
using Game.Network;

namespace Game.Scenes
{
    /// <summary>
    /// ログイン・サインアップ画面
    /// </summary>
    public class LoginScene : MonoBehaviour
    {
        [Header("UI - Input")]
        [SerializeField] private TMP_InputField emailInput;
        [SerializeField] private TMP_InputField passwordInput;
        [SerializeField] private TMP_InputField confirmPasswordInput;

        [Header("UI - Buttons")]
        [SerializeField] private Button loginButton;
        [SerializeField] private Button signUpButton;
        [SerializeField] private Button switchModeButton;
        [SerializeField] private TextMeshProUGUI switchModeText;

        [Header("UI - Feedback")]
        [SerializeField] private TextMeshProUGUI errorText;
        [SerializeField] private TextMeshProUGUI titleText;

        [Header("UI - Verification")]
        [SerializeField] private GameObject verificationPanel;
        [SerializeField] private TMP_InputField verificationCodeInput;
        [SerializeField] private Button verifyButton;
        [SerializeField] private Button backToSignUpButton;
        [SerializeField] private TextMeshProUGUI verificationMessageText;

        [Header("UI - Loading")]
        [SerializeField] private GameObject loadingPanel;

        private bool isSignUpMode = false;
        private string tempEmailForVerification = "";
        private string tempPasswordForLogin = "";

        private void Start()
        {
            if (loginButton != null)
            {
                loginButton.onClick.AddListener(OnLoginButtonClicked);
            }

            if (signUpButton != null)
            {
                signUpButton.onClick.AddListener(OnSignUpButtonClicked);
            }

            if (switchModeButton != null)
            {
                switchModeButton.onClick.AddListener(OnSwitchModeClicked);
            }

            if (verifyButton != null)
            {
                verifyButton.onClick.AddListener(OnVerifyButtonClicked);
            }

            if (backToSignUpButton != null)
            {
                backToSignUpButton.onClick.AddListener(OnBackToSignUpClicked);
            }

            if (loadingPanel != null)
            {
                loadingPanel.SetActive(false);
            }

            if (verificationPanel != null)
            {
                verificationPanel.SetActive(false);
            }

            ClearError();
            UpdateUIMode();
        }

        private void UpdateUIMode()
        {
            // ログインモード / サインアップモードの切り替え
            if (loginButton != null)
                loginButton.gameObject.SetActive(!isSignUpMode);
            
            if (signUpButton != null)
                signUpButton.gameObject.SetActive(isSignUpMode);
            
            // サインアップ時のみ表示するフィールド
            if (confirmPasswordInput != null)
                confirmPasswordInput.gameObject.SetActive(isSignUpMode);

            if (titleText != null)
                titleText.text = isSignUpMode ? "アカウント作成" : "ログイン";

            if (switchModeText != null)
                switchModeText.text = isSignUpMode ? "ログインに戻る" : "新規登録はこちら";

            // 検証パネルが表示されている場合は、他の入力パネルを隠すなどの制御が必要ならここで行う
            // 今回はVerificationPanelがOverlay的に表示されるか、専用の画面として扱うかによるが
            // 基本的に検証中はVerificationPanelがActiveになる想定

            ClearError();
        }

        private void OnSwitchModeClicked()
        {
            isSignUpMode = !isSignUpMode;
            UpdateUIMode();
        }

        private void OnBackToSignUpClicked()
        {
            if (verificationPanel != null)
            {
                verificationPanel.SetActive(false);
            }
            // 入力内容はクリアしないでおく（利便性のため）
        }

        private async void OnLoginButtonClicked()
        {
            string email = emailInput?.text ?? "";
            string password = passwordInput?.text ?? "";

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowError("メールアドレスとパスワードを入力してください");
                return;
            }

            if (!email.Contains("@"))
            {
                ShowError("有効なメールアドレスを入力してください");
                return;
            }

            if (ApiClient.Instance == null)
            {
                ShowError("システムエラー: ApiClientが初期化されていません。Bootシーンから起動してください。");
                return;
            }

            SetLoading(true);

            var response = await ApiClient.Instance.Login(email, password);

            SetLoading(false);

            if (response.Success)
            {
                await SceneController.Instance.GoToHome();
            }
            else
            {
                ShowError(response.Error ?? "ログインに失敗しました");
            }
        }

        private async void OnSignUpButtonClicked()
        {
            string email = emailInput?.text ?? "";
            string password = passwordInput?.text ?? "";
            string confirmPassword = confirmPasswordInput?.text ?? "";

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowError("メールアドレスとパスワードを入力してください");
                return;
            }

            if (!email.Contains("@"))
            {
                ShowError("有効なメールアドレスを入力してください");
                return;
            }

            if (password != confirmPassword)
            {
                ShowError("パスワードが一致しません");
                return;
            }

            if (password.Length < 8)
            {
                ShowError("パスワードは8文字以上で入力してください");
                return;
            }

            if (CognitoAuthManager.Instance == null)
            {
                ShowError("認証システムが初期化されていません");
                return;
            }

            SetLoading(true);

            var (success, message) = await CognitoAuthManager.Instance.SignUp(email, password);

            SetLoading(false);

            if (success)
            {
                // サインアップ成功後、検証画面へ
                ShowSuccess("確認コードを送信しました。");
                tempEmailForVerification = email;
                tempPasswordForLogin = password;
                
                if (verificationPanel != null)
                {
                    verificationPanel.SetActive(true);
                    if (verificationMessageText != null)
                    {
                        verificationMessageText.text = $"{email} に送信された確認コードを入力してください";
                    }
                }
            }
            else
            {
                ShowError(message);
            }
        }

        private async void OnVerifyButtonClicked()
        {
            string code = verificationCodeInput?.text ?? "";
            if (string.IsNullOrEmpty(code))
            {
                ShowError("確認コードを入力してください"); // 必要であればVerificationPanel内のエラー表示を使う
                return;
            }

            SetLoading(true);

            var (success, message) = await CognitoAuthManager.Instance.ConfirmSignUp(tempEmailForVerification, code);

            SetLoading(false);

            if (success)
            {
                ShowSuccess("アカウント認証に成功しました。ログイン中...");
                
                // 自動ログイン試行
                SetLoading(true);
                var loginResult = await CognitoAuthManager.Instance.SignIn(tempEmailForVerification, tempPasswordForLogin);
                SetLoading(false);

                if (loginResult.success)
                {
                    await SceneController.Instance.GoToHome();
                }
                else
                {
                    // 認証はできたがログインに失敗した場合
                    if (verificationPanel != null) verificationPanel.SetActive(false);
                    isSignUpMode = false;
                    UpdateUIMode();
                    ShowError("アカウント作成完了。ログインしてください");
                }
            }
            else
            {
                ShowError(message); // ここもVerificationPanel内に出すべきかもしれないが、一旦共通のエラー表示に出す
            }
        }

        private void SetLoading(bool isLoading)
        {
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(isLoading);
            }
            if (loginButton != null)
            {
                loginButton.interactable = !isLoading;
            }
            if (signUpButton != null)
            {
                signUpButton.interactable = !isLoading;
            }
            if (switchModeButton != null)
            {
                switchModeButton.interactable = !isLoading;
            }
            if (verifyButton != null)
            {
                verifyButton.interactable = !isLoading;
            }
            if (backToSignUpButton != null)
            {
                backToSignUpButton.interactable = !isLoading;
            }
        }

        private void ShowError(string message)
        {
            if (errorText != null)
            {
                errorText.color = Color.red;
                errorText.text = message;
            }
            Debug.LogWarning($"Login Error: {message}");
        }

        private void ShowSuccess(string message)
        {
            if (errorText != null)
            {
                errorText.color = Color.green;
                errorText.text = message;
            }
        }

        private void ClearError()
        {
            if (errorText != null)
            {
                errorText.text = "";
            }
        }
    }
}
