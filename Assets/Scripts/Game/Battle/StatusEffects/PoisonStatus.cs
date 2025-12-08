using UnityEngine;

namespace Game.Battle.StatusEffects
{
    [CreateAssetMenu(fileName = "PoisonStatus", menuName = "Game/Status/Poison")]
    public class PoisonStatus : StatusEffect
    {
        public int DamagePerTurn = 1;

        public override void OnTurnStart()
        {
            if (ownerCard != null && !ownerCard.IsDead)
            {
                Debug.Log($"Poison deals {DamagePerTurn} damage to {ownerCard.Name}");
                ownerCard.TakeDamage(DamagePerTurn);
            }
        }
    }
}
