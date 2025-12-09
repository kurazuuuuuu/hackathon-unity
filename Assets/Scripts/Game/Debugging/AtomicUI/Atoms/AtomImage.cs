using UnityEngine;
using UnityEngine.UI;

namespace Game.Debugging.AtomicUI.Atoms
{
    public class AtomImage : AtomBase
    {
        private string _name;
        private Color _color;
        private Sprite _sprite;
        private bool _raycastTarget;

        public AtomImage(string name, Color color, Sprite sprite = null, bool raycastTarget = true)
        {
            _name = name;
            _color = color;
            _sprite = sprite;
            _raycastTarget = raycastTarget;
        }

        // Cache the root transform
        private Transform _root;

        public override void Build(Transform parent)
        {
            _root = EnsureObject(parent, _name);
            if (!IsAlive(_root)) return;

            // Anchor logic handled by parent (Molecule/Organism) usually, 
            // but Atom should ensure it stretches if it's a background? 
            // For true atomic design, layout is often separate. 
            // Here we assume full stretch default, but usually layout data comes from outside.
            // Simplified: Atom just ensures Component properties.

            var img = EnsureComponent<Image>(_root.gameObject);
            if (img != null)
            {
                img.color = _color;
                if (_sprite != null) img.sprite = _sprite;
                img.raycastTarget = _raycastTarget;
            }
        }
        public void SetGlassStyle(bool isGlass)
        {
            if (isGlass && IsAlive(_root))
            {
                var img = EnsureComponent<Image>(_root.gameObject);
                if (img != null)
                {
                    // Glassmorphism: White, Low Alpha
                    img.color = new Color(1f, 1f, 1f, 0.15f); 
                    // Note: Ideally needs blur shader, but transparency is step 1.
                    // If we had a rounded corner sprite, we'd use it here.
                    // For now, assume default sprite or user provided one.
                }
            }
        }
    }
}
