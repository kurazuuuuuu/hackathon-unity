using UnityEngine;

namespace Game.Battle.StatusEffects
{
    [CreateAssetMenu(fileName = "StunStatus", menuName = "Game/Status/Stun")]
    public class StunStatus : StatusEffect
    {
        // Stun logic needs to be checked by BattleManager/CardAction
        // Since we don't have a direct "OnCanAct" hook yet, we rely on checking StatusEffects list
        // Or we add a hook to Card if it has "CanAct" property.
        
        // For now, let's assume Card has CanAction() method or similar that checks this.
        public bool IsStunned => true;
    }
}
