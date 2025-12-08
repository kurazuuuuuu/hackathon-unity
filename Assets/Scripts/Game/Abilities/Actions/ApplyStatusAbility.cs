using UnityEngine;
using Game.Battle.StatusEffects;

namespace Game.Abilities.Actions
{
    [CreateAssetMenu(fileName = "ApplyStatusAbility", menuName = "Game/Abilities/Actions/ApplyStatus")]
    public class ApplyStatusAbility : CardAbility
    {
        [SerializeField] private StatusEffect statusEffectPrefab;
        [SerializeField] private bool targetSelf = false;
        [SerializeField] private bool targetAllEnemies = false; // 5D: All enemies -3 power (Debuff status)

        public override void Activate(BattleContext context)
        {
            if (statusEffectPrefab == null) return;
            
            if (targetAllEnemies && context.TargetPlayer != null)
            {
                foreach (var card in context.TargetPlayer.PrimaryCardsInPlay)
                {
                    var primaryCard = card as PrimaryCard;
                    if (primaryCard != null && !primaryCard.IsDead)
                    {
                        var cardComponent = card.GetComponent<Card>();
                        if (cardComponent != null)
                            cardComponent.AddStatus(statusEffectPrefab);
                    }
                }
            }
            else if (targetSelf && context.SourceCard != null)
            {
                var sourceCard = context.SourceCard.GetComponent<Card>();
                if (sourceCard != null)
                    sourceCard.AddStatus(statusEffectPrefab);
            }
            else if (context.TargetCard != null)
            {
                var targetCard = context.TargetCard.GetComponent<Card>();
                if (targetCard != null)
                    targetCard.AddStatus(statusEffectPrefab);
            }
        }
    }
}
