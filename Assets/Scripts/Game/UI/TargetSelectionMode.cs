using UnityEngine;
using UnityEngine.UI;
using System;

namespace Game.UI
{
    /// <summary>
    /// 対象選択モードを管理
    /// </summary>
    public class TargetSelectionMode : MonoBehaviour
    {
        public static TargetSelectionMode Instance { get; private set; }

        [Header("Visual")]
        [SerializeField] private Color highlightColor = new Color(1f, 0.5f, 0.5f, 0.5f);
        
        private bool isSelecting = false;
        private CardBase sourceCard;
        private bool isAttackMode;
        private Action<CardBase> onTargetSelected;

        public bool IsSelecting => isSelecting;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 対象選択モードを開始
        /// </summary>
        public void StartTargetSelection(CardBase source, bool isAttack, Action<CardBase> callback)
        {
            sourceCard = source;
            isAttackMode = isAttack;
            onTargetSelected = callback;
            isSelecting = true;

            Debug.Log($"[TargetSelection] Started. Source: {source?.Name}, IsAttack: {isAttack}");
            
            // Highlight valid targets (opponent's primary cards)
            HighlightValidTargets(true);
        }

        /// <summary>
        /// 対象が選択された
        /// </summary>
        public void SelectTarget(CardBase target)
        {
            if (!isSelecting) return;

            Debug.Log($"[TargetSelection] Target selected: {target?.Name}");
            
            isSelecting = false;
            HighlightValidTargets(false);
            
            onTargetSelected?.Invoke(target);
        }

        /// <summary>
        /// 対象選択をキャンセル
        /// </summary>
        public void Cancel()
        {
            if (!isSelecting) return;

            Debug.Log("[TargetSelection] Cancelled");
            
            isSelecting = false;
            HighlightValidTargets(false);
            onTargetSelected = null;
        }

        private void HighlightValidTargets(bool highlight)
        {
            // Find all PrimaryCards in opponent's zone
            // For now, highlight all PrimaryCards except the source
            var primaryCards = FindObjectsByType<PrimaryCard>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var pc in primaryCards)
            {
                var card = pc.GetComponent<Card>();
                if (card != null && card != sourceCard)
                {
                    // Add/remove highlight effect
                    var outline = pc.GetComponent<Outline>();
                    if (highlight)
                    {
                        if (outline == null) outline = pc.gameObject.AddComponent<Outline>();
                        outline.effectColor = highlightColor;
                        outline.effectDistance = new Vector2(5, -5);
                    }
                    else
                    {
                        if (outline != null) Destroy(outline);
                    }
                }
            }
        }

        // Note: Cancel can be triggered via UI button instead of Input
    }
}
