using UnityEngine;
using UnityEngine.UI;
using Game.Debugging.AtomicUI.Atoms;

namespace Game.Debugging.AtomicUI.Molecules
{
    public class DeckMolecule : AtomBase
    {
        private string _name;
        private int _stackCount;
        private float _offsetY;
        private float _scale;

        public DeckMolecule(string name, int stackCount = 5, float scale = 1.0f)
        {
            _name = name;
            _stackCount = stackCount;
            _scale = scale;
        }

        public override void Build(Transform parent)
        {
            // Container
            var root = EnsureObject(parent, _name);
            if (!IsAlive(root)) return;

            var rect = EnsureComponent<RectTransform>(root.gameObject);
            // Base size 280x390 * _scale
            float w = 280f * _scale;
            float h = 390f * _scale;
            rect.sizeDelta = new Vector2(w, h); // Scaled size 

            // Create ACTUAL 3D Object (Cube)
            // Note: Since we are in ScreenSpace-Camera, 3D objects verify depth.
            // We need to properly scale the cube to match UI pixels.
            // 1 UI unit = 1 World Unit in this mode (mostly, depending on scale).
            
            // Create a dedicated child for the 3D model
            var modelContainer = GetChild(root, "3DModel"); 
            if(modelContainer == null)
            {
                var go = new GameObject("3DModel");
                go.transform.SetParent(root, false);
                modelContainer = go.transform;
            }
            
            // Re-create cube if not exists? Or just update.
            // Simplified: Clear old model wrapper logic for idempotency check?
            // For now, let's just make sure we don't spam create.
            if(modelContainer.childCount == 0)
            {
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.SetParent(modelContainer, false);
                
                // Scale: Width ~ 200, Height ~ 300, Depth ~ 50 (Thickness)
                // Base 196x273 is approx 0.7 scale. 
                // We want matches w, h
                // Z depth arbitrary
                cube.transform.localScale = new Vector3(w * 0.9f, h * 0.9f, 40); 
                // Rotation: Slight tilt to show 3D nature
                cube.transform.localRotation = Quaternion.Euler(0, -15, 0);

                 // Remove Collider if we want to rely on UI click info (interaction via invisible button)
                var col = cube.GetComponent<Collider>();
                if(col != null) Object.DestroyImmediate(col);

                // Material
                var rend = cube.GetComponent<Renderer>();
                if(rend != null)
                {
                    // Load Texture
                    var sprite = Resources.Load<Sprite>("Textures/h2511_card_frame_secondary");
                    if(sprite == null) sprite = Resources.Load<Sprite>("Textures/h2511_card_frame");
                    
                    if(sprite != null)
                    {
                        // Create material at runtime
                        var mat = new Material(Shader.Find("Standard"));
                        mat.mainTexture = sprite.texture;
                        mat.color = Color.white; 
                        rend.material = mat;
                    }
                }
            }

            // Interaction: Invisible Button on Root (RectTransform)
            var rootImg = EnsureComponent<Image>(root.gameObject);
            rootImg.color = new Color(0,0,0,0); // Invisible
            EnsureComponent<Button>(root.gameObject);
        }
    }
}
