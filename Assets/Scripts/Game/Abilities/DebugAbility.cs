using UnityEngine;

namespace Game.Abilities
{
    [CreateAssetMenu(fileName = "DebugAbility", menuName = "Game/Abilities/Debug Ability")]
    public class DebugAbility : CardAbility
    {
        public override void Activate(BattleContext context)
        {
            string targetName = context.TargetCard != null ? context.TargetCard.Name : "None";
            string userName = context.SourceCard != null ? context.SourceCard.Name : "None";
            Debug.Log($"Debug Ability Activated by {userName}! Target: {targetName}");
            // Use context for more details if needed
        }
    }
}
