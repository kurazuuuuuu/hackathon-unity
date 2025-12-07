using UnityEngine;
using System.Collections.Generic;

namespace Game.Gacha
{
    [CreateAssetMenu(fileName = "GachaRateTable", menuName = "Game/Gacha Rate Table")]
    public class GachaRateTable : ScriptableObject
    {
        [Header("Probabilities")]
        public float Rate5Star = 0.03f;
        public float Rate5StarFirstTime = 0.06f; // 初回限定
        public float Rate4Star = 0.15f;
        public float SpookRate = 0.5f; // すり抜け確率 (50%)

        [Header("Card Pools")]
        public List<CardData> Featured5Stars;
        public List<CardData> Standard5Stars;
        
        // 4 Star Pools
        public List<CardData> Pool4StarSupport;
        public List<CardData> Pool4StarSpecial;

        // 3 Star Pools
        public List<CardData> Pool3StarSupport;
        public List<CardData> Pool3StarSpecial;
    }
}
