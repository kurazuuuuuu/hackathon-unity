using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Data
{
    /// <summary>
    /// デッキデータを格納するモデル
    /// 主力3枚 + サポート・特殊20枚 = 合計23枚
    /// </summary>
    [Serializable]
    public class DeckData
    {
        public const int PRIMARY_CARD_COUNT = 3;
        public const int SUPPORT_CARD_COUNT = 20;
        public const int TOTAL_CARD_COUNT = PRIMARY_CARD_COUNT + SUPPORT_CARD_COUNT;

        public string DeckName;

        // 主力カード (3枚)
        public List<string> PrimaryCards = new List<string>();

        // サポート・特殊カード (20枚)
        public List<string> SupportCards = new List<string>();

        public DeckData()
        {
            DeckName = "新規デッキ";
        }

        public DeckData(string deckName)
        {
            DeckName = deckName;
        }

        /// <summary>
        /// デッキが有効かどうか（枚数チェック）
        /// </summary>
        public bool IsValid()
        {
            return PrimaryCards.Count == PRIMARY_CARD_COUNT &&
                   SupportCards.Count == SUPPORT_CARD_COUNT;
        }

        /// <summary>
        /// デッキの合計枚数を取得
        /// </summary>
        public int GetTotalCardCount()
        {
            return PrimaryCards.Count + SupportCards.Count;
        }

        /// <summary>
        /// 主力カードを追加
        /// </summary>
        public bool AddPrimaryCard(string cardId)
        {
            if (PrimaryCards.Count >= PRIMARY_CARD_COUNT)
            {
                Debug.LogWarning($"主力カードは最大{PRIMARY_CARD_COUNT}枚までです");
                return false;
            }
            
            // 重複チェック - 同じカードIDは1枚のみ
            if (PrimaryCards.Contains(cardId))
            {
                Debug.LogWarning($"カードID {cardId} は既にデッキに編成されています。同じ主力カードは1枚のみ編成できます。");
                return false;
            }
            
            PrimaryCards.Add(cardId);
            return true;
        }

        /// <summary>
        /// サポート・特殊カードを追加
        /// </summary>
        public bool AddSupportCard(string cardId)
        {
            if (SupportCards.Count >= SUPPORT_CARD_COUNT)
            {
                return false;
            }
            SupportCards.Add(cardId);
            return true;
        }

        /// <summary>
        /// 主力カードを削除
        /// </summary>
        public bool RemovePrimaryCard(string cardId)
        {
            return PrimaryCards.Remove(cardId);
        }

        /// <summary>
        /// サポート・特殊カードを削除
        /// </summary>
        public bool RemoveSupportCard(string cardId)
        {
            return SupportCards.Remove(cardId);
        }

        /// <summary>
        /// デッキをクリア
        /// </summary>
        public void Clear()
        {
            PrimaryCards.Clear();
            SupportCards.Clear();
        }
    }
}
