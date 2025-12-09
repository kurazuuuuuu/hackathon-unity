using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.UI
{
    /// <summary>
    /// セーブ中などの処理中表示を管理するオーバーレイマネージャー
    /// シングルトンパターンでどこからでもアクセス可能
    /// 独自のCanvasを持ち、DontDestroyOnLoadで永続化
    /// </summary>
    public class SaveOverlayManager : MonoBehaviour
    {
        public static SaveOverlayManager Instance { get; private set; }

        private Canvas _canvas;
        private GameObject _overlayPanel;
        private TextMeshProUGUI _messageText;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                
                // 常に独自のCanvasを生成
                CreateOverlayCanvas();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// オーバーレイ用のCanvasとUIを作成
        /// </summary>
        private void CreateOverlayCanvas()
        {
            // Canvas作成（自分自身にアタッチ）
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 9999; // 最前面に表示
            
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            gameObject.AddComponent<GraphicRaycaster>();

            // オーバーレイパネル (暗い背景)
            _overlayPanel = new GameObject("OverlayPanel");
            _overlayPanel.transform.SetParent(transform, false);
            
            var panelImage = _overlayPanel.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.7f);
            panelImage.raycastTarget = true; // 入力をブロック
            
            var panelRect = _overlayPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // メッセージテキスト
            var textGO = new GameObject("MessageText");
            textGO.transform.SetParent(_overlayPanel.transform, false);
            
            _messageText = textGO.AddComponent<TextMeshProUGUI>();
            _messageText.text = "保存中...";
            _messageText.fontSize = 48;
            _messageText.alignment = TextAlignmentOptions.Center;
            _messageText.color = Color.white;
            
            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            // 初期状態は非表示
            _overlayPanel.SetActive(false);
            
            Debug.Log("[SaveOverlayManager] Overlay Canvas created.");
        }

        /// <summary>
        /// オーバーレイを表示
        /// </summary>
        /// <param name="message">表示するメッセージ (デフォルト: "保存中...")</param>
        public void Show(string message = "保存中...")
        {
            if (_overlayPanel != null)
            {
                if (_messageText != null)
                {
                    _messageText.text = message;
                }
                _overlayPanel.SetActive(true);
                Debug.Log($"[SaveOverlayManager] Show: {message}");
            }
        }

        /// <summary>
        /// オーバーレイを非表示
        /// </summary>
        public void Hide()
        {
            if (_overlayPanel != null)
            {
                _overlayPanel.SetActive(false);
                Debug.Log("[SaveOverlayManager] Hide");
            }
        }

        /// <summary>
        /// オーバーレイが表示中かどうか
        /// </summary>
        public bool IsShowing => _overlayPanel != null && _overlayPanel.activeSelf;
    }
}
