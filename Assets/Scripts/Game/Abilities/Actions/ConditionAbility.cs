using UnityEngine;

namespace Game.Abilities.Actions
{
    [CreateAssetMenu(fileName = "ConditionAbility", menuName = "Game/Abilities/Actions/Condition")]
    public class ConditionAbility : CardAbility
    {
        public enum ConditionType { Chance, ChargeLevel, TurnsInHand }
        
        [SerializeField] private ConditionType condition;
        [SerializeField] private float floatValue; // Chance (0.05 for 5%)
        [SerializeField] private int intValue; // Charge level, Turns
        
        [SerializeField] private CardAbility successAbility;
        [SerializeField] private CardAbility failAbility; // Optional

        public override void Activate(BattleContext context)
        {
            bool passed = false;
            switch (condition)
            {
                case ConditionType.Chance:
                    passed = Random.value <= floatValue;
                    break;
                case ConditionType.ChargeLevel:
                    // Check Source Card Charge (Convex)
                    if (context.SourceCard != null)
                    {
                        passed = context.SourceCard.Charge >= intValue;
                    }
                    break;
                case ConditionType.TurnsInHand:
                    if (context.SourceCard != null)
                    {
                        var sourceCard = context.SourceCard.GetComponent<Card>();
                        if (sourceCard != null)
                            passed = sourceCard.TurnsInHand >= intValue;
                    }
                    break;
            }

            if (passed)
            {
                if (successAbility != null) successAbility.Activate(context);
            }
            else
            {
                if (failAbility != null) failAbility.Activate(context);
            }
        }
    }
}
