using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game
{
    /// <summary>
    /// カードのドロップ先となるゾーン
    /// </summary>
    public class CardDropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Settings")]
        [SerializeField] private CardType acceptedCardType = CardType.Primary;
        [SerializeField] private bool acceptAnyType = false;
        [SerializeField] private int maxCards = 1;

        [Header("Visual Feedback")]
        [SerializeField] private Color highlightColor = new Color(0.5f, 1f, 0.5f, 0.5f);
        [SerializeField] private GameObject highlightObject;

        // 現在配置されているカード
        private Card currentCard;
        private int cardCount = 0;

        // イベント
        public event Action<Card> OnCardDropped;
        public event Action<Card> OnCardRemoved;

        public bool HasCard => currentCard != null || cardCount > 0;
        public Card CurrentCard => currentCard;

        public void OnPointerEnter(PointerEventData eventData)
        {
            // ドラッグ中のカードがある場合、ハイライト表示
            if (eventData.pointerDrag != null)
            {
                Card draggedCard = eventData.pointerDrag.GetComponent<Card>();
                if (draggedCard != null && CanAcceptCard(draggedCard))
                {
                    ShowHighlight(true);
                }
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ShowHighlight(false);
        }

        public void OnDrop(PointerEventData eventData)
        {
            ShowHighlight(false);

            if (eventData.pointerDrag == null) return;

            Card droppedCard = eventData.pointerDrag.GetComponent<Card>();
            if (droppedCard == null) return;

            if (!CanAcceptCard(droppedCard))
            {
                Debug.Log($"このゾーンには {droppedCard.Name} を配置できません");
                return;
            }

            // カードを配置
            PlaceCard(droppedCard);
        }

        /// <summary>
        /// カードを受け入れ可能かチェック
        /// </summary>
        public bool CanAcceptCard(Card card)
        {
            if (cardCount >= maxCards) return false;
            if (acceptAnyType) return true;

            // CardDataからCardTypeを取得する必要がある場合は別途実装
            // 現在はacceptAnyTypeで制御
            return true;
        }

        /// <summary>
        /// カードを配置
        /// </summary>
        public void PlaceCard(Card card)
        {
            if (card == null) return;

            // カードをこのゾーンの子にする
            card.transform.SetParent(transform);
            card.transform.localPosition = Vector3.zero;

            // ドラッグを無効化（配置後は動かせないようにする場合）
            // card.DisableDrag();

            currentCard = card;
            cardCount++;

            OnCardDropped?.Invoke(card);
            Debug.Log($"{card.Name} を配置しました");
        }

        /// <summary>
        /// カードを取り除く
        /// </summary>
        public Card RemoveCard()
        {
            if (currentCard == null) return null;

            Card removed = currentCard;
            currentCard = null;
            cardCount--;

            OnCardRemoved?.Invoke(removed);
            return removed;
        }

        /// <summary>
        /// ハイライト表示を切り替え
        /// </summary>
        private void ShowHighlight(bool show)
        {
            if (highlightObject != null)
            {
                highlightObject.SetActive(show);
            }
        }

        /// <summary>
        /// ゾーンをクリア
        /// </summary>
        public void Clear()
        {
            currentCard = null;
            cardCount = 0;
        }
    }
}
