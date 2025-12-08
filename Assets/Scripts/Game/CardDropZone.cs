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

            // Determine Target (if this zone represents a card, e.g. PrimaryCard)
            Card targetCard = GetComponentInChildren<Card>(); // If placed on a PrimaryCard slot which has a card
            if (targetCard == null) targetCard = GetComponent<Card>();
            
            // Get Player (assuming P1 for now, but ideally pass via context or find owner)
            // Simplified: Find Player1 from BattleManager
            var bm = FindAnyObjectByType<Game.Battle.BattleManager>();
            var player = bm?.Player1; // TODO: Determine correct player dynamically
            
            if (player != null)
            {
                 Game.Battle.CardAction.PlayCard(player, droppedCard, targetCard);
                 
                 // If success (Card moved to grave/removed from hand), visual update happens via CardAction
                 // But CardAction currently just removes from Hand List. 
                 // We need to destroy the card object or move it to grave queue.
                 Destroy(droppedCard.gameObject, 0.5f); // Temporary cleanup
            }
        }

        /// <summary>
        /// カードを受け入れ可能かチェック
        /// </summary>
        public bool CanAcceptCard(Card card)
        {
            if (card == null) return false;
            
            // Check Card Type vs This Zone
            // If this is a Primary Slot (Target), we accept Support cards.
            // If this is Field (Background), we accept Special cards.
            
            Card targetCard = GetComponentInChildren<Card>();
            if (targetCard != null && targetCard.Type == CardType.Primary)
            {
                // This is a Primary Card slot (or the card itself)
                return card.Type == CardType.Support;
            }
            
            // Otherwise assume it's a field play (Special)
            return card.Type == CardType.Special;
        }

        /// <summary>
        /// (Legacy) Visual Placement
        /// </summary>
        public void PlaceCard(Card card)
        {
           // Logic moved to OnDrop -> CardAction
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
