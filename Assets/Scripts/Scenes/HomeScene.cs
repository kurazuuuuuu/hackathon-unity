using UnityEngine;
using UnityEngine.UI;
using Game.System;

namespace Game.Scenes
{
    /// <summary>
    /// ホーム画面（メインメニュー）
    /// </summary>
    public class HomeScene : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Button battleButton;
        [SerializeField] private Button deckEditButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button logoutButton;

        private void Start()
        {
            if (battleButton != null)
            {
                battleButton.onClick.AddListener(OnBattleButtonClicked);
            }

            if (deckEditButton != null)
            {
                deckEditButton.onClick.AddListener(OnDeckEditButtonClicked);
            }

            if (logoutButton != null)
            {
                logoutButton.onClick.AddListener(OnLogoutButtonClicked);
            }
        }

        private async void OnBattleButtonClicked()
        {
            await SceneController.Instance.GoToMatching();
        }

        private async void OnDeckEditButtonClicked()
        {
            await SceneController.Instance.GoToDeckEdit();
        }

        private async void OnLogoutButtonClicked()
        {
            Network.ApiClient.Instance?.Logout();
            await SceneController.Instance.GoToTitle();
        }
    }
}
