using UnityEngine;

namespace Game
{
    /// <summary>
    /// サポート資格カード・特殊カード用データ
    /// 基底クラスのプロパティのみを使用
    /// </summary>
    [CreateAssetMenu(fileName = "NewSupportCard", menuName = "Game/Card Data/Support Card")]
    public class SupportCardData : CardDataBase
    {
        [Header("Support Card Settings")]
        [SerializeField] private bool isSpecialCard = false;  // 特殊カードフラグ
        
        /// <summary>
        /// カードタイプ（Support or Special）
        /// </summary>
        public override CardType CardType => isSpecialCard ? CardType.Special : CardType.Support;
        
        /// <summary>
        /// 特殊カードかどうか
        /// </summary>
        public bool IsSpecialCard => isSpecialCard;
    }
}
