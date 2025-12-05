using UnityEngine;
using Game.Network;
using Game.Multiplayer;

namespace Game.System
{
    /// <summary>
    /// ゲーム起動時に永続オブジェクトを初期化するローダー
    /// Boot シーンに配置
    /// </summary>
    public class BootLoader : MonoBehaviour
    {
        [Header("Prefabs")] 
        [SerializeField] private GameObject sceneControllerPrefab;
        [SerializeField] private GameObject framerateLimitterPrefab;
        [SerializeField] private GameObject cursorEffectManagerPrefab;
        [SerializeField] private GameObject apiClientPrefab;
        [SerializeField] private GameObject matchmakingManagerPrefab;
        [SerializeField] private GameObject networkManagerPrefab;

        [Header("Settings")]
        [SerializeField] private bool autoTransitionToTitle = true;

        private async void Start()
        {
            Debug.Log("=== Boot 開始 ===");

            // 永続オブジェクトを生成
            InitializePersistentObjects();

            // API クライアントの初期化を待機
            if (ApiClient.Instance != null)
            {
                while (!ApiClient.Instance.IsServerAvailable)
                {
                    await global::System.Threading.Tasks.Task.Yield();
                }
            }

            Debug.Log("=== Boot 完了 ===");

            // タイトルへ自動遷移
            if (autoTransitionToTitle && SceneController.Instance != null)
            {
                await SceneController.Instance.GoToTitle();
            }
        }

        private void InitializePersistentObjects()
        {
            // SceneController
            if (SceneController.Instance == null && sceneControllerPrefab != null)
            {
                Instantiate(sceneControllerPrefab);
            }

            // ApiClient
            if (ApiClient.Instance == null && apiClientPrefab != null)
            {
                Instantiate(apiClientPrefab);
            }

            // MatchmakingManager
            if (MatchmakingManager.Instance == null && matchmakingManagerPrefab != null)
            {
                Instantiate(matchmakingManagerPrefab);
            }

            // NetworkManager
            if (networkManagerPrefab != null && FindAnyObjectByType<Unity.Netcode.NetworkManager>() == null)
            {
                Instantiate(networkManagerPrefab);
            }
        }
    }
}
