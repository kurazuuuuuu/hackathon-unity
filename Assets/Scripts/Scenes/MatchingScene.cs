using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.System;
using Game.Multiplayer;

namespace Game.Scenes
{
    /// <summary>
    /// マッチング画面
    /// </summary>
    public class MatchingScene : MonoBehaviour
    {
        [Header("UI - Create/Join")]
        [SerializeField] private Button createLobbyButton;
        [SerializeField] private Button joinLobbyButton;
        [SerializeField] private TMP_InputField joinCodeInput;

        [Header("UI - Waiting")]
        [SerializeField] private GameObject waitingPanel;
        [SerializeField] private TextMeshProUGUI lobbyCodeText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button startButton;

        [Header("UI - Back")]
        [SerializeField] private Button backButton;

        private void Start()
        {
            // ボタン設定
            createLobbyButton?.onClick.AddListener(OnCreateLobbyClicked);
            joinLobbyButton?.onClick.AddListener(OnJoinLobbyClicked);
            cancelButton?.onClick.AddListener(OnCancelClicked);
            startButton?.onClick.AddListener(OnStartClicked);
            backButton?.onClick.AddListener(OnBackClicked);

            // イベント購読
            if (MatchmakingManager.Instance != null)
            {
                MatchmakingManager.Instance.OnPlayerJoined += OnPlayerJoined;
                MatchmakingManager.Instance.OnMatchStart += OnMatchStart;
            }

            // 初期状態
            SetWaitingPanelVisible(false);
            if (startButton != null) startButton.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (MatchmakingManager.Instance != null)
            {
                MatchmakingManager.Instance.OnPlayerJoined -= OnPlayerJoined;
                MatchmakingManager.Instance.OnMatchStart -= OnMatchStart;
            }
        }

        private async void OnCreateLobbyClicked()
        {
            string code = await MatchmakingManager.Instance.CreateLobby();
            if (!string.IsNullOrEmpty(code))
            {
                SetWaitingPanelVisible(true);
                if (lobbyCodeText != null) lobbyCodeText.text = $"コード: {code}";
                if (statusText != null) statusText.text = "対戦相手を待っています...";
                if (startButton != null) startButton.gameObject.SetActive(false);
            }
        }

        private async void OnJoinLobbyClicked()
        {
            string code = joinCodeInput?.text ?? "";
            if (string.IsNullOrEmpty(code))
            {
                Debug.LogWarning("コードを入力してください");
                return;
            }

            bool success = await MatchmakingManager.Instance.JoinLobby(code);
            if (success)
            {
                SetWaitingPanelVisible(true);
                if (lobbyCodeText != null) lobbyCodeText.text = $"コード: {code}";
                if (statusText != null) statusText.text = "ホストの開始を待っています...";
            }
        }

        private async void OnCancelClicked()
        {
            await MatchmakingManager.Instance.LeaveLobby();
            SetWaitingPanelVisible(false);
        }

        private async void OnStartClicked()
        {
            await MatchmakingManager.Instance.StartMatch();
        }

        private async void OnBackClicked()
        {
            await SceneController.Instance.GoToHome();
        }

        private void OnPlayerJoined()
        {
            if (statusText != null) statusText.text = "対戦相手が見つかりました！";
            if (startButton != null && MatchmakingManager.Instance.IsHost)
            {
                startButton.gameObject.SetActive(true);
            }
        }

        private async void OnMatchStart()
        {
            await SceneController.Instance.GoToBattle();
        }

        private void SetWaitingPanelVisible(bool visible)
        {
            if (waitingPanel != null) waitingPanel.SetActive(visible);
            if (createLobbyButton != null) createLobbyButton.gameObject.SetActive(!visible);
            if (joinLobbyButton != null) joinLobbyButton.gameObject.SetActive(!visible);
            if (joinCodeInput != null) joinCodeInput.gameObject.SetActive(!visible);
        }
    }
}
