using System;
using System.Collections.Generic;

namespace Game.Data
{
    /// <summary>
    /// ユーザーのゲームプレイデータを格納するモデル
    /// </summary>
    [Serializable]
    public class UserData
    {
        // ユーザー情報
        public string UserName;
        public int GachaTickets;

        // 所有カード (カードID -> 所持数)
        public Dictionary<string, int> OwnedCards = new Dictionary<string, int>();

        // デッキリスト
        public List<DeckData> Decks = new List<DeckData>();

        // タイムスタンプ
        public DateTime CreatedAt;
        public DateTime UpdatedAt;

        public UserData()
        {
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }

        public UserData(string userName)
        {
            UserName = userName;
            GachaTickets = 0;
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }

        /// <summary>
        /// カードを追加
        /// </summary>
        public void AddCard(string cardId, int count = 1)
        {
            if (OwnedCards.ContainsKey(cardId))
            {
                OwnedCards[cardId] += count;
            }
            else
            {
                OwnedCards[cardId] = count;
            }
            UpdatedAt = DateTime.Now;
        }

        /// <summary>
        /// カードの所持数を取得
        /// </summary>
        public int GetCardCount(string cardId)
        {
            return OwnedCards.TryGetValue(cardId, out int count) ? count : 0;
        }

        /// <summary>
        /// 更新日時を記録
        /// </summary>
        public void MarkUpdated()
        {
            UpdatedAt = DateTime.Now;
        }
    }
}
