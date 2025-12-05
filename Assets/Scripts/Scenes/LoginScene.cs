using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.System;
using Game.Network;

namespace Game.Scenes
{
    /// <summary>
    /// ログイン画面
    /// </summary>
    public class LoginScene : MonoBehaviour
    {
        [Header("UI - Login")]
        [SerializeField] private TMP_InputField usernameInput;
        [SerializeField] private TMP_InputField emailInput;
        [SerializeField] private TMP_InputField passwordInput;
        [SerializeField] private Button loginButton;
        [SerializeField] private TextMeshProUGUI errorText;

        [Header("UI - Loading")]
        [SerializeField] private GameObject loadingPanel;

        private void Start()
        {
            if (loginButton != null)
            {
                loginButton.onClick.AddListener(OnLoginButtonClicked);
            }

            if (loadingPanel != null)
            {
                loadingPanel.SetActive(false);
            }

            if (errorText != null)
            {
                errorText.text = "";
            }
        }

        private async void OnLoginButtonClicked()
        {
            string username = usernameInput?.text ?? "";
            string email = emailInput?.text ?? "";
            string password = passwordInput?.text ?? "";

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowError("メールアドレスとパスワードを入力してください");
                return;
            }

            SetLoading(true);

            var response = await ApiClient.Instance.Login(username, email, password);

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
        }

        private void ShowError(string message)
        {
            if (errorText != null)
            {
                errorText.text = message;
            }
            Debug.LogWarning($"Login Error: {message}");
        }
    }
}
