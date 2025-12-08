using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Simple pulsing glow effect for UI elements
    /// </summary>
    public class GlowPulse : MonoBehaviour
    {
        private Image glowImage;
        private Color baseColor;
        private float speed = 3f;
        private float minAlpha = 0.3f;
        private float maxAlpha = 0.8f;
        
        public void Initialize(Color color, int rarity)
        {
            glowImage = GetComponent<Image>();
            baseColor = color;
            
            // Adjust pulse parameters based on rarity
            switch(rarity)
            {
                case 5:
                    speed = 5f;
                    minAlpha = 0.5f;
                    maxAlpha = 1.0f;
                    break;
                case 4:
                    speed = 3f;
                    minAlpha = 0.4f;
                    maxAlpha = 0.9f;
                    break;
                case 3:
                    speed = 2f;
                    minAlpha = 0.3f;
                    maxAlpha = 0.7f;
                    break;
            }
        }
        
        private void Update()
        {
            if (glowImage == null) return;
            
            // Pulsing alpha
            float pulse = Mathf.Sin(Time.time * speed) * 0.5f + 0.5f; // 0 to 1
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, pulse);
            
            Color c = baseColor;
            c.a = alpha;
            glowImage.color = c;
        }
    }
}
