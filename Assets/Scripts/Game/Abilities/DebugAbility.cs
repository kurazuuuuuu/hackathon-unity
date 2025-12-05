using UnityEngine;

namespace Game.Abilities
{
    [CreateAssetMenu(fileName = "DebugAbility", menuName = "Game/Abilities/Debug Ability")]
    public class DebugAbility : CardAbility
    {
        public override void Activate(Card user)
        {
            Debug.Log($"Debug Ability Activated by {user.Name}! Power: {user.Power}, Heal: {user.Heal}");
        }
    }
}
