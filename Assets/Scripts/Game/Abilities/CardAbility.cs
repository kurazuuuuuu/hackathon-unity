using UnityEngine;

namespace Game.Abilities
{
    /// <summary>
    /// カードの能力を表す基底クラス
    /// </summary>
    public abstract class CardAbility : ScriptableObject
    {
        [TextArea]
        [SerializeField] private string description;

        public string Description => description;

        /// <summary>
        /// 能力を発動する
        /// </summary>
        /// <param name="user">能力を使用したカードの所有者など</param>
        public abstract void Activate(Card user);
    }
}
