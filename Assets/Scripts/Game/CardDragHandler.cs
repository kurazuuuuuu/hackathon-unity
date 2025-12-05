using UnityEngine;
using UnityEngine.EventSystems;

namespace Game
{
    /// <summary>
    /// カードのドラッグ操作を処理するコンポーネント
    /// </summary>
    [RequireComponent(typeof(Card))]
    public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Settings")]
        [SerializeField] private float dragScale = 1.1f;
        [SerializeField] private float dragAlpha = 0.8f;

        private Card card;
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Canvas canvas;

        private Vector3 originalPosition;
        private Vector3 originalScale;
        private Transform originalParent;

        private void Awake()
        {
            card = GetComponent<Card>();
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();

            // CanvasGroupがなければ追加
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            // 親Canvasを取得
            canvas = GetComponentInParent<Canvas>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!card.IsDraggable) return;
            // ポインターダウン時の処理（必要に応じて）
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // ポインターアップ時の処理（必要に応じて）
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!card.IsDraggable)
            {
                eventData.pointerDrag = null;
                return;
            }

            // ドラッグ開始
            card.StartDrag();

            // 元の位置と親を保存
            originalPosition = rectTransform.position;
            originalScale = rectTransform.localScale;
            originalParent = transform.parent;

            // ドラッグ中の見た目を変更
            rectTransform.localScale = originalScale * dragScale;
            canvasGroup.alpha = dragAlpha;
            canvasGroup.blocksRaycasts = false;

            // 最前面に表示するためにCanvasの直下に移動
            if (canvas != null)
            {
                transform.SetParent(canvas.transform);
            }
            transform.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!card.IsDragging) return;

            // マウス/タッチ位置に追従
            if (canvas != null)
            {
                rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
            }
            else
            {
                rectTransform.position += (Vector3)eventData.delta;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!card.IsDragging) return;

            // ドラッグ終了
            card.EndDrag();

            // 見た目を元に戻す
            rectTransform.localScale = originalScale;
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;

            // ドロップ先が見つからなかった場合は元の位置に戻す
            if (!IsDroppedOnValidTarget(eventData))
            {
                ReturnToOriginalPosition();
            }
        }

        /// <summary>
        /// 有効なドロップ先にドロップされたかチェック
        /// </summary>
        private bool IsDroppedOnValidTarget(PointerEventData eventData)
        {
            // ドロップ先の判定ロジック
            // IDropHandlerを実装したオブジェクトがあればtrueを返す
            return eventData.pointerEnter != null && 
                   eventData.pointerEnter.GetComponent<IDropHandler>() != null;
        }

        /// <summary>
        /// 元の位置に戻す
        /// </summary>
        public void ReturnToOriginalPosition()
        {
            transform.SetParent(originalParent);
            rectTransform.position = originalPosition;
        }
    }
}
