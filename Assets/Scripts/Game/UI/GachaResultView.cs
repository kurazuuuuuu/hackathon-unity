using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System;
using Game.Gacha;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.UI
{
    public class GachaResultView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private Transform cardGridParent;
        [SerializeField] private Button btnSkip;
        [SerializeField] private Button btnClose;

        [Header("Prefabs (Optional)")]
        [SerializeField] private GameObject cardResultItemPrefab; // If null, we generate simple ones

        private List<CardDataBase> currentResults;
        private Action onCompleteCallback;
        private bool isRevealing = false;
        private bool skipRequested = false;

        private void Awake()
        {
            // Removed EnsureUI(false); to rely strictly on Prefab structure
            if (resultPanel != null) resultPanel.SetActive(false);
            if (btnSkip != null) btnSkip.onClick.AddListener(OnSkipClicked);
            if (btnClose != null) btnClose.onClick.AddListener(OnCloseClicked);
        }

        public void ShowResults(List<CardDataBase> cards, Action onComplete)
        {
            currentResults = cards;
            onCompleteCallback = onComplete;
            skipRequested = false;
            
            // Ensure UI elements exist before accessing
            EnsureUI(false);
            
            if (resultPanel != null) resultPanel.SetActive(true);
            if (btnClose != null) btnClose.gameObject.SetActive(false); // Hide close button until done
            if (btnSkip != null) btnSkip.gameObject.SetActive(true);

            // Clear old children
            foreach (Transform child in cardGridParent)
            {
                Destroy(child.gameObject);
            }

            StartCoroutine(ResultsSequence());
        }

        private IEnumerator ResultsSequence()
        {
            isRevealing = true;
            revealedCount = 0;
            
            // Spawn all cards face-down first
            List<CardFlipController> flipControllers = new List<CardFlipController>();
            
            foreach (var card in currentResults)
            {
                GameObject cardObj = SpawnCardFaceDown(card);
                if (cardObj != null)
                {
                    CardFlipController flipController = cardObj.GetComponent<CardFlipController>();
                    if (flipController == null)
                    {
                        flipController = cardObj.AddComponent<CardFlipController>();
                    }
                    
                    // Setup for click reveal with rarity info
                    flipController.SetupForReveal(card.Rarity, () => {
                        revealedCount++;
                        // Check if all cards revealed
                        if (revealedCount >= currentResults.Count)
                        {
                            OnAllCardsRevealed();
                        }
                    });
                    
                    flipControllers.Add(flipController);
                }
                
                // Small stagger for spawning
                yield return new WaitForSeconds(0.1f);
            }
            
            // Wait until skip or all revealed
            while (!skipRequested && revealedCount < currentResults.Count)
            {
                yield return null;
            }
            
            // If skip was requested, reveal all remaining
            if (skipRequested)
            {
                foreach (var fc in flipControllers)
                {
                    if (!fc.IsRevealed)
                    {
                        fc.RevealInstant();
                    }
                }
            }
            
            isRevealing = false;
        }
        
        private void OnAllCardsRevealed()
        {
            if (btnSkip != null) btnSkip.gameObject.SetActive(false);
            if (btnClose != null) btnClose.gameObject.SetActive(true);
        }
        
        private int revealedCount = 0;
        
        /// <summary>
        /// カードを裏面状態で生成する（クリックでめくる用）
        /// </summary>
        private GameObject SpawnCardFaceDown(CardDataBase card)
        {
            GameObject itemObj = null;
            
            // Priority 1: Use assigned prefab (if set in inspector)
            if (cardResultItemPrefab != null)
            {
                itemObj = Instantiate(cardResultItemPrefab, cardGridParent);
            }
            else
            {
                // Priority 2: Load type-specific prefab (PrimaryCard or SupportCard)
                string prefabPath = CardHelper.GetCardPrefabPath(card.CardId);
                GameObject cardPrefab = Resources.Load<GameObject>(prefabPath);
                
                if (cardPrefab != null)
                {
                    itemObj = Instantiate(cardPrefab, cardGridParent);
                    Debug.Log($"[GachaResultView] Loaded prefab: {prefabPath}");
                }
                else
                {
                    // Priority 3: Fallback - create simple card UI
                    Debug.LogWarning($"[GachaResultView] Prefab not found: {prefabPath}, using fallback");
                    itemObj = CreateFallbackCard(card);
                }
            }
            
            if (itemObj == null) return null;
            
            // Try new CardBase first
            CardBase cardBase = itemObj.GetComponent<CardBase>();
            if (cardBase != null)
            {
                cardBase.InitializeForGacha(card);
                cardBase.DisableDrag();
            }
            else
            {
                // Fall back to legacy Card component
                Card cardComponent = itemObj.GetComponent<Card>();
                if (cardComponent != null)
                {
                    cardComponent.InitializeForGacha(card);
                    cardComponent.DisableDrag();
                }
                else
                {
                    // Apply visualizer manually
                    CardVisualizer visualizer = itemObj.GetComponent<CardVisualizer>();
                    if (visualizer == null)
                    {
                        visualizer = itemObj.AddComponent<CardVisualizer>();
                    }
                    visualizer.ApplyRarityStyle(card.Rarity);
                }
            }
            
            // Force rebuild layout for child VerticalLayoutGroup to work correctly
            Canvas.ForceUpdateCanvases();
            RectTransform itemRect = itemObj.GetComponent<RectTransform>();
            if (itemRect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(itemRect);
            }
            
            return itemObj;
        }

        private void SpawnCard(CardDataBase card, bool instantReveal)
        {
            GameObject itemObj = null;
            
            // Priority 1: Use assigned prefab
            if (cardResultItemPrefab != null)
            {
                itemObj = Instantiate(cardResultItemPrefab, cardGridParent);
            }
            else
            {
                // Priority 2: Load Card.prefab from Resources
                GameObject cardPrefab = Resources.Load<GameObject>("Prefabs/Card");
                if (cardPrefab != null)
                {
                    itemObj = Instantiate(cardPrefab, cardGridParent);
                }
                else
                {
                    // Priority 3: Fallback - create simple card UI
                    itemObj = CreateFallbackCard(card);
                }
            }
            
            if (itemObj == null) return;
            
            // Try to initialize using Card component
            Card cardComponent = itemObj.GetComponent<Card>();
            if (cardComponent != null)
            {
                cardComponent.InitializeForGacha(card);
                
                // Disable drag for gacha result display
                cardComponent.DisableDrag();
            }
            else
            {
                // Fallback: Apply visualizer manually
                CardVisualizer visualizer = itemObj.GetComponent<CardVisualizer>();
                if (visualizer == null)
                {
                    visualizer = itemObj.AddComponent<CardVisualizer>();
                }
                visualizer.ApplyRarityStyle(card.Rarity);
            }
        }
        
        private GameObject CreateFallbackCard(CardDataBase card)
        {
            GameObject itemObj = new GameObject($"Card_{card.CardName}");
            itemObj.transform.SetParent(cardGridParent, false);
            
            Image img = itemObj.AddComponent<Image>();
            img.color = GetRarityColor(card.Rarity);

            LayoutElement le = itemObj.AddComponent<LayoutElement>();
            le.preferredWidth = 100;
            le.preferredHeight = 150;

            GameObject txtObj = new GameObject("Label");
            txtObj.transform.SetParent(itemObj.transform, false);
            Text t = txtObj.AddComponent<Text>();
            t.text = $"{card.CardName}\n☆{card.Rarity}";
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (t.font == null) t.font = Resources.FindObjectsOfTypeAll<Font>()[0];
            t.resizeTextForBestFit = true;
            
            RectTransform rt = txtObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            
            return itemObj;
        }

        private void ApplyGlowEffect(GameObject itemObj, int rarity)
        {
            // Skip for low rarity
            if (rarity < 3) return;
            
            // Find the Image component (could be on root or child)
            Image cardImg = itemObj.GetComponentInChildren<Image>();
            if (cardImg == null) return;
            
            RectTransform cardRect = cardImg.GetComponent<RectTransform>();
            if (cardRect == null) return;
            
            // Create Glow as CHILD of the card object, then reorder
            GameObject glowObj = new GameObject("GlowEffect");
            glowObj.transform.SetParent(cardImg.transform, false);
            glowObj.transform.SetAsFirstSibling(); // First child = rendered behind siblings
            
            Image glowImg = glowObj.AddComponent<Image>();
            RectTransform glowRect = glowObj.GetComponent<RectTransform>();
            
            // Center the glow on the card
            glowRect.anchorMin = new Vector2(0.5f, 0.5f);
            glowRect.anchorMax = new Vector2(0.5f, 0.5f);
            glowRect.pivot = new Vector2(0.5f, 0.5f);
            glowRect.anchoredPosition = Vector2.zero;
            
            // Make glow larger than the card (extend outward)
            Vector2 cardSize = cardRect.sizeDelta;
            if (cardSize == Vector2.zero)
            {
                // If sizeDelta is 0, the card might be using anchors for sizing
                cardSize = new Vector2(cardRect.rect.width, cardRect.rect.height);
            }
            float glowPadding = 40f; // pixels of glow extending outside
            glowRect.sizeDelta = cardSize + new Vector2(glowPadding * 2, glowPadding * 2);
            glowRect.localScale = Vector3.one;
            
            // Apply soft glow shader
            Shader glowShader = Shader.Find("UI/SoftGlow");
            if (glowShader == null)
            {
                glowShader = Resources.Load<Shader>("Shaders/SoftGlow");
            }
            
            Color glowColor = GetGlowColor(rarity);
            
            if (glowShader != null)
            {
                Material mat = new Material(glowShader);
                mat.SetColor("_GlowColor", glowColor);
                glowImg.material = mat;
                glowImg.color = Color.white; // Let shader handle color
            }
            else
            {
                // Fallback: just use color with transparency
                glowImg.color = glowColor;
            }
            
            // Add pulsing animation
            GlowPulse pulse = glowObj.AddComponent<GlowPulse>();
            pulse.Initialize(glowColor, rarity);
        }
        
        private Color GetGlowColor(int rarity)
        {
            switch(rarity)
            {
                case 5: return new Color(1.0f, 0.8f, 0.0f, 0.8f); // Gold
                case 4: return new Color(1.0f, 0.2f, 1.0f, 0.7f); // Magenta
                case 3: return new Color(0.2f, 1.0f, 1.0f, 0.6f); // Cyan
                default: return Color.clear;
            }
        }

        public static Color GetRarityColor(int rarity)
        {
            switch(rarity)
            {
                case 5: return Color.yellow; // Gold
                case 4: return new Color(0.8f, 0, 0.8f); // Purple
                case 3: return Color.cyan; // Light Blue
                default: return Color.gray;
            }
        }

        private void OnSkipClicked()
        {
            skipRequested = true;
        }

        private void OnCloseClicked()
        {
            if (resultPanel != null) resultPanel.SetActive(false);
            onCompleteCallback?.Invoke();
        }

        // --- Editor Tools ---
#if UNITY_EDITOR
        [ContextMenu("Create Result UI")]
        private void CreateResultUI()
        {
            EnsureUI(true);
        }

        [ContextMenu("Spawn Dummy Card")]
        private void SpawnDummyCard()
        {
            if (cardGridParent == null) EnsureUI(false);
            
            var dummyCard = ScriptableObject.CreateInstance<CardData>();
            
            // Use reflection to set private fields for testing since properties are read-only
            var type = typeof(CardData);
            type.GetField("cardName", global::System.Reflection.BindingFlags.NonPublic | global::System.Reflection.BindingFlags.Instance)?.SetValue(dummyCard, "Dummy Card 5*");
            type.GetField("rarity", global::System.Reflection.BindingFlags.NonPublic | global::System.Reflection.BindingFlags.Instance)?.SetValue(dummyCard, 5);
            
            SpawnCard(dummyCard, false); 
        }
#endif

        // EnsureUI acts as a runtime fallback or editor tool
        public void EnsureUI(bool markDirty)
        {
            // For Prefab usage, we want everything to be children of THIS object.
            Transform targetParent = this.transform;

            // Create or Find Panel
            if (resultPanel == null)
            {
                // 1. Check if THIS object is the panel
                if (GetComponent<Image>() != null && name.Contains("Panel"))
                {
                    resultPanel = this.gameObject;
                }
                else
                {
                    // 2. Try to find existing child
                    Transform existing = targetParent.Find("Panel_Result");
                    if (existing != null)
                    {
                        resultPanel = existing.gameObject;
                    }
                    else
                    {
                        // 3. Create new child
                        GameObject panel = new GameObject("Panel_Result");
                        panel.transform.SetParent(targetParent, false); 
                        panel.transform.SetAsLastSibling();
                        
                        Image img = panel.AddComponent<Image>();
                        img.color = new Color(0, 0, 0, 0.9f); // Dark background

                        RectTransform rect = panel.GetComponent<RectTransform>();
                        rect.anchorMin = Vector2.zero;
                        rect.anchorMax = Vector2.one;
                        rect.sizeDelta = Vector2.zero;
                        
                        resultPanel = panel;
                    }
                }
            }

            // Create or Find Grid
            if (cardGridParent == null)
            {
                Transform existing = resultPanel.transform.Find("Grid_Cards");
                if (existing != null)
                {
                    cardGridParent = existing;
                }
                else
                {
                    GameObject gridObj = new GameObject("Grid_Cards");
                    gridObj.transform.SetParent(resultPanel.transform, false);
                    gridObj.AddComponent<GridLayoutGroup>();
                    cardGridParent = gridObj.transform;
                }
            }
            
            // Get actual Canvas size for responsive layout
            RectTransform canvasRect = null;
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                canvasRect = canvas.GetComponent<RectTransform>();
            }
            
            // Calculate sizes based on Canvas dimensions (or use defaults)
            float canvasWidth = canvasRect != null ? canvasRect.rect.width : 3840f;
            float canvasHeight = canvasRect != null ? canvasRect.rect.height : 2160f;
            
            // Prefab native size (MUST match actual Prefab RectTransform size)
            Vector2 prefabSize = new Vector2(280, 390);
            
            // Grid layout: 5 columns, 2 rows
            int columns = 5;
            int rows = 2;
            
            // Fixed spacing in Prefab units
            Vector2 spacing = new Vector2(30, 30);
            
            // Calculate grid size in Prefab units (without scaling)
            float gridWidthNative = prefabSize.x * columns + spacing.x * (columns - 1);
            float gridHeightNative = prefabSize.y * rows + spacing.y * (rows - 1);
            
            // Calculate scale to fit canvas (90% width, 65% height - leaving room for buttons)
            float targetWidth = canvasWidth * 0.90f;
            float targetHeight = canvasHeight * 0.65f;
            float scaleX = targetWidth / gridWidthNative;
            float scaleY = targetHeight / gridHeightNative;
            float scale = Mathf.Min(scaleX, scaleY);
            
            // Apply scale to grid (NOT to cellSize, to preserve internal layout)
            RectTransform gridRect = cardGridParent.GetComponent<RectTransform>();
            if (gridRect != null)
            {
                gridRect.anchorMin = new Vector2(0.5f, 0.5f);
                gridRect.anchorMax = new Vector2(0.5f, 0.5f);
                gridRect.pivot = new Vector2(0.5f, 0.5f);
                gridRect.anchoredPosition = new Vector2(0, canvasHeight * 0.08f); // Move up more
                gridRect.sizeDelta = new Vector2(gridWidthNative, gridHeightNative);
                gridRect.localScale = new Vector3(scale, scale, 1f); // Scale the grid!
            }
            
            // Use Prefab's native size for cellSize (NOT scaled)
            GridLayoutGroup gridLayout = cardGridParent.GetComponent<GridLayoutGroup>();
            if (gridLayout != null)
            {
                gridLayout.cellSize = prefabSize;
                gridLayout.spacing = spacing;
                gridLayout.childAlignment = TextAnchor.MiddleCenter;
                gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayout.constraintCount = columns;
            }

            // Create or Find Skip Button
            if (btnSkip == null)
            {
                Transform existing = resultPanel.transform.Find("Btn_Skip");
                if (existing != null)
                {
                    btnSkip = existing.GetComponent<Button>();
                }
                else
                {
                    GameObject btnObj = new GameObject("Btn_Skip");
                    btnObj.transform.SetParent(resultPanel.transform, false);
                    
                    Image img = btnObj.AddComponent<Image>();
                    img.color = Color.white;

                    btnSkip = btnObj.AddComponent<Button>();
                    
                    RectTransform rect = btnObj.GetComponent<RectTransform>();
                    rect.anchorMin = new Vector2(1, 0);
                    rect.anchorMax = new Vector2(1, 0);
                    rect.pivot = new Vector2(1, 0);
                    rect.anchoredPosition = new Vector2(-50, 50);
                    rect.sizeDelta = new Vector2(120, 50);

                    GameObject txt = new GameObject("Text");
                    txt.transform.SetParent(btnObj.transform, false);
                    Text t = txt.AddComponent<Text>();
                    t.text = "SKIP";
                    t.color = Color.black;
                    t.alignment = TextAnchor.MiddleCenter;
                    t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    if (t.font == null) t.font = Resources.FindObjectsOfTypeAll<Font>()[0];
                     RectTransform txtRect = txt.GetComponent<RectTransform>();
                    txtRect.anchorMin = Vector2.zero;
                    txtRect.anchorMax = Vector2.one;
                    txtRect.sizeDelta = Vector2.zero;
                }
            }

            // Create or Find Close Button
            if (btnClose == null)
            {
                Transform existing = resultPanel.transform.Find("Btn_Close");
                if (existing != null)
                {
                    btnClose = existing.GetComponent<Button>();
                }
                else
                {
                    GameObject btnObj = new GameObject("Btn_Close");
                    btnObj.transform.SetParent(resultPanel.transform, false);
                    
                    Image img = btnObj.AddComponent<Image>();
                    img.color = Color.green;

                    btnClose = btnObj.AddComponent<Button>();
                    
                    RectTransform rect = btnObj.GetComponent<RectTransform>();
                    rect.anchorMin = new Vector2(0.5f, 0);
                    rect.anchorMax = new Vector2(0.5f, 0);
                    rect.pivot = new Vector2(0.5f, 0);
                    rect.anchoredPosition = new Vector2(0, 50);
                    rect.sizeDelta = new Vector2(200, 60);

                    GameObject txt = new GameObject("Text");
                    txt.transform.SetParent(btnObj.transform, false);
                    Text t = txt.AddComponent<Text>();
                    t.text = "CLOSE";
                    t.color = Color.white;
                    t.alignment = TextAnchor.MiddleCenter;
                    t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    if (t.font == null) t.font = Resources.FindObjectsOfTypeAll<Font>()[0];
                    
                    RectTransform txtRect = txt.GetComponent<RectTransform>();
                    txtRect.anchorMin = Vector2.zero;
                    txtRect.anchorMax = Vector2.one;
                    txtRect.sizeDelta = Vector2.zero;
                    
                    // Initially active for seeing it, but script hides it at runtime
                }
            }
            
            #if UNITY_EDITOR
            if (markDirty) EditorUtility.SetDirty(this);
            #endif
        }
    }
}
