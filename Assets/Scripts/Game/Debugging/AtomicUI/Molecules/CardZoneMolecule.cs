using UnityEngine;
using UnityEngine.UI;
using Game.Debugging.AtomicUI.Atoms;

namespace Game.Debugging.AtomicUI.Molecules
{
    public class CardZoneMolecule : AtomBase
    {
        private string _name;
        private bool _isHand;
        
        public CardZoneMolecule(string name, bool isHand)
        {
            _name = name;
            _isHand = isHand;
        }

        public override void Build(Transform parent)
        {
            var zone = EnsureObject(parent, _name);
            if (!IsAlive(zone)) return;

            // Visuals
            if (!_isHand)
            {
                // Add a background image to visualize the zone
                var img = EnsureComponent<Image>(zone.gameObject);
                img.color = new Color(1f, 1f, 1f, 0.2f); // Semi-transparent white
            }
            // Logic specific to game components
            if (!_isHand) EnsureComponent<Game.PrimaryCardZone>(zone.gameObject);
            
            // Layout Group Atom? Or just EnsureComponent here.
            var lg = EnsureComponent<HorizontalLayoutGroup>(zone.gameObject);
            if (lg != null)
            {
                lg.childAlignment = _isHand ? TextAnchor.LowerCenter : TextAnchor.MiddleCenter;
                lg.spacing = _isHand ? -60 : 50;
                lg.childControlWidth = false; 
                lg.childControlHeight = false;
            }
        }
    }
}
