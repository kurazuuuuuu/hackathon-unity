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
        
        /// <summary>
        /// このゾーンの所有者（プレイヤー）
        /// </summary>
        public Battle.Player Owner { get; private set; }
        
        /// <summary>
        /// 所有者を設定
        /// </summary>
        public void SetOwner(Battle.Player owner)
        {
            Owner = owner;
        }

        // 配置されているカード
        private List<CardBase> placedCards = new List<CardBase>();

        // イベント
        public event Action<CardBase, int> OnCardPlaced;      // (カード, スロット番号)
        public event Action<CardBase, int> OnCardRemoved;     // (カード, スロット番号)
        public event Action OnZoneFull;                   // ゾーンが満杯になった時

        public int PlacedCardCount => placedCards.Count;
        public bool IsFull => placedCards.Count >= maxSlots;
        public IReadOnlyList<CardBase> PlacedCards => placedCards;

        private void Awake()
        {
            // スロットが設定されていない場合
            if (cardSlots.Count == 0)
            {
                // 子オブジェクトを検索してスロットとしてリストに追加
                foreach (Transform child in transform)
                {
                    cardSlots.Add(child);
                }

                // それでもスロットがない場合は、実行時にスロットを生成
                if (cardSlots.Count == 0)
                {
                    Debug.Log("スロットが見つからないため、自動生成します");

                    // Check if this is Player 2 (Opponent) based on naming convention
                    bool isP2 = transform.parent != null && transform.parent.name.Contains("Player2");
                    
                    float spacing = 380f; // Increased from 160
                    float centerOffset = 40f; 

                    for (int i = 0; i < maxSlots; i++)
                    {
                        var slotGO = new GameObject($"Slot_{i}");
                        slotGO.transform.SetParent(this.transform, false);
                        
                        var rect = slotGO.AddComponent<RectTransform>();
                        
                        // i=0 (Left), i=1 (Center), i=2 (Right)
                        float xPos = (i - 1) * spacing;
                        float yPos = 0;

                        // Center Card Logic
                        if (i == 1)
                        {
                            // P2: Shift Down (-), P1: Shift Up (+)
                            // Note: If P2 Zone is rotated 180, then "Up" is "Screen Down".
                            // But current PlayerHUDOrganism doesn't execute rotation on PrimaryZone.
                            // Assuming NO rotation on parent:
                            yPos = isP2 ? -centerOffset : centerOffset;
                        }

                        rect.anchoredPosition = new Vector2(xPos, yPos);
                        
                        cardSlots.Add(slotGO.transform);
                    }
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

            CardDataBase data = cardManager?.GetCardData(cardId);
            if (data == null) return false;

            // 主力カードかチェック
            if (data.CardType != CardType.Primary)
            {
                Debug.LogWarning($"{data.CardName} は主力カードではありません");
                return false;
            }

            Card card = cardManager.SpawnCard(data);
            if (card != null)
            {
                var cardBase = card.GetComponent<CardBase>();
                return PlaceCard(cardBase);
            }
            return false;
        }

        /// <summary>
        /// カードを配置
        /// </summary>
        public bool PlaceCard(CardBase card)
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
            
            // Cardコンポーネント（旧）がついている場合、そちらもドラッグ無効化
            var legacyCard = card.GetComponent<Card>();
            if (legacyCard != null)
            {
                legacyCard.DisableDrag();
            }
            
            // CardDragHandlerがついている場合、コンポーネントごと無効化
            var dragHandler = card.GetComponent<CardDragHandler>();
            if (dragHandler != null)
            {
                dragHandler.enabled = false;
            }

            placedCards.Add(card);
            
            // Set owner for primary cards
            var primaryCard = card as PrimaryCard;
            if (primaryCard != null && Owner != null)
            {
                primaryCard.SetOwner(Owner);
            }
            
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
        public CardBase RemoveCardAt(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= placedCards.Count) return null;

            CardBase removed = placedCards[slotIndex];
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
                CardBase card = placedCards[i];
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
