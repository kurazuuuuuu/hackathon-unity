using UnityEngine;

namespace Game
{
    /// <summary>
    /// カード関連のユーティリティメソッド
    /// </summary>
    public static class CardHelper
    {
        /// <summary>
        /// カードIDからレアリティを判定
        /// 5x = 5, 4x = 4, 3x = 3
        /// </summary>
        public static int GetRarityFromId(string cardId)
        {
            if (string.IsNullOrEmpty(cardId) || cardId.Length < 2) return 3;
            
            char firstChar = cardId[0];
            if (firstChar == '5') return 5;
            if (firstChar == '4') return 4;
            return 3;
        }
        
        /// <summary>
        /// カードIDからカードタイプを判定
        /// 5x = Primary, 4x = Support, 3A-3E = Support, 3F以降 = Special
        /// </summary>
        public static CardType GetCardTypeFromId(string cardId)
        {
            if (string.IsNullOrEmpty(cardId)) return CardType.Support;
            
            // If ID is short, it might be just "5A" etc.
            if (cardId.Length >= 2)
            {
                char firstChar = cardId[0];
                
                // 5x: Primary cards
                if (firstChar == '5') return CardType.Primary;
                
                // 4x: Support cards
                if (firstChar == '4') return CardType.Support;
                
                // 3x: Check second character
                if (firstChar == '3')
                {
                    if (cardId.Length >= 2)
                    {
                        char secondChar = cardId[1];
                        // 3A-3E: Support, 3F onwards: Special
                        if (secondChar >= 'A' && secondChar <= 'E')
                        {
                            return CardType.Support;
                        }
                    }
                    return CardType.Special;
                }
            }
            // Fallback for names like "5A" which are 2 chars long
            return CardType.Support;
        }
        
        /// <summary>
        /// レアリティに応じた色を取得
        /// </summary>
        public static Color GetRarityColor(int rarity)
        {
            switch (rarity)
            {
                case 5: return new Color(1.0f, 0.85f, 0.0f);  // Gold
                case 4: return new Color(0.8f, 0.2f, 0.8f);   // Purple
                case 3: return new Color(0.2f, 0.9f, 0.9f);   // Cyan
                default: return new Color(0.5f, 0.5f, 0.5f);  // Gray
            }
        }
        
        /// <summary>
        /// レアリティに応じたグロー色を取得
        /// </summary>
        public static Color GetGlowColor(int rarity)
        {
            switch (rarity)
            {
                case 5: return new Color(1.0f, 0.8f, 0.0f, 0.8f);
                case 4: return new Color(1.0f, 0.2f, 1.0f, 0.7f);
                case 3: return new Color(0.2f, 1.0f, 1.0f, 0.6f);
                default: return Color.clear;
            }
        }
        
        /// <summary>
        /// Resources/Cardsからカードデータをロード
        /// </summary>
        public static CardDataBase LoadCardData(string cardId)
        {
            int rarity = GetRarityFromId(cardId);
            string path = $"Cards/{rarity}x/{cardId}";
            
            // まずCardDataBaseとしてロード試行
            CardDataBase data = Resources.Load<CardDataBase>(path);
            
            if (data == null)
            {
                // レガシーCardDataとしてロード試行
                data = Resources.Load<CardData>(path);
            }
            
            if (data == null)
            {
                Debug.LogWarning($"Card data not found: {path}");
            }
            
            return data;
        }
        
        /// <summary>
        /// カードIDに対応するPrefabパスを取得
        /// </summary>
        public static string GetCardPrefabPath(string cardId)
        {
            CardType type = GetCardTypeFromId(cardId);
            
            switch (type)
            {
                case CardType.Primary:
                    return "Prefabs/PrimaryCard";
                case CardType.Support:
                case CardType.Special:
                default:
                    return "Prefabs/SupportCard";
            }
        }
    }
}
