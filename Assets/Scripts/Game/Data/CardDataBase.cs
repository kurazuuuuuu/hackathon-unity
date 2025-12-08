using UnityEngine;
using Game.Abilities;

namespace Game
{
    /// <summary>
    /// カードデータの基底クラス
    /// すべてのカードタイプで共通のプロパティを定義
    /// </summary>
    public abstract class CardDataBase : ScriptableObject
    {
        [Header("Card Identity")]
        [SerializeField] protected string cardId;
        [SerializeField] protected string cardName;
        
        [Header("Stats")]
        [SerializeField] protected int power;      // 攻撃力
        [SerializeField] protected int cost;       // 使用時の体力消費量
        
        [Header("Effects")]
        [SerializeField] protected CardAbility ability;           // 特殊効果
        [SerializeField] protected CardAbility passiveEffect;     // 所持追加効果（内部データ）
        
        [Header("Progression")]
        [SerializeField] protected int charge;     // 凸数
        
        // カードIDはファイル名から自動取得も可能
        public string CardId => !string.IsNullOrEmpty(cardId) ? cardId : name;
        public string CardName => cardName;
        public int Power => power;
        public int Cost => cost;
        public CardAbility Ability => ability;
        public CardAbility PassiveEffect => passiveEffect;
        public int Charge => charge;
        
        /// <summary>
        /// カードタイプを取得（派生クラスで実装）
        /// </summary>
        public abstract CardType CardType { get; }
        
        /// <summary>
        /// ファイル名からレアリティを自動判定
        /// 5x = 5, 4x = 4, 3x = 3
        /// </summary>
        public int Rarity
        {
            get
            {
                string id = CardId;
                if (string.IsNullOrEmpty(id) || id.Length < 2) return 3;
                
                char firstChar = id[0];
                if (firstChar == '5') return 5;
                if (firstChar == '4') return 4;
                return 3;
            }
        }
        
        /// <summary>
        /// 主力カードかどうか
        /// </summary>
        public bool IsPrimary => CardType == CardType.Primary;
        
        /// <summary>
        /// サポートカードかどうか
        /// </summary>
        public bool IsSupport => CardType == CardType.Support;
        
        /// <summary>
        /// 特殊カードかどうか
        /// </summary>
        public bool IsSpecial => CardType == CardType.Special;
    }
}
