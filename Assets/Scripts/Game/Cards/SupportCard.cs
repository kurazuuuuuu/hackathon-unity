using UnityEngine;

namespace Game
{
    /// <summary>
    /// サポート資格カード・特殊カード用MonoBehaviour
    /// 基底クラスの機能のみを使用
    /// </summary>
    public class SupportCard : CardBase
    {
        /// <summary>
        /// 特殊カードかどうか
        /// </summary>
        public bool IsSpecial { get; private set; }
        
        /// <summary>
        /// SupportCardDataを使用して初期化
        /// </summary>
        public void Initialize(SupportCardData data)
        {
            base.Initialize(data);
            this.IsSpecial = data.IsSpecialCard;
        }
        
        /// <summary>
        /// CardDataBaseからの初期化（互換性用）
        /// </summary>
        public override void Initialize(CardDataBase data)
        {
            if (data is SupportCardData supportData)
            {
                Initialize(supportData);
            }
            else
            {
                base.Initialize(data);
            }
        }
    }
}
