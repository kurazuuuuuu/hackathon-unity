using UnityEngine;

namespace Game.Battle.StatusEffects
{
    [CreateAssetMenu(fileName = "ShieldStatus", menuName = "Game/Status/Shield")]
    public class ShieldStatus : StatusEffect
    {
        public int ShieldAmount; // Reduced when taking damage

        public override void OnTakeDamage(ref int damage)
        {
            if (damage <= 0) return;
            
            int blocked = Mathf.Min(damage, ShieldAmount);
            damage -= blocked;
            ShieldAmount -= blocked;
            
            Debug.Log($"Shield blocked {blocked} damage. Remaining Shield: {ShieldAmount}");
            
            if (ShieldAmount <= 0)
            {
                Remove();
            }
        }
    }
}
