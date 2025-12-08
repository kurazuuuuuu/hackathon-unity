using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.System
{
    /// <summary>
    /// シーン遷移を管理するコントローラー
    /// </summary>
    public class SceneController : MonoBehaviour
    {
        private static SceneController _instance;
        public static SceneController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<SceneController>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("SceneController");
                        _instance = go.AddComponent<SceneController>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [Header("Settings")]
        [SerializeField] private float minLoadingTime = 0.5f;
        [SerializeField] private float bgmFadeDuration = 0.5f;

        // 状態
        public bool IsLoading { get; private set; }
        public string CurrentScene => SceneManager.GetActiveScene().name;

        // イベント
        public event Action<string> OnSceneLoadStart;
        public event Action<string> OnSceneLoadComplete;
        public event Action<float> OnLoadingProgress;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// シーンを読み込む
        /// </summary>
        public async Task LoadScene(string sceneName)
        {
            if (IsLoading)
            {
                Debug.LogWarning("既にシーンを読み込み中です");
                return;
            }

            IsLoading = true;
            OnSceneLoadStart?.Invoke(sceneName);
            Debug.Log($"シーン読み込み開始: {sceneName}");

            float startTime = Time.time;

            // BGMをフェードアウト
            if (BGMManager.Instance != null && BGMManager.Instance.IsPlaying)
            {
                BGMManager.Instance.StopBGM(bgmFadeDuration);
                // フェード完了を待機
                await Task.Delay((int)(bgmFadeDuration * 1000));
            }

            // 非同期読み込み
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f)
            {
                OnLoadingProgress?.Invoke(operation.progress);
                await Task.Yield();
            }

            // 最小ローディング時間を確保
            float elapsed = Time.time - startTime;
            if (elapsed < minLoadingTime)
            {
                await Task.Delay((int)((minLoadingTime - elapsed) * 1000));
            }

            operation.allowSceneActivation = true;

            // シーンがアクティブになるまで待機
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            IsLoading = false;
            OnSceneLoadComplete?.Invoke(sceneName);
            Debug.Log($"シーン読み込み完了: {sceneName}");
        }

        /// <summary>
        /// タイトルへ
        /// </summary>
        public async Task GoToTitle()
        {
            await LoadScene(SceneNames.Title);
        }

        /// <summary>
        /// ログインへ
        /// </summary>
        public async Task GoToLogin()
        {
            await LoadScene(SceneNames.Login);
        }

        /// <summary>
        /// ホームへ
        /// </summary>
        public async Task GoToHome()
        {
            await LoadScene(SceneNames.Home);
        }

        /// <summary>
        /// デッキ編集へ
        /// </summary>
        public async Task GoToDeckEdit()
        {
            await LoadScene(SceneNames.DeckEdit);
        }

        /// <summary>
        /// マッチングへ
        /// </summary>
        public async Task GoToMatching()
        {
            await LoadScene(SceneNames.Matching);
        }

        /// <summary>
        /// バトルへ
        /// </summary>
        public async Task GoToBattle()
        {
            await LoadScene(SceneNames.Battle);
        }

        /// <summary>
        /// ガチャへ
        /// </summary>
        public async Task GoToGacha()
        {
            await LoadScene(SceneNames.Capsule);
        }
    }
}
