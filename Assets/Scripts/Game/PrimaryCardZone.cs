using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// デッキの主力資格カード（3枚）を表示・管理するゾーン
    /// </summary>
    public class PrimaryCardZone : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int maxSlots = 3;

        [Header("Card Slots")]
        [SerializeField] private List<Transform> cardSlots = new List<Transform>();

        [Header("References")]
        [SerializeField] private CardManager cardManager;

        // 配置されているカード
        private List<Card> placedCards = new List<Card>();

        // イベント
        public event Action<Card, int> OnCardPlaced;      // (カード, スロット番号)
        public event Action<Card, int> OnCardRemoved;     // (カード, スロット番号)
        public event Action OnZoneFull;                   // ゾーンが満杯になった時

        public int PlacedCardCount => placedCards.Count;
        public bool IsFull => placedCards.Count >= maxSlots;
        public IReadOnlyList<Card> PlacedCards => placedCards;

        private void Awake()
        {
            // スロットが設定されていない場合は自動生成
            if (cardSlots.Count == 0)
            {
                for (int i = 0; i < maxSlots; i++)
                {
                    cardSlots.Add(transform);
                }
            }
        }

        /// <summary>
        /// カードIDからカードを配置
        /// </summary>
        public bool PlaceCardById(string cardId)
        {
            if (IsFull)
            {
                Debug.LogWarning("主力カードゾーンが満杯です");
                return false;
            }

            if (cardManager == null)
            {
                cardManager = FindAnyObjectByType<CardManager>();
            }

            CardData data = cardManager?.GetCardData(cardId);
            if (data == null) return false;

            // 主力カードかチェック
            if (data.CardType != CardType.Primary)
            {
                Debug.LogWarning($"{data.CardName} は主力カードではありません");
                return false;
            }

            Card card = cardManager.SpawnCard(data);
            return PlaceCard(card);
        }

        /// <summary>
        /// カードを配置
        /// </summary>
        public bool PlaceCard(Card card)
        {
            if (card == null) return false;

            if (IsFull)
            {
                Debug.LogWarning("主力カードゾーンが満杯です");
                return false;
            }

            int slotIndex = placedCards.Count;
            Transform slot = slotIndex < cardSlots.Count ? cardSlots[slotIndex] : transform;

            // カードを配置
            card.transform.SetParent(slot);
            card.transform.localPosition = Vector3.zero;
            card.transform.localScale = Vector3.one;

            // ドラッグを無効化（表示専用）
            card.DisableDrag();

            placedCards.Add(card);
            OnCardPlaced?.Invoke(card, slotIndex);

            if (IsFull)
            {
                OnZoneFull?.Invoke();
            }

            Debug.Log($"主力カードを配置: {card.Name} (スロット {slotIndex + 1}/{maxSlots})");
            return true;
        }

        /// <summary>
        /// 指定スロットのカードを取り除く
        /// </summary>
        public Card RemoveCardAt(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= placedCards.Count) return null;

            Card removed = placedCards[slotIndex];
            placedCards.RemoveAt(slotIndex);

            OnCardRemoved?.Invoke(removed, slotIndex);

            // 残りのカードを前にシフト
            ReorganizeCards();

            return removed;
        }

        /// <summary>
        /// 全カードを取り除く
        /// </summary>
        public void Clear()
        {
            for (int i = placedCards.Count - 1; i >= 0; i--)
            {
                Card card = placedCards[i];
                placedCards.RemoveAt(i);
                if (card != null)
                {
                    Destroy(card.gameObject);
                }
            }
        }

        /// <summary>
        /// カードを再配置（スロット位置を整理）
        /// </summary>
        private void ReorganizeCards()
        {
            for (int i = 0; i < placedCards.Count; i++)
            {
                Transform slot = i < cardSlots.Count ? cardSlots[i] : transform;
                placedCards[i].transform.SetParent(slot);
                placedCards[i].transform.localPosition = Vector3.zero;
            }
        }

        /// <summary>
        /// 配置カードのIDリストを取得（セーブ用）
        /// </summary>
        public List<string> GetPlacedCardIds()
        {
            List<string> ids = new List<string>();
            foreach (var card in placedCards)
            {
                // CardDataからIDを取得する必要がある場合は別途実装
                ids.Add(card.Name); // 暫定でNameを使用
            }
            return ids;
        }
    }
}
