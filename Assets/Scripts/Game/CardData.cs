using UnityEngine;
using Game.Abilities;

namespace Game
{
    /// <summary>
    /// カードの基本データを管理するScriptableObject
    /// 後方互換性のため維持。新規作成時はPrimaryCardData/SupportCardDataを使用推奨
    /// </summary>
    [CreateAssetMenu(fileName = "NewCardData", menuName = "Game/Card Data/Legacy Card")]
    public class CardData : CardDataBase
    {
        [Header("Legacy Fields")]
        [SerializeField] private CardType cardType;
        [SerializeField] private int heal;     // 回復力（レガシー）
        [SerializeField] private int health;   // 体力（Primary用）
        
        [Header("Gacha Info (Legacy)")]
        [SerializeField] private int rarity;   // レアリティ (手動設定用)
        
        /// <summary>
        /// カードタイプ
        /// </summary>
        public override CardType CardType => cardType;
        
        /// <summary>
        /// 回復力（レガシー互換）
        /// </summary>
        public int Heal => heal;
        
        /// <summary>
        /// 体力（Primary用）
        /// </summary>
        public int Health => health;
        
        /// <summary>
        /// レアリティ（手動設定がある場合はそちらを優先）
        /// </summary>
        public new int Rarity => rarity > 0 ? rarity : base.Rarity;
    }
}
