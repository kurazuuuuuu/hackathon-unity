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
        [SerializeField] private bool autoLoadFromResources = true;
        [SerializeField] private string resourcesPath = "Cards";
        [SerializeField] private List<CardData> allCards = new List<CardData>();

        // カードIDでの高速検索用辞書
        private Dictionary<string, CardData> cardDictionary = new Dictionary<string, CardData>();
        private bool isInitialized = false;

        private void Awake()
        {
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (isInitialized) return;

            if (autoLoadFromResources)
            {
                LoadCardsFromResources();
            }
            BuildCardDictionary();
            isInitialized = true;
        }

        /// <summary>
        /// Resourcesフォルダからカードを自動読み込み
        /// </summary>
        private void LoadCardsFromResources()
        {
            CardData[] loadedCards = Resources.LoadAll<CardData>(resourcesPath);
            allCards.Clear();
            allCards.AddRange(loadedCards);
            Debug.Log($"Resourcesからカードを読み込み: {loadedCards.Length} 枚");
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

                // CardIdが空の場合はアセット名（ファイル名）を使用
                string id = string.IsNullOrEmpty(cardData.CardId) ? cardData.name : cardData.CardId;

                if (cardDictionary.ContainsKey(id))
                {
                    Debug.LogWarning($"重複したカードID: {id} ({cardData.CardName})");
                    continue;
                }
                cardDictionary.Add(id, cardData);
            }
            Debug.Log($"カード辞書を構築: {cardDictionary.Count} 枚");
        }

        /// <summary>
        /// カードIDからCardDataを取得
        /// </summary>
        public CardData GetCardData(string cardId)
        {
            EnsureInitialized();
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
            EnsureInitialized();
            CardData data = GetCardData(cardId);
            if (data == null) return null;

            return SpawnCard(data);
        }

        /// <summary>
        /// CardDataを指定してカードをスポーン
        /// </summary>
        public Card SpawnCard(CardData data)
        {
            EnsureInitialized();
            if (cardPrefab == null)
            {
                Debug.LogError("カードPrefabが設定されていません");
                return null;
            }

            Transform parent = cardSpawnParent != null ? cardSpawnParent : transform;
            Card newCard = Instantiate(cardPrefab, parent);
            
            // Z位置をリセット（Prefabの位置がずれている場合の対策）
            RectTransform rt = newCard.GetComponent<RectTransform>();
            if (rt != null)
            {
                Vector3 pos = rt.anchoredPosition3D;
                pos.z = 0;
                rt.anchoredPosition3D = pos;
            }
            
            newCard.Initialize(data);
            return newCard;
        }

        /// <summary>
        /// 複数のカードをスポーン
        /// </summary>
        public List<Card> SpawnCards(string[] cardIds)
        {
            EnsureInitialized();
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

        /// <summary>
        /// ランダムなCardDataを取得
        /// </summary>
        public CardData GetRandomCardData()
        {
            EnsureInitialized();
            if (allCards.Count == 0)
            {
                Debug.LogError("カードが登録されていません");
                return null;
            }
            int randomIndex = Random.Range(0, allCards.Count);
            return allCards[randomIndex];
        }
    }
}

