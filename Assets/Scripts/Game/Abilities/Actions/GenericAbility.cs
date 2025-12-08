using UnityEngine;

namespace Game.Abilities.Actions
{
    [CreateAssetMenu(fileName = "GenericAbility", menuName = "Game/Abilities/Generic")]
    public class GenericAbility : CardAbility
    {
        public override void Activate(BattleContext context)
        {
            Debug.Log($"Generic Ability Activated: {Description}");
            // Placeholder for complex custom logic not yet verified
        }
    }
}
