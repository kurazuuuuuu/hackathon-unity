using UnityEngine;
using TMPro;
using Game.Debugging.AtomicUI.Atoms;

namespace Game.Debugging.AtomicUI.Molecules
{
    public class UnitStatusMolecule : AtomBase
    {
        private string _prefix;
        private string _nameText;
        private TextAlignmentOptions _alignment;

        public UnitStatusMolecule(string prefix, string nameText, bool isRightAligned)
        {
            _prefix = prefix;
            _nameText = nameText;
            _alignment = isRightAligned ? TextAlignmentOptions.Right : TextAlignmentOptions.Left;
        }

        public override void Build(Transform parent)
        {
            // Name Atom
            var nameAtom = new AtomText($"{_prefix}Name", _nameText, 80, _alignment);
            nameAtom.Build(parent);

            // Anchor/Pos logic is typically handled by layout or strict positioning in parent Organism,
            // but here we can ensure relative positioning "Molecule style" if we wrap them in a container,
            // OR we just spawn them as siblings if that's the design.
            // For BattleUIConstructor legacy parity, they were siblings on the Area.
            // Let's create a container "StatusPanel" to be a true Molecule.
            
            var panel = EnsureObject(parent, $"{_prefix}StatusPanel");
            if(IsAlive(panel))
            {
                // Background (Glass)
                var bg = new AtomImage("BG", Color.white, null, false);
                bg.Build(panel);
                bg.SetGlassStyle(true);
                var bgRect = EnsureComponent<RectTransform>(GetChild(panel, "BG").gameObject);
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.sizeDelta = Vector2.zero;
                
                var rect = EnsureComponent<RectTransform>(panel.gameObject);
                if (CreatedNew)
                {
                    rect.anchorMin = _alignment == TextAlignmentOptions.Left ? new Vector2(0.05f, 0.3f) : new Vector2(0.95f, 0.7f);
                    rect.anchorMax = rect.anchorMin; 
                    rect.sizeDelta = new Vector2(900, 300); // Expanded for Bar
                }

                // Re-build atoms inside panel
                string nameObjName = $"{_prefix}Name";
                string hpObjName = $"{_prefix}HP";
                
                new AtomText(nameObjName, _nameText, 72, _alignment).Build(panel);
                bool nameNew = CreatedNew; // Track if Name was just created
                
                new AtomText(hpObjName, "HP: 20/20", 56, _alignment).Build(panel);
                bool hpNew = CreatedNew; // Track if HP was just created

                // --- HP BAR ---
                // Container
                var barContainer = EnsureObject(panel, "HPBarContainer");
                bool barNew = CreatedNew;
                var barRect = EnsureComponent<RectTransform>(barContainer.gameObject);
                if (barNew) barRect.sizeDelta = new Vector2(600, 30);
                
                // Background Bar
                new AtomImage("BarBG", new Color(0,0,0, 0.5f)).Build(barContainer);
                var bgBarR = EnsureComponent<RectTransform>(GetChild(barContainer, "BarBG").gameObject);
                if (CreatedNew) // Previous call was build BarBG
                {
                    bgBarR.anchorMin = Vector2.zero;
                    bgBarR.anchorMax = Vector2.one;
                    bgBarR.sizeDelta = Vector2.zero;
                }

                // Fill Bar
                new AtomImage("BarFill", new Color(0.2f, 0.8f, 0.2f, 1f)).Build(barContainer);
                var fillBarR = EnsureComponent<RectTransform>(GetChild(barContainer, "BarFill").gameObject);
                if (CreatedNew)
                {
                    fillBarR.anchorMin = Vector2.zero;
                    fillBarR.anchorMax = Vector2.one;
                    fillBarR.pivot = new Vector2(0, 0.5f);
                }
                
                // Layout manually inside the molecule
                float xOffset = _alignment == TextAlignmentOptions.Left ? 50 : -50;
                
                var nt = GetChild(panel, nameObjName);
                if(IsAlive(nt) && nameNew)
                {
                    var r = EnsureComponent<RectTransform>(nt.gameObject);
                    r.anchoredPosition = new Vector2(xOffset, 80);
                    r.sizeDelta = new Vector2(800, 100);
                }
                var ht = GetChild(panel, hpObjName);
                if(IsAlive(ht) && hpNew)
                {
                    var r = EnsureComponent<RectTransform>(ht.gameObject);
                    r.anchoredPosition = new Vector2(xOffset, 0); 
                    r.sizeDelta = new Vector2(800, 80);
                }
                
                // Position Bar below HP Text
                if (barNew)
                {
                    barRect.anchoredPosition = new Vector2(xOffset, -70);
                    if(_alignment == TextAlignmentOptions.Right)
                    {
                        barRect.pivot = new Vector2(1, 0.5f);
                    }
                }
            }
        }
    }
}
