using UnityEngine;

namespace Game.Battle.StatusEffects
{
    [CreateAssetMenu(fileName = "AttackBuffStatus", menuName = "Game/Status/AttackBuff")]
    public class AttackBuffStatus : StatusEffect
    {
        public int PowerIncrease;

        public override void OnDealDamage(ref int damage)
        {
            damage += PowerIncrease;
            Debug.Log($"Attack Buff added {PowerIncrease} damage.");
        }
    }
}
