using UnityEngine;
using System.Collections;
using Game.Battle.StatusEffects;

namespace Game.Abilities.Actions
{
    [CreateAssetMenu(fileName = "ChannelingAbility", menuName = "Game/Abilities/Actions/Channeling")]
    public class ChannelingAbility : CardAbility
    {
        [SerializeField] private int turnsToWait;
        [SerializeField] private CardAbility effectToTrigger;
        [SerializeField] private StatusEffect channelingStatus; // Visual/Stun effect during wait

        public override void Activate(BattleContext context)
        {
            // Channeling logic requires a persistent object to count turns or a StatusEffect that triggers an ability on expiry.
            // Since StatusEffect expiration calls Remove(), we need a Hook "OnRemove" or "OnExpire" to trigger the effect.
            // Or we create a specific "ChannelingStatus" that holds the context and ability.
            
            // To be faithful to "ChannelingStatus", we need to create it at runtime.
            // This is complex via ScriptableObject architecture alone.
            
            // Hack: Use a Coroutine runner on BattleManager? No, turns are manual.
            
            // Best approach: A "ChannelingStatus" class that inherits StatusEffect, has a reference to Ability.
            // But we can't easily modify ScriptableObject status at runtime to hold references to Scene objects (Context).
            
            // Simplified: Add 'ChannelingStatus' to providing card. 
            // The Status effect itself triggers the ability in OnTurnEnd when Duration == 0.
            
            // We need a custom ChannelingStatus script (non-asset based or runtime instance).
            
            var status = ScriptableObject.CreateInstance<ChannelingStatus>();
            var sourceCard = context.SourceCard?.GetComponent<Card>();
            status.Initialize(sourceCard);
            status.Duration = turnsToWait;
            status.EffectToTrigger = effectToTrigger;
            status.SavedContext = context; // Warning: holding references might be risky if objects die, but for 3-5 turns OK.
            status.IsStackable = false;
            status.DisplayName = "Channeling";
            
            if (sourceCard != null)
            {
                sourceCard.AddStatus(status);
                Debug.Log($"{sourceCard.Name} started channeling for {turnsToWait} turns.");
            }
        }
    }
    
    // Runtime Status class defined in same file for convenience
    public class ChannelingStatus : StatusEffect
    {
        public CardAbility EffectToTrigger;
        public BattleContext SavedContext;
        
        public override void OnTurnEnd()
        {
            base.OnTurnEnd();
            if (Duration <= 0)
            {
                // Trigger!
                if (EffectToTrigger != null)
                {
                    Debug.Log("Channeling Complete! Unleashing effect.");
                    EffectToTrigger.Activate(SavedContext);
                }
            }
        }
        
        // Channeling usually prevents action?
        // Check Stun status logic.
    }
}
