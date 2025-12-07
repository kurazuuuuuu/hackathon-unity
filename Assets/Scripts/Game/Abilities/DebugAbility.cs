using UnityEngine;

namespace Game.Abilities
{
    [CreateAssetMenu(fileName = "DebugAbility", menuName = "Game/Abilities/Debug Ability")]
    public class DebugAbility : CardAbility
    {
        public override void Activate(Card user, Card target = null)
        {
            string targetName = target != null ? target.Name : "None";
            Debug.Log($"Debug Ability Activated by {user.Name}! Target: {targetName}, Power: {user.Power}, Heal: {user.Heal}");
        }
    }
}
