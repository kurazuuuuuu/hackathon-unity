using UnityEngine;

namespace Game.Abilities.Actions
{
    [CreateAssetMenu(fileName = "DamageAbility", menuName = "Game/Abilities/Actions/Damage")]
    public class DamageAbility : CardAbility
    {
        public enum DamageTargetType { SingleEnemy, AllEnemies, RandomEnemy }
        
        [Header("Damage Settings")]
        [SerializeField] private int fixedDamage;
        [SerializeField] private bool useSourcePower = false; // Add source card power to damage
        [SerializeField] private DamageTargetType targetType = DamageTargetType.SingleEnemy;

        public override void Activate(BattleContext context)
        {
            int amount = fixedDamage;
            if (useSourcePower && context.SourceCard != null)
            {
                amount += context.SourceCard.Power;
            }

            switch (targetType)
            {
                case DamageTargetType.SingleEnemy:
                    if (context.TargetCard != null)
                    {
                        var targetCard = context.TargetCard.GetComponent<Card>();
                        if (targetCard != null)
                        {
                            Debug.Log($"{context.SourceCard?.Name} deals {amount} damage to {targetCard.Name}");
                            targetCard.TakeDamage(amount);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("DamageAbility: No target specified for SingleEnemy damage.");
                    }
                    break;

                case DamageTargetType.AllEnemies:
                    if (context.TargetPlayer != null)
                    {
                        foreach (var card in context.TargetPlayer.PrimaryCardsInPlay)
                        {
                            var primaryCard = card as PrimaryCard;
                            if (primaryCard != null && !primaryCard.IsDead)
                            {
                                var cardComponent = card.GetComponent<Card>();
                                if (cardComponent != null)
                                    cardComponent.TakeDamage(amount);
                            }
                        }
                        Debug.Log($"{context.SourceCard?.Name} deals {amount} damage to all enemies.");
                    }
                    break;
                    
                 // RandomEnemy unimplemented for now, fallback to single or ignore
            }
        }
    }
}
