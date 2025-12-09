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
        public List<CardDataBase> Featured5Stars;
        public List<CardDataBase> Standard5Stars;
        
        // 4 Star Pools
        public List<CardDataBase> Pool4StarSupport;
        public List<CardDataBase> Pool4StarSpecial;

        // 3 Star Pools
        public List<CardDataBase> Pool3StarSupport;
        public List<CardDataBase> Pool3StarSpecial;
    }
}
