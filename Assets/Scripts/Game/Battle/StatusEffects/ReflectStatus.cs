using UnityEngine;

namespace Game.Battle.StatusEffects
{
    [CreateAssetMenu(fileName = "ReflectStatus", menuName = "Game/Status/Reflect")]
    public class ReflectStatus : StatusEffect
    {
        public bool ReflectsAll = true; // or just specific amount?

        public override void OnTakeDamage(ref int damage)
        {
            // Logic: Damage happens to self? 
            // 4D says "Reflect opponent's attack". Usually implies ignoring damage self and dealing it to attacker.
            // Or both take damage?
            // Assuming "Counter": Take damage, then deal back.
            // Assuming "Reflect": Negate damage, deal back.
            
            // Implementing "Reflect": Negate incoming, deal to opponent (source needed).
            // Problem: OnTakeDamage hook doesn't pass Source info currently.
            // I need to update OnTakeDamage to accept Source info if I want to reflect back to source.
            
            // HACK: For now, Reflection might just negate damage. 
            // To deal damage back, we need context.
            // I will update Card.cs TakeDamage signature or store "LastAttacker".
            
            Debug.Log("Reflect triggered! (Damage negation only for MVP)");
            damage = 0;
            // TODO: Deal damage to attacker.
        }
    }
}
