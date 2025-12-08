using UnityEngine;

namespace Game.Abilities.Actions
{
    [CreateAssetMenu(fileName = "HealAbility", menuName = "Game/Abilities/Actions/Heal")]
    public class HealAbility : CardAbility
    {
        public enum HealTargetType { Self, TargetAlly, AllAllies, Player }
        
        [Header("Heal Settings")]
        [SerializeField] private int fixedHeal;
        [SerializeField] private bool useSourceHealStat = false;
        [SerializeField] private HealTargetType targetType = HealTargetType.TargetAlly;

        public override void Activate(BattleContext context)
        {
            int amount = fixedHeal;
            if (useSourceHealStat && context.SourceCard != null)
            {
                var sourceCard = context.SourceCard.GetComponent<Card>();
                if (sourceCard != null)
                    amount += sourceCard.Heal;
            }

            switch (targetType)
            {
                case HealTargetType.Self:
                    if (context.SourceCard != null)
                    {
                        var sourceCard = context.SourceCard.GetComponent<Card>();
                        if (sourceCard != null)
                        {
                            sourceCard.RecoverHealth(amount);
                            Debug.Log($"{sourceCard.Name} healed for {amount}.");
                        }
                    }
                    break;

                case HealTargetType.TargetAlly:
                    if (context.TargetCard != null)
                    {
                        var targetCard = context.TargetCard.GetComponent<Card>();
                        if (targetCard != null)
                        {
                            targetCard.RecoverHealth(amount);
                            Debug.Log($"{targetCard.Name} healed for {amount} by {context.SourceCard?.Name}.");
                        }
                    }
                    else
                    {
                         Debug.LogWarning("HealAbility: No target for TargetAlly heal.");
                    }
                    break;

                case HealTargetType.AllAllies:
                     if (context.SourcePlayer != null)
                    {
                        foreach (var card in context.SourcePlayer.PrimaryCardsInPlay)
                        {
                            var primaryCard = card as PrimaryCard;
                            if (primaryCard != null && !primaryCard.IsDead)
                            {
                                var cardComponent = card.GetComponent<Card>();
                                if (cardComponent != null)
                                    cardComponent.RecoverHealth(amount);
                            }
                        }
                        Debug.Log($"All allies healed for {amount}.");
                    }
                    break;
                case HealTargetType.Player:
                    if (context.SourcePlayer != null)
                    {
                        context.SourcePlayer.HealHP(amount);
                    }
                    break;
            }
        }
    }
}
