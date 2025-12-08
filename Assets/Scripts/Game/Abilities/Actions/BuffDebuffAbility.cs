using UnityEngine;

namespace Game.Abilities.Actions
{
    [CreateAssetMenu(fileName = "BuffDebuffAbility", menuName = "Game/Abilities/Actions/BuffDebuff")]
    public class BuffDebuffAbility : CardAbility
    {
        public enum StatType { Power, Defense }
        
        [Header("Buff Settings")]
        [SerializeField] private StatType stat = StatType.Power;
        [SerializeField] private int value; // Positive for Buff, Negative for Debuff
        [SerializeField] private int duration = 0; // 0 = Permanent, >0 = Turns
        [SerializeField] private bool targetAllEnemies = false;

        public override void Activate(BattleContext context)
        {
            // Note: Buff system needs a "StatusEffect" manager or list on Card/Player.
            // For now, implementing immediate permanent stat change as MVP if duration=0
            
            if (targetAllEnemies && context.TargetPlayer != null)
            {
                foreach (var card in context.TargetPlayer.PrimaryCardsInPlay)
                {
                    ApplyBuff(card.GetComponent<CardBase>());
                }
            }
            else if (context.TargetCard != null)
            {
                ApplyBuff(context.TargetCard);
            }
            else if (context.SourceCard != null && !targetAllEnemies) // Self buff default
            {
                ApplyBuff(context.SourceCard);
            }
        }

        private void ApplyBuff(CardBase cardBase)
        {
            if (cardBase == null) return;
            
            var card = cardBase.GetComponent<Card>();
            if (card == null) return;
            
            Debug.Log($"Applying Buff/Debuff {stat} {value} to {card.Name}");
            
            // TODO: Implement actual stat modification on Card class
            // card.ModifyStat(stat, value, duration);
        }
    }
}
