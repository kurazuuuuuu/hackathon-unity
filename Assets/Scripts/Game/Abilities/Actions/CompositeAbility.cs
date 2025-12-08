using System.Collections.Generic;
using UnityEngine;

namespace Game.Abilities.Actions
{
    [CreateAssetMenu(fileName = "CompositeAbility", menuName = "Game/Abilities/Composite")]
    public class CompositeAbility : CardAbility
    {
        [SerializeField] private List<CardAbility> abilities = new List<CardAbility>();

        public override void Activate(BattleContext context)
        {
            foreach (var ability in abilities)
            {
                if (ability != null)
                {
                    ability.Activate(context);
                }
            }
        }
    }
}
