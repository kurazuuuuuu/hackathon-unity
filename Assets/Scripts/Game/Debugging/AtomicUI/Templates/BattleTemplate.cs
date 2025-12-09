using UnityEngine;
using Game.Debugging.AtomicUI.Atoms;

namespace Game.Debugging.AtomicUI.Templates
{
    public class BattleTemplate : AtomBase
    {
        public override void Build(Transform root)
        {
            // Define main containers/anchors.
            // 1. Background Layer (Sibling 0)
            EnsureObject(root, "BackgroundLayer");

            // 2. Play Area (P1, P2)
            EnsureObject(root, "Player1Area"); 
            EnsureObject(root, "Player2Area");

            // 3. Center Info
            EnsureObject(root, "CenterInfoArea");

            // 4. Overlays (Action, Result)
            EnsureObject(root, "ActionSelectionUI");
            EnsureObject(root, "ResultOverlay");
            
            // 5. VFX Layers (Optional, should be high sibling index)
            // Handled by specific VFX Organism/Atom if needed
        }
    }
}
