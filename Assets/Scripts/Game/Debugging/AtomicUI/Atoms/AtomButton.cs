using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.Debugging.AtomicUI.Atoms
{
    public class AtomButton : AtomBase
    {
        private string _name;
        private string _label;
        private int _fontSize;
        private global::System.Action _onClick; // Not persistent in editor tool build, but useful for runtime generation

        public AtomButton(string name, string label, int fontSize = 32)
        {
            _name = name;
            _label = label;
            _fontSize = fontSize;
        }

        public override void Build(Transform parent)
        {
            var tf = EnsureObject(parent, _name);
            if (!IsAlive(tf)) return;

            var img = EnsureComponent<Image>(tf.gameObject);
            // Default styling
            if(img.sprite == null) img.color = new Color(0.9f, 0.9f, 0.9f);

            var btn = EnsureComponent<Button>(tf.gameObject);

            // Label Atom reused!
            var textAtom = new AtomText("Label", _label, _fontSize, TextAlignmentOptions.Center, Color.black);
            textAtom.Build(tf);
            
            // Ensure label stretches
            var labelT = GetChild(tf, "Label");
            if(IsAlive(labelT))
            {
                var r = EnsureComponent<RectTransform>(labelT.gameObject);
                if(r != null)
                {
                    r.anchorMin = Vector2.zero;
                    r.anchorMax = Vector2.one;
                    r.offsetMin = Vector2.zero;
                    r.offsetMax = Vector2.zero;
                }
            }
        }
    }
}
