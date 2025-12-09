using UnityEngine;
using TMPro;

namespace Game.Debugging.AtomicUI.Atoms
{
    public class AtomText : AtomBase
    {
        private string _name;
        private string _content;
        private int _fontSize;
        private TextAlignmentOptions _alignment;
        private Color _color;
        private bool _enableOutline;
        private Color _outlineColor;

        public AtomText(string name, string content, int fontSize, TextAlignmentOptions align = TextAlignmentOptions.Center, Color? color = null, bool enableOutline = true)
        {
            _name = name;
            _content = content;
            _fontSize = fontSize;
            _alignment = align;
            _color = color ?? Color.black;
            _enableOutline = enableOutline;
            _outlineColor = Color.white; // Default outline color
            if (_color == Color.white) _outlineColor = Color.black;
        }

        public override void Build(Transform parent)
        {
            var tf = EnsureObject(parent, _name);
            if (!IsAlive(tf)) return;

            var tmp = EnsureComponent<TextMeshProUGUI>(tf.gameObject);
            if (tmp != null)
            {
                if (string.IsNullOrEmpty(tmp.text) || tmp.text != _content) 
                    tmp.text = _content; 

                // Only set style defaults if looks uninitialized or force update? 
                // Let's force update for Atomic nature
                tmp.fontSize = _fontSize;
                tmp.alignment = _alignment;
                tmp.color = _color;
                
                if (_enableOutline)
                {
                    // TMP Outline is handled via shader or material usually, 
                    // but for Quick UI we can use UnityEngine.UI.Outline component specifically if requested "Outline component"
                    // However, TMP has its own outline. 
                    // Let's use the simple component approach for "Outline" effect on the Rect if desired, 
                    // but on TextMeshProUGUI, the component 'Outline' might not work as expected vs TMP's material outline.
                    // Actually, UnityEngine.UI.Outline works on Graphic, so it works on TMP too (but might look cheap).
                    // The user said "use Outline component".
                    var outline = tf.gameObject.GetComponent<UnityEngine.UI.Outline>();
                    if (outline == null) outline = tf.gameObject.AddComponent<UnityEngine.UI.Outline>();
                    outline.effectColor = _outlineColor;
                    outline.effectDistance = new Vector2(2, -2);
                }
                else
                {
                    var outline = tf.gameObject.GetComponent<UnityEngine.UI.Outline>();
                    if (outline != null) Object.DestroyImmediate(outline);
                }
            }
        }
    }
}
