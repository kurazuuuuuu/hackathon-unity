using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// カードの管理とスポーンを行うマネージャー
    /// </summary>
    public class CardManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Card cardPrefab;
        [SerializeField] private Transform cardSpawnParent;

        [Header("Card Database")]
        [SerializeField] private List<CardData> allCards = new List<CardData>();

        // カードIDでの高速検索用辞書
        private Dictionary<string, CardData> cardDictionary = new Dictionary<string, CardData>();

        private void Awake()
        {
            BuildCardDictionary();
        }

        /// <summary>
        /// カードリストから辞書を構築
        /// </summary>
        private void BuildCardDictionary()
        {
            cardDictionary.Clear();
            foreach (var cardData in allCards)
            {
                if (cardData == null) continue;

                if (cardDictionary.ContainsKey(cardData.CardId))
                {
                    Debug.LogWarning($"重複したカードID: {cardData.CardId} ({cardData.CardName})");
                    continue;
                }
                cardDictionary.Add(cardData.CardId, cardData);
            }
            Debug.Log($"カード辞書を構築: {cardDictionary.Count} 枚");
        }

        /// <summary>
        /// カードIDからCardDataを取得
        /// </summary>
        public CardData GetCardData(string cardId)
        {
            if (cardDictionary.TryGetValue(cardId, out CardData data))
            {
                return data;
            }
            Debug.LogError($"カードID {cardId} が見つかりません");
            return null;
        }

        /// <summary>
        /// カードIDを指定してカードをスポーン
        /// </summary>
        public Card SpawnCard(string cardId)
        {
            CardData data = GetCardData(cardId);
            if (data == null) return null;

            return SpawnCard(data);
        }

        /// <summary>
        /// CardDataを指定してカードをスポーン
        /// </summary>
        public Card SpawnCard(CardData data)
        {
            if (cardPrefab == null)
            {
                Debug.LogError("カードPrefabが設定されていません");
                return null;
            }

            Transform parent = cardSpawnParent != null ? cardSpawnParent : transform;
            Card newCard = Instantiate(cardPrefab, parent);
            newCard.Initialize(data);
            return newCard;
        }

        /// <summary>
        /// 複数のカードをスポーン
        /// </summary>
        public List<Card> SpawnCards(string[] cardIds)
        {
            List<Card> spawnedCards = new List<Card>();
            foreach (string id in cardIds)
            {
                Card card = SpawnCard(id);
                if (card != null)
                {
                    spawnedCards.Add(card);
                }
            }
            return spawnedCards;
        }
    }
}

