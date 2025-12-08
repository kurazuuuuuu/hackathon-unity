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
        [SerializeField] private string resourcesPath = "Cards"; // If empty, loads from root
        [SerializeField] private List<CardDataBase> allCards = new List<CardDataBase>();

        // カードIDでの高速検索用辞書
        private Dictionary<string, CardDataBase> cardDictionary = new Dictionary<string, CardDataBase>();
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
        /// Resourcesフォルダからカードを自動読み込み (CardDataBase型)
        /// </summary>
        [ContextMenu("Load Cards")]
        private void LoadCardsFromResources()
        {
            // パス指定なしでResources以下の全CardDataBaseを読み込む（サブフォルダ含む）
            string path = string.IsNullOrEmpty(resourcesPath) ? "" : resourcesPath;
            CardDataBase[] loadedCards = Resources.LoadAll<CardDataBase>(path);
            
            allCards.Clear();
            allCards.AddRange(loadedCards);
            Debug.Log($"Resourcesからカードを読み込み: {loadedCards.Length} 枚 (Path: {path}, Type: CardDataBase)");
        }
        
        /// <summary>
        /// ランダムな主力カードを取得
        /// </summary>
        public CardDataBase GetRandomPrimaryCard()
        {
            EnsureInitialized();
            var primaryCards = allCards.FindAll(c => c.CardType == CardType.Primary);
            if (primaryCards.Count == 0)
            {
                Debug.LogWarning("主力カード (Primary) が見つかりません");
                return null;
            }
            return primaryCards[Random.Range(0, primaryCards.Count)];
        }

        /// <summary>
        /// ランダムなサポート・特殊カードを取得
        /// </summary>
        public CardDataBase GetRandomSupportCard()
        {
            EnsureInitialized();
            var supportCards = allCards.FindAll(c => c.CardType == CardType.Support || c.CardType == CardType.Special);
            if (supportCards.Count == 0)
            {
                Debug.LogWarning("サポート・特殊カードが見つかりません");
                return null;
            }
            return supportCards[Random.Range(0, supportCards.Count)];
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
        /// カードIDからCardDataBaseを取得
        /// </summary>
        public CardDataBase GetCardData(string cardId)
        {
            EnsureInitialized();
            if (cardDictionary.TryGetValue(cardId, out CardDataBase data))
            {
                return data;
            }
            Debug.LogError($"カードID {cardId} が見つかりません");
            return null;
        }

        /// <summary>
        /// カードIDを指定してカードをスポーン（親指定）
        /// </summary>
        public Card SpawnCard(string cardId, Transform parent = null)
        {
            EnsureInitialized();
            CardDataBase data = GetCardData(cardId);
            if (data == null) return null;

            return SpawnCard(data, parent);
        }

        /// <summary>
        /// CardDataBaseを指定してカードをスポーン（親指定可能）
        /// </summary>
        public Card SpawnCard(CardDataBase data, Transform parent = null)
        {
            EnsureInitialized();

            // Try to load type-specific prefab using CardHelper
            string prefabPath = CardHelper.GetCardPrefabPath(data.CardId);
            GameObject prefabObj = Resources.Load<GameObject>(prefabPath);
            
            Card prefabToUse = null;

            if (prefabObj != null)
            {
                prefabToUse = prefabObj.GetComponent<Card>();
            }

            // Fallback to inspector reference
            if (prefabToUse == null)
            {
                prefabToUse = cardPrefab;
            }

            if (prefabToUse == null)
            {
                Debug.LogError($"カードPrefabが見つかりません (Path: {prefabPath}, Inspector: {cardPrefab})");
                return null;
            }

            Transform spawnParent = parent != null ? parent : (cardSpawnParent != null ? cardSpawnParent : transform);
            Card newCard = Instantiate(prefabToUse, spawnParent);
            
            // Z位置をリセット
            RectTransform rt = newCard.GetComponent<RectTransform>();
            if (rt != null)
            {
                Vector3 pos = rt.anchoredPosition3D;
                pos.z = 0;
                rt.anchoredPosition3D = pos;
            }
            
            // Card component initialization
            newCard.Initialize(data);
            
            // CardBase component initialization (if exists, e.g., PrimaryCard)
            var cardBase = newCard.GetComponent<CardBase>();
            if (cardBase != null && !(newCard is CardBase))
            {
                cardBase.Initialize(data);
            }
            
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
        /// ランダムなCardDataBaseを取得
        /// </summary>
        public CardDataBase GetRandomCardData()
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

