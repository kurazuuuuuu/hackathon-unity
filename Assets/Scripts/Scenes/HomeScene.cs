using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.System;
using Game.Network;

namespace Game.Scenes
{
    /// <summary>
    /// ホーム画面（メインメニュー）
    /// </summary>
    public class HomeScene : MonoBehaviour
    {
        [Header("UI - Navigation")]
        [SerializeField] private Button battleButton; // Matching
        [SerializeField] private Button botBattleButton;
        [SerializeField] private Button deckEditButton;
        [SerializeField] private Button gachaButton;

        [Header("UI - Header")]
        [SerializeField] private Button settingsButton;

        [Header("UI - Settings")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Button closeSettingsButton;
        [SerializeField] private TextMeshProUGUI usernameText;
        [SerializeField] private Button logoutButton;

        private async void Start()
        {
            SetupNavigation();
            SetupSettings();

            // 初期化時にユーザー名を表示（データを取得）
            if (ApiClient.Instance != null)
            {
                await ApiClient.Instance.FetchUserData();
            }

            if (usernameText != null && ApiClient.Instance != null && ApiClient.Instance.UserData != null)
            {
                usernameText.text = ApiClient.Instance.UserData.UserName;
            }
        }

        private void SetupNavigation()
        {
            if (battleButton != null)
            {
                battleButton.onClick.AddListener(OnBattleButtonClicked);
            }

            if (botBattleButton != null)
            {
                botBattleButton.onClick.AddListener(OnBotBattleButtonClicked);
            }

            if (deckEditButton != null)
            {
                deckEditButton.onClick.AddListener(OnDeckEditButtonClicked);
            }

            if (gachaButton != null)
            {
                gachaButton.onClick.AddListener(OnGachaButtonClicked);
            }
        }

        private void SetupSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OnSettingsButtonClicked);
            }

            if (closeSettingsButton != null)
            {
                closeSettingsButton.onClick.AddListener(OnCloseSettingsButtonClicked);
            }

            if (logoutButton != null)
            {
                logoutButton.onClick.AddListener(OnLogoutButtonClicked);
            }
        }

        #region Navigation Events

        private async void OnBattleButtonClicked()
        {
            // マッチング対戦へ
            await SceneController.Instance.GoToMatching();
        }

        private async void OnBotBattleButtonClicked()
        {
            // Bot対戦へ（現在は同一のBattleシーン等の想定、仕様に合わせて調整）
             await SceneController.Instance.GoToBattle();
        }

        private async void OnDeckEditButtonClicked()
        {
            await SceneController.Instance.GoToDeckEdit();
        }

        private async void OnGachaButtonClicked()
        {
            // ガチャ画面へ
            await SceneController.Instance.GoToGacha();
        }

        #endregion

        #region Settings Events

        private void OnSettingsButtonClicked()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
                
                // パネルを開いた時に最新のユーザー名を表示更新
                if (usernameText != null && ApiClient.Instance != null && ApiClient.Instance.UserData != null)
                {
                    usernameText.text = ApiClient.Instance.UserData.UserName;
                }
            }
        }

        private void OnCloseSettingsButtonClicked()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
        }

        private async void OnLogoutButtonClicked()
        {
            Network.ApiClient.Instance?.Logout();
            await SceneController.Instance.GoToTitle();
        }

        #endregion
    }
}
