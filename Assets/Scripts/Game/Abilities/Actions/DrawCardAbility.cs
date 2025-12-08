using UnityEngine;

namespace Game.Abilities.Actions
{
    [CreateAssetMenu(fileName = "DrawCardAbility", menuName = "Game/Abilities/Actions/DrawCard")]
    public class DrawCardAbility : CardAbility
    {
        [Header("Draw Settings")]
        [SerializeField] private int drawCount = 1;

        public override void Activate(BattleContext context)
        {
            if (context.SourcePlayer != null && context.BattleManager != null)
            {
                 // BattleManager usually handles drawing, but Player has DrawCard(CardManager) method
                 // We need CardManager reference. BattleManager has it private usually.
                 // Ideally BattleManager should expose a "PlayerDraws(Player p, int count)" method.
                 
                 // For now, assuming we can access CardManager via Service Locator or Context having it.
                 // Context doesn't have CardManager, only BattleManager.
                 // Let's assume BattleManager has a public method or property we added/will add.
                 
                 // HACK: Find CardManager for now if not available
                 var cardManager = Object.FindAnyObjectByType<CardManager>();
                 if (cardManager != null)
                 {
                     for(int i=0; i<drawCount; i++)
                     {
                         context.SourcePlayer.DrawCard(cardManager);
                     }
                 }
            }
        }
    }
}
