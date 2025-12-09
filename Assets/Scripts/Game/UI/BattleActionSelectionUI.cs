using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Game.Battle;

namespace Game.UI
{
    public class BattleActionSelectionUI : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Button attackButton;
        [SerializeField] private Button skillButton;
        [SerializeField] private Button closeButton; // Click outside to close

        private Action<bool> onSelection; // true=Attack, false=Skill
        private CardBase currentCard;

        private void Awake()
        {
            // Auto-bind CanvasGroup
            if (canvasGroup == null) 
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
            
            CreateUI();

            // Auto-find close button if missing (it's not created by CreateUI)
            if (closeButton == null) closeButton = GetComponent<Button>();
            closeButton?.onClick.AddListener(Close);
            
            // Initially hidden
            Close();
        }

        private Transform cardPreviewContainer;
        private GameObject currentPreview;

        private void CreateUI()
        {
            // Create card preview container on the right
            if (cardPreviewContainer == null)
            {
                GameObject containerObj = new GameObject("CardPreviewContainer");
                containerObj.transform.SetParent(transform, false);
                RectTransform rect = containerObj.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(400, 0); // Right side
                cardPreviewContainer = containerObj.transform;
            }

            // Create buttons on the left
            if (attackButton == null)
            {
                // Top button
                attackButton = CreateButton("Attack", () => OnActionSelected(true), new Vector2(-400, 150));
            }
            if (skillButton == null)
            {
                // Bottom button
                skillButton = CreateButton("Skill", () => OnActionSelected(false), new Vector2(-400, -150));
            }
        }

        private Button CreateButton(string label, UnityEngine.Events.UnityAction onClick, Vector2 position)
        {
            GameObject buttonObj = new GameObject(label + "Button");
            buttonObj.transform.SetParent(transform, false);
            
            // 4K Canvas用に大きなボタンサイズ (600x200)
            RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(600, 200);
            rectTransform.anchoredPosition = position;
            
            Button button = buttonObj.AddComponent<Button>();
            button.onClick.AddListener(onClick);
            
            // Button background
            UnityEngine.UI.Image image = buttonObj.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            
            // Button text with AutoSize
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = label;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            
            // Enable AutoSize
            text.enableAutoSizing = true;
            text.fontSizeMin = 24;
            text.fontSizeMax = 72;
            
            return button;
        }

        public void Show(CardBase card, Action<bool> callback)
        {
            currentCard = card;
            onSelection = callback;

            // Update Preview
            if (currentPreview != null) Destroy(currentPreview);
            
            if (card != null && cardPreviewContainer != null)
            {
                // Create a visual copy of the card
                currentPreview = Instantiate(card.gameObject, cardPreviewContainer);
                
                // Reset transform
                currentPreview.transform.localPosition = Vector3.zero;
                currentPreview.transform.localRotation = Quaternion.identity;
                currentPreview.transform.localScale = Vector3.one * 2.5f; // Scale up 2.5x
                
                // Disable logic components to make it purely visual
                var cardBase = currentPreview.GetComponent<CardBase>();
                if (cardBase != null) Destroy(cardBase);
                
                // Clean up other scripts
                var components = currentPreview.GetComponents<MonoBehaviour>();
                foreach (var comp in components)
                {
                    if (comp is not UnityEngine.UI.Image && comp is not TMPro.TextMeshProUGUI)
                    {
                        Destroy(comp);
                    }
                }
                
                // Ensure raycasts are blocked on the preview? No, keep it visual only
                var canvasGroup = currentPreview.GetComponent<CanvasGroup>();
                if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
            }

            gameObject.SetActive(true);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
            }
        }

        public void Close()
        {
            if (currentPreview != null) Destroy(currentPreview);
            
            gameObject.SetActive(false);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
            }
        }

        private void OnActionSelected(bool isAttack)
        {
            onSelection?.Invoke(isAttack);
            Close();
        }
    }
}
