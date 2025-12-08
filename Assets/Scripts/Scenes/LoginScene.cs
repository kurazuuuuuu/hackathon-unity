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
        [SerializeField] private TMP_InputField usernameInput;
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

        [Header("UI - Loading")]
        [SerializeField] private GameObject loadingPanel;

        private bool isSignUpMode = false;

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

            if (loadingPanel != null)
            {
                loadingPanel.SetActive(false);
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
            
            if (confirmPasswordInput != null)
                confirmPasswordInput.gameObject.SetActive(isSignUpMode);

            if (titleText != null)
                titleText.text = isSignUpMode ? "アカウント作成" : "ログイン";

            if (switchModeText != null)
                switchModeText.text = isSignUpMode ? "ログインに戻る" : "新規登録はこちら";

            ClearError();
        }

        private void OnSwitchModeClicked()
        {
            isSignUpMode = !isSignUpMode;
            UpdateUIMode();
        }

        private async void OnLoginButtonClicked()
        {
            string username = usernameInput?.text ?? "";
            string password = passwordInput?.text ?? "";

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowError("ユーザー名とパスワードを入力してください");
                return;
            }

            SetLoading(true);

            var response = await ApiClient.Instance.Login(username, password);

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
            string username = usernameInput?.text ?? "";
            string password = passwordInput?.text ?? "";
            string confirmPassword = confirmPasswordInput?.text ?? "";

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowError("ユーザー名とパスワードを入力してください");
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

            var (success, message) = await CognitoAuthManager.Instance.SignUp(username, password);

            SetLoading(false);

            if (success)
            {
                // サインアップ成功後、自動でログイン
                ShowSuccess("アカウントを作成しました。ログイン中...");
                
                SetLoading(true);
                var loginResult = await CognitoAuthManager.Instance.SignIn(username, password);
                SetLoading(false);

                if (loginResult.success)
                {
                    await SceneController.Instance.GoToHome();
                }
                else
                {
                    // 自動ログイン失敗時はログイン画面に切り替え
                    isSignUpMode = false;
                    UpdateUIMode();
                    ShowError("アカウント作成完了。ログインしてください");
                }
            }
            else
            {
                ShowError(message);
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
