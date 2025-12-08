using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using DG.Tweening;

namespace Game.UI
{
    /// <summary>
    /// カードのめくりアニメーションを制御するコンポーネント
    /// クリックで裏面から表面にフリップする
    /// </summary>
    public class CardFlipController : MonoBehaviour, IPointerClickHandler
    {
        [Header("References")]
        [SerializeField] private GameObject frontFace;
        [SerializeField] private GameObject backFace;
        [SerializeField] private Image backImage;
        
        [Header("Animation Settings")]
        [SerializeField] private float flipDuration = 0.4f;
        [SerializeField] private Color backColor = new Color(0.15f, 0.15f, 0.25f, 1f);
        [SerializeField] private Ease flipEase = Ease.OutBack;
        
        private bool isRevealed = false;
        private bool isAnimating = false;
        private Action onRevealCallback;
        private RectTransform rectTransform;
        
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }
        
        private int cardRarity = 3;
        
        /// <summary>
        /// カードを裏面状態で初期化（レアリティ指定版）
        /// </summary>
        public void SetupForReveal(int rarity, Action onReveal = null)
        {
            cardRarity = rarity;
            onRevealCallback = onReveal;
            isRevealed = false;
            
            // Create back face if not assigned
            if (backFace == null)
            {
                backFace = new GameObject("BackFace");
                backFace.transform.SetParent(transform, false);
                backFace.transform.SetAsLastSibling();
                
                backImage = backFace.AddComponent<Image>();
                backImage.color = backColor;
                
                // Match parent size
                RectTransform rt = backFace.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                
                // Add "?" text
                GameObject textObj = new GameObject("QuestionMark");
                textObj.transform.SetParent(backFace.transform, false);
                Text text = textObj.AddComponent<Text>();
                text.text = "?";
                text.fontSize = 48;
                text.alignment = TextAnchor.MiddleCenter;
                text.color = new Color(1f, 1f, 1f, 0.4f);
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                
                RectTransform textRt = textObj.GetComponent<RectTransform>();
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.offsetMin = Vector2.zero;
                textRt.offsetMax = Vector2.zero;
            }
            
            // Show back
            backFace.SetActive(true);
            
            // Slight scale animation on spawn
            transform.localScale = Vector3.zero;
            transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
        }
        
        /// <summary>
        /// カードを裏面状態で初期化（互換性用）
        /// </summary>
        public void SetupForReveal(Action onReveal = null)
        {
            SetupForReveal(3, onReveal);
        }
        
        /// <summary>
        /// クリックでカードをめくる
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isRevealed && !isAnimating)
            {
                Reveal();
            }
        }
        
        /// <summary>
        /// カードをめくるアニメーション
        /// </summary>
        public void Reveal(bool instant = false)
        {
            if (isRevealed) return;
            
            isRevealed = true;
            
            if (instant)
            {
                if (backFace != null) backFace.SetActive(false);
                onRevealCallback?.Invoke();
                return;
            }
            
            isAnimating = true;
            
            Vector3 originalScale = transform.localScale;
            float halfDuration = flipDuration * 0.5f;
            
            // Get expansion scale based on rarity
            float expansionMultiplier = GetRarityExpansion();
            Vector3 expandedScale = originalScale * expansionMultiplier;
            
            Sequence flipSequence = DOTween.Sequence();
            
            // Phase 1: Flip closed (ScaleX → 0) while starting to expand Y
            flipSequence.Append(
                transform.DOScaleX(0, halfDuration).SetEase(Ease.InQuad)
            );
            flipSequence.Join(
                transform.DOScaleY(expandedScale.y, halfDuration).SetEase(Ease.OutQuad)
            );
            
            // Switch face at midpoint
            flipSequence.AppendCallback(() => {
                if (backFace != null) backFace.SetActive(false);
            });
            
            // Phase 2: Flip open (ScaleX → expanded) - fully expanded state
            flipSequence.Append(
                transform.DOScaleX(expandedScale.x, halfDuration).SetEase(flipEase)
            );
            
            // Phase 3: Return to original size with bounce
            flipSequence.Append(
                transform.DOScale(originalScale, 0.3f).SetEase(GetRarityEase())
            );
            
            flipSequence.OnComplete(() => {
                isAnimating = false;
                onRevealCallback?.Invoke();
            });
        }
        
        /// <summary>
        /// レアリティに応じた拡大倍率
        /// </summary>
        private float GetRarityExpansion()
        {
            switch (cardRarity)
            {
                case 5: return 1.5f;  // 5-star: 1.5x expansion
                case 4: return 1.3f;  // 4-star: 1.3x expansion
                default: return 1.1f; // 3-star and below: 1.1x expansion
            }
        }
        
        /// <summary>
        /// レアリティに応じたイージング
        /// </summary>
        private Ease GetRarityEase()
        {
            switch (cardRarity)
            {
                case 5: return Ease.OutElastic;  // Dramatic bounce
                case 4: return Ease.OutBack;     // Medium bounce
                default: return Ease.OutQuad;    // Simple ease
            }
        }
        
        /// <summary>
        /// 即座に表示（スキップ時）
        /// </summary>
        public void RevealInstant()
        {
            Reveal(true);
        }
        
        /// <summary>
        /// 既にめくられているか
        /// </summary>
        public bool IsRevealed => isRevealed;
    }
}
