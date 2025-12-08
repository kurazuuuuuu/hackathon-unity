using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// カードの見た目をレアリティに応じて変更するコンポーネント
    /// </summary>
    public class CardVisualizer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image cardBackground;
        [SerializeField] private Image glowImage;
        
        [Header("Rarity Colors")]
        [SerializeField] private Color star5Color = new Color(1.0f, 0.85f, 0.0f, 1.0f); // Gold
        [SerializeField] private Color star4Color = new Color(0.8f, 0.2f, 0.8f, 1.0f);  // Purple
        [SerializeField] private Color star3Color = new Color(0.2f, 0.9f, 0.9f, 1.0f);  // Cyan
        [SerializeField] private Color defaultColor = new Color(0.5f, 0.5f, 0.5f, 1.0f); // Gray
        
        [Header("Glow Settings")]
        [SerializeField] private float glowPadding = 40f;
        [SerializeField] private bool enableGlow = true;
        
        private GlowPulse glowPulse;
        
        /// <summary>
        /// レアリティに基づいてカードの見た目を設定
        /// グローエフェクトのみ適用（背景色は変更しない）
        /// </summary>
        public void ApplyRarityStyle(int rarity)
        {
            Color rarityColor = GetRarityColor(rarity);
            
            // Apply glow effect only for rarity 3+
            if (enableGlow && rarity >= 3)
            {
                CreateGlowEffect(rarity, rarityColor);
            }
        }
        
        /// <summary>
        /// レアリティから色を取得
        /// </summary>
        public Color GetRarityColor(int rarity)
        {
            switch (rarity)
            {
                case 5: return star5Color;
                case 4: return star4Color;
                case 3: return star3Color;
                default: return defaultColor;
            }
        }
        
        /// <summary>
        /// グローエフェクトを作成
        /// </summary>
        private void CreateGlowEffect(int rarity, Color baseColor)
        {
            // Find or create glow image
            if (glowImage == null)
            {
                // Create glow as child of card, placed at back (first sibling = rendered first = behind)
                GameObject glowObj = new GameObject("GlowEffect");
                glowObj.transform.SetParent(this.transform, false);
                glowObj.transform.SetAsFirstSibling(); // Behind all other children
                
                glowImage = glowObj.AddComponent<Image>();
                
                RectTransform glowRect = glowObj.GetComponent<RectTransform>();
                RectTransform cardRect = GetComponent<RectTransform>();
                
                // Stretch to fill parent with overflow
                glowRect.anchorMin = Vector2.zero;
                glowRect.anchorMax = Vector2.one;
                glowRect.pivot = new Vector2(0.5f, 0.5f);
                
                // Calculate proportional glow padding (15% of card size)
                float cardWidth = cardRect.rect.width > 0 ? cardRect.rect.width : cardRect.sizeDelta.x;
                float cardHeight = cardRect.rect.height > 0 ? cardRect.rect.height : cardRect.sizeDelta.y;
                if (cardWidth <= 0) cardWidth = 280f; // Default
                if (cardHeight <= 0) cardHeight = 390f;
                float proportionalPadding = Mathf.Max(cardWidth, cardHeight) * 0.12f;
                
                // Expand beyond parent bounds using negative offsets
                glowRect.offsetMin = new Vector2(-proportionalPadding, -proportionalPadding);
                glowRect.offsetMax = new Vector2(proportionalPadding, proportionalPadding);
            }
            
            // Apply glow color with transparency
            Color glowColor = GetGlowColor(rarity);
            glowImage.color = glowColor;
            
            // Try to apply soft glow shader
            Shader glowShader = Shader.Find("UI/SoftGlow");
            if (glowShader == null)
            {
                glowShader = Resources.Load<Shader>("Shaders/SoftGlow");
            }
            
            if (glowShader != null)
            {
                Material mat = new Material(glowShader);
                mat.SetColor("_GlowColor", glowColor);
                glowImage.material = mat;
                glowImage.color = Color.white;
            }
            
            // Add pulse animation
            glowPulse = glowImage.gameObject.GetComponent<GlowPulse>();
            if (glowPulse == null)
            {
                glowPulse = glowImage.gameObject.AddComponent<GlowPulse>();
            }
            glowPulse.Initialize(glowColor, rarity);
        }
        
        /// <summary>
        /// グロー用の色を取得（半透明）
        /// </summary>
        private Color GetGlowColor(int rarity)
        {
            switch (rarity)
            {
                case 5: return new Color(1.0f, 0.8f, 0.0f, 0.8f);
                case 4: return new Color(1.0f, 0.2f, 1.0f, 0.7f);
                case 3: return new Color(0.2f, 1.0f, 1.0f, 0.6f);
                default: return Color.clear;
            }
        }
        
        /// <summary>
        /// グローを無効化
        /// </summary>
        public void DisableGlow()
        {
            if (glowImage != null)
            {
                glowImage.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// グローを有効化
        /// </summary>
        public void EnableGlowEffect()
        {
            if (glowImage != null)
            {
                glowImage.gameObject.SetActive(true);
            }
        }
    }
}
