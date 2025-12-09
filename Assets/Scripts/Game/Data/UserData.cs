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
        public int Level;
        public int GachaTickets;
        public bool IsFirstGacha = true; // 初回ガチャフラグ

        // 所有カード (カードID -> 所持数)
        public Dictionary<string, int> OwnedCards = new Dictionary<string, int>();

        // デッキリスト
        public List<DeckData> Decks = new List<DeckData>();

        // 現在使用中のデッキID
        public string CurrentDeckId;

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
        /// API用DTOに変換
        /// </summary>
        public UserGameProfileDto ToApiProfile()
        {
            var profile = new UserGameProfileDto
            {
                display_name = UserName,
                level = Level,
                ticket = GachaTickets,
                current_deck_id = CurrentDeckId,
                // CreatedAt/UpdatedAtは文字列変換 (必要ならISO8601等)
                // サーバー側で管理されるため送信時はnullでも良いが、念のため
                created_at = CreatedAt.ToString("O"), 
                updated_at = UpdatedAt.ToString("O")
            };

            // Cards conversion
            foreach (var kvp in OwnedCards)
            {
                profile.cards.Add(new CardDto { card_id = kvp.Key, amount = kvp.Value });
            }

            // Decks conversion
            // 注意: DeckDataは内部でGuid等を持っていないため、既存の構造だとdeck_idの管理が曖昧ですが、
            // 今回は便宜上DeckNameやIndexなどをIDとするか、DeckDataにIDを持たせるのが理想です。
            // ここではDeckDataにIDがないため、一時的にGuidを生成するか、DeckDataの改修が必要です。
            // DeckDataにIDが無いので、暫定的にDeckNameをIDとして扱います (または単純に生成)
            foreach (var deck in Decks)
            {
                 // Primary/Secondary conversion needs helper or manual map
                 var deckDto = new DeckDto
                 {
                     deck_id = global::System.Guid.NewGuid().ToString(), // 仮: 本当は永続化すべき
                     name = deck.DeckName,
                     primary_cards = new List<string>(deck.PrimaryCards),
                     secondary_cards = new List<string>(deck.SupportCards)
                 };
                 profile.decks.Add(deckDto);
            }

            return profile;
        }

        /// <summary>
        /// APIレスポンスからデータを適用
        /// </summary>
        public void FromApiProfile(UserGameProfileDto profile)
        {
            if (profile == null) return;

            UserName = profile.display_name;
            Level = profile.level;
            GachaTickets = profile.ticket;
            CurrentDeckId = profile.current_deck_id;
            
            // Cards
            OwnedCards.Clear();
            if (profile.cards != null)
            {
                foreach (var card in profile.cards)
                {
                    OwnedCards[card.card_id] = card.amount;
                }
            }

            // Decks
            Decks.Clear();
            if (profile.decks != null)
            {
                foreach (var deckDto in profile.decks)
                {
                    var deckData = new DeckData(deckDto.name);
                    
                    if (deckDto.primary_cards != null)
                        deckData.PrimaryCards.AddRange(deckDto.primary_cards);
                    
                    if (deckDto.secondary_cards != null)
                        deckData.SupportCards.AddRange(deckDto.secondary_cards);

                    Decks.Add(deckData);
                }
            }
            
            // Timestamps
            if (DateTime.TryParse(profile.created_at, out var created)) CreatedAt = created;
            if (DateTime.TryParse(profile.updated_at, out var updated)) UpdatedAt = updated;
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
