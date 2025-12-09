using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Game.UI;

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
        [SerializeField] private float highlightDistance = 200f; // ハイライト判定距離

        private Card card;
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Canvas canvas;

        private Vector3 originalPosition;
        private Vector3 originalScale;
        private Transform originalParent;
        private int originalSiblingIndex;

        // ターゲットハイライト用
        private CardHighlight currentHighlightedCard;
        private static List<CardBase> allPrimaryCards = new List<CardBase>();

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

            // 現在のプレイヤーのカードかチェック
            var bm = Battle.BattleManager.Instance;
            if (bm != null)
            {
                // 自分のターンかチェック
                if (bm.CurrentState != Battle.BattleState.PlayerTurn)
                {
                    Debug.Log("[CardDragHandler] 自分のターンではありません");
                    eventData.pointerDrag = null;
                    return;
                }
                
                // Player1（ローカルプレイヤー）のターンかチェック
                if (bm.CurrentPlayer != bm.Player1)
                {
                    Debug.Log("[CardDragHandler] 相手のターンです");
                    eventData.pointerDrag = null;
                    return;
                }
                
                // 手札に含まれているかで所有者を判定
                bool isMyCard = false;
                foreach (var handCard in bm.CurrentPlayer.Hand)
                {
                    if (handCard != null && handCard.gameObject == gameObject)
                    {
                        isMyCard = true;
                        break;
                    }
                }
                
                if (!isMyCard)
                {
                    Debug.Log("[CardDragHandler] 相手のカードはドラッグできません");
                    eventData.pointerDrag = null;
                    return;
                }
            }

            // ドラッグ開始
            card.StartDrag();

            // 元の位置と親を保存
            originalPosition = rectTransform.position;
            originalScale = rectTransform.localScale;
            originalParent = transform.parent;
            originalSiblingIndex = transform.GetSiblingIndex();

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

            // 主力カードリストを取得（ターゲットハイライト用）
            RefreshPrimaryCardsList();
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

            // ターゲットハイライト更新
            UpdateTargetHighlight();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!card.IsDragging) return;

            // ドラッグ終了
            card.EndDrag();

            // ハイライト解除
            ClearHighlight();

            // 見た目を元に戻す
            rectTransform.localScale = originalScale;
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;

            // 特殊カードの場合：有効なドロップ先がなくても場に出したら発動
            if (card.Type == CardType.Special)
            {
                // ドロップ先が見つからなくても発動
                if (!IsDroppedOnValidTarget(eventData))
                {
                    // BattleManagerでカードを使用
                    var bm = Battle.BattleManager.Instance;
                    if (bm != null && bm.CurrentPlayer != null)
                    {
                        Debug.Log($"[CardDragHandler] 特殊カード [{card.Name}] を場に出して発動");
                        bm.PlayCard(card, null);
                        Destroy(gameObject, 0.1f);
                        return;
                    }
                }
            }

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
        /// 元の位置に戻す（挿入位置を考慮）
        /// </summary>
        public void ReturnToOriginalPosition()
        {
            transform.SetParent(originalParent);
            
            // 挿入位置を計算
            int insertIndex = CalculateInsertIndex();
            transform.SetSiblingIndex(insertIndex);
            
            // レイアウト更新をトリガー
            var layoutController = originalParent.GetComponent<HandLayoutController>();
            if (layoutController != null)
            {
                layoutController.UpdateLayout();
            }
        }

        /// <summary>
        /// 挿入位置を計算
        /// </summary>
        private int CalculateInsertIndex()
        {
            if (originalParent == null) return originalSiblingIndex;

            float myX = rectTransform.position.x;
            int childCount = originalParent.childCount;
            
            for (int i = 0; i < childCount; i++)
            {
                Transform child = originalParent.GetChild(i);
                if (child == transform) continue;
                
                float childX = child.position.x;
                if (myX < childX)
                {
                    return i;
                }
            }
            
            return childCount; // 最後に追加
        }

        /// <summary>
        /// 主力カードリストを更新
        /// </summary>
        private void RefreshPrimaryCardsList()
        {
            allPrimaryCards.Clear();
            
            // BattleManagerから主力カードを取得
            var battleManager = Battle.BattleManager.Instance;
            if (battleManager != null)
            {
                if (battleManager.Player1 != null)
                    allPrimaryCards.AddRange(battleManager.Player1.PrimaryCardsInPlay);
                if (battleManager.Player2 != null)
                    allPrimaryCards.AddRange(battleManager.Player2.PrimaryCardsInPlay);
            }
        }

        /// <summary>
        /// ターゲットハイライトを更新
        /// </summary>
        private void UpdateTargetHighlight()
        {
            CardHighlight closestHighlight = null;
            float closestDistance = highlightDistance;

            foreach (var cardBase in allPrimaryCards)
            {
                if (cardBase == null) continue;
                
                var highlight = cardBase.GetComponent<CardHighlight>();
                if (highlight == null)
                {
                    // ハイライトコンポーネントがなければ追加
                    highlight = cardBase.gameObject.AddComponent<CardHighlight>();
                }

                float distance = Vector3.Distance(transform.position, cardBase.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestHighlight = highlight;
                }
            }

            // 前回のハイライトを解除
            if (currentHighlightedCard != null && currentHighlightedCard != closestHighlight)
            {
                currentHighlightedCard.SetHighlight(false);
            }

            // 新しいハイライトを設定
            if (closestHighlight != null)
            {
                closestHighlight.SetHighlight(true);
                currentHighlightedCard = closestHighlight;
            }
            else
            {
                currentHighlightedCard = null;
            }
        }

        /// <summary>
        /// ハイライトをクリア
        /// </summary>
        private void ClearHighlight()
        {
            if (currentHighlightedCard != null)
            {
                currentHighlightedCard.SetHighlight(false);
                currentHighlightedCard = null;
            }
        }
    }
}

