using UnityEngine;

namespace Game
{
    /// <summary>
    /// 主力資格カード用データ
    /// 体力パラメータを追加で持つ
    /// </summary>
    [CreateAssetMenu(fileName = "NewPrimaryCard", menuName = "Game/Card Data/Primary Card")]
    public class PrimaryCardData : CardDataBase
    {
        [Header("Primary Card Stats")]
        [SerializeField] private int health;     // 体力
        
        /// <summary>
        /// カードの体力値
        /// </summary>
        public int Health => health;
        
        /// <summary>
        /// カードタイプは常にPrimary
        /// </summary>
        public override CardType CardType => CardType.Primary;
    }
}
