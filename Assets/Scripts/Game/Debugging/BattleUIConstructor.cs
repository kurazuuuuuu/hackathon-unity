using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Debugging
{
    [InitializeOnLoad]
    public class BattleUIConstructor : MonoBehaviour
    {
        static BattleUIConstructor()
        {
            EditorApplication.delayCall += () =>
            {
                if (!SessionState.GetBool("ConstructedBattleUI", false))
                {
                    // Only run if we are in a scene with a Canvas (likely BattleScene)
                    if (Object.FindAnyObjectByType<Canvas>() != null)
                    {
                        ConstructUI();
                        SessionState.SetBool("ConstructedBattleUI", true);
                    }
                }
            };
        }

#if UNITY_EDITOR
        [MenuItem("Tools/Battle/Construct UI based on GameSystem")]
        public static void ConstructUI()
        {
            try
            {
                var canvas = FindObjectOfType<Canvas>();
                if (canvas == null)
                {
                    Debug.LogWarning("Scene requires a Canvas.");
                    return;
                }

                // Build UI structure
                BuildUIStructure(canvas);
                
                Debug.Log("Battle UI Construction Complete. Use 'Save UI As Prefab' to save for manual placement.");
            }
            catch (global::System.Exception ex)
            {
                Debug.LogWarning($"BattleUIConstructor error: {ex.Message}");
            }
        }

        [MenuItem("Tools/Battle/Save Current UI As Prefab")]
        public static void SaveUIAsPrefab()
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("No Canvas found in scene.");
                return;
            }

            // Save each major area as separate prefab
            string prefabPath = "Assets/Resources/Prefabs/UI/";
            
            // Ensure directory exists
            if (!global::System.IO.Directory.Exists(prefabPath))
            {
                global::System.IO.Directory.CreateDirectory(prefabPath);
                AssetDatabase.Refresh();
            }

            // Find and save player areas
            var p1Area = canvas.transform.Find("Player1Area");
            if (p1Area != null)
            {
                PrefabUtility.SaveAsPrefabAsset(p1Area.gameObject, prefabPath + "BattlePlayerArea.prefab");
                Debug.Log($"Saved: {prefabPath}BattlePlayerArea.prefab");
            }

            var centerArea = canvas.transform.Find("CenterInfoArea");
            if (centerArea != null)
            {
                PrefabUtility.SaveAsPrefabAsset(centerArea.gameObject, prefabPath + "BattleCenterInfo.prefab");
                Debug.Log($"Saved: {prefabPath}BattleCenterInfo.prefab");
            }

            var actionUI = canvas.transform.Find("ActionSelectionUI");
            if (actionUI != null)
            {
                PrefabUtility.SaveAsPrefabAsset(actionUI.gameObject, prefabPath + "BattleActionSelectionUI.prefab");
                Debug.Log($"Saved: {prefabPath}BattleActionSelectionUI.prefab");
            }

            var resultOverlay = canvas.transform.Find("ResultOverlay");
            if (resultOverlay != null)
            {
                PrefabUtility.SaveAsPrefabAsset(resultOverlay.gameObject, prefabPath + "BattleResultOverlay.prefab");
                Debug.Log($"Saved: {prefabPath}BattleResultOverlay.prefab");
            }

            AssetDatabase.Refresh();
            Debug.Log("UI Prefabs saved. You can now delete the generated UI and place prefabs manually.");
        }

        private static void BuildUIStructure(Canvas canvas)
        {
            try
            {
                if (canvas == null) return;

                // Ensure EventSystem with new Input System module
                var eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
                if (eventSystem == null)
                {
                    var eventSystemGO = new GameObject("EventSystem");
                    eventSystem = eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                }
                
                // Remove old StandaloneInputModule if exists
                var standaloneModule = eventSystem.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                if (standaloneModule != null)
                {
                    Object.DestroyImmediate(standaloneModule);
                }
                
                // Add new Input System module if not present
                if (eventSystem.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>() == null)
                {
                    eventSystem.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
                }

                // 0. Background (New)
                CreateBackground(canvas.transform);

                // 1. Player 1 Area (Bottom) - Balanced for 4K, positioned lower
                var p1Area = CreateArea("Player1Area", canvas.transform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 100), new Vector2(3600, 900)); // Anchored Bottom
                if (p1Area != null) ConstructPlayerArea(p1Area, isPlayer1: true);

                // 2. Player 2 Area (Top) - Balanced for 4K, positioned higher
                var p2Area = CreateArea("Player2Area", canvas.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -100), new Vector2(3600, 900)); // Anchored Top
                if (p2Area != null) ConstructPlayerArea(p2Area, isPlayer1: false);

                // 3. Center Info (Turn, etc) - 4K scaled
                var centerArea = CreateArea("CenterInfoArea", canvas.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(800, 400));
                if (centerArea != null)
                {
                    CreateText(centerArea, "TurnInfoText", "Turn 1", 72, new Vector2(0, 100));
                    var turnTxt = centerArea.Find("TurnInfoText")?.GetComponent<TextMeshProUGUI>();
                    if (turnTxt != null) turnTxt.color = new Color(0.1f, 0.1f, 0.1f); // Dark Text

                    CreateButton(centerArea, "SkipButton", "SKIP TURN", new Vector2(0, -100));
                }

                // 4. Result Overlay (Initially Hidden)
                var resultPanel = CreateArea("ResultOverlay", canvas.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                if (resultPanel != null)
                {
                    var img = resultPanel.GetComponent<Image>();
                    if (img == null) img = resultPanel.gameObject.AddComponent<Image>();
                    img.color = new Color(0, 0, 0, 0.8f);
                    resultPanel.gameObject.SetActive(false);
                    
                    CreateText(resultPanel, "ResultText", "VICTORY", 128, new Vector2(0, 100));
                    CreateButton(resultPanel, "ReturnHomeButton", "Return to Home", new Vector2(0, -200));
                }

                Debug.Log("Battle UI Construction Complete.");
                
                // Ensure Managers exist
                var bm = Object.FindObjectOfType<Game.Battle.BattleManager>();
                if (bm == null)
                {
                    var go = new GameObject("BattleManager");
                    bm = go.AddComponent<Game.Battle.BattleManager>();
                    Debug.Log("Created BattleManager.");
                }

                var cm = Object.FindObjectOfType<Game.CardManager>();
                if (cm == null)
                {
                    var go = new GameObject("CardManager");
                    cm = go.AddComponent<Game.CardManager>();
                    Debug.Log("Created CardManager. Please assign Card Prefab in Inspector.");
                }

                // Ensure Scene Controller (BattleScene) exists
                var sceneController = Object.FindObjectOfType<Game.Scenes.BattleScene>();
                if (sceneController == null)
                {
                    var go = new GameObject("BattleSceneController");
                    sceneController = go.AddComponent<Game.Scenes.BattleScene>();
                    sceneController.useDebugPlayers = true; // Enable debug mode by default
                    Debug.Log("Created BattleSceneController.");
                }

                // Link UI to BattleManager
                 if (bm != null)
                 {
                     var p1Hand = p1Area != null ? p1Area.Find("HandArea") : null;
                     var p2Hand = p2Area != null ? p2Area.Find("HandArea") : null;
                     
                     var p1ZoneT = p1Area != null ? p1Area.Find("PrimaryZone") : null;
                     var p2ZoneT = p2Area != null ? p2Area.Find("PrimaryZone") : null;
                     
                     Game.PrimaryCardZone p1Zone = null;
                     if (p1ZoneT != null)
                     {
                         p1Zone = p1ZoneT.GetComponent<Game.PrimaryCardZone>();
                         if (p1Zone == null) p1Zone = p1ZoneT.gameObject.AddComponent<Game.PrimaryCardZone>();
                     }
                     
                     Game.PrimaryCardZone p2Zone = null;
                     if (p2ZoneT != null)
                     {
                         p2Zone = p2ZoneT.GetComponent<Game.PrimaryCardZone>();
                         if (p2Zone == null) p2Zone = p2ZoneT.gameObject.AddComponent<Game.PrimaryCardZone>();
                     }
                     
                     bm.SetUIReferences(p1Hand, p2Hand, p1Zone, p2Zone);
                     EditorUtility.SetDirty(bm);
                 }

                 // Link BattleScene to BattleManager
                 if (sceneController != null && bm != null)
                 {
                     var so = new SerializedObject(sceneController);
                     var bmProp = so.FindProperty("battleManager");
                     if (bmProp != null)
                     {
                         bmProp.objectReferenceValue = bm;
                         so.ApplyModifiedProperties();
                     }
                 }

                // 5. Action Selection UI (New)
                var actionUI = CreateArea("ActionSelectionUI", canvas.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                if (actionUI != null)
                {
                    // Add Components (check if already exists)
                    if (actionUI.GetComponent<CanvasGroup>() == null)
                        actionUI.gameObject.AddComponent<CanvasGroup>();
                    
                    if (actionUI.GetComponent<Game.UI.BattleActionSelectionUI>() == null)
                        actionUI.gameObject.AddComponent<Game.UI.BattleActionSelectionUI>();
                    
                    // Full screen blocker / close button
                    var img = actionUI.GetComponent<Image>();
                    if (img == null)
                    {
                        img = actionUI.gameObject.AddComponent<Image>();
                        img.color = new Color(0, 0, 0, 0.4f); // Dim background
                    }
                    
                    if (actionUI.GetComponent<Button>() == null)
                        actionUI.gameObject.AddComponent<Button>(); // Clicking bg closes
                    
                    // Buttons Container - 4K scaled
                    var buttons = CreateArea("Buttons", actionUI, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600, 300));
                    if (buttons != null)
                    {
                        var layout = buttons.GetComponent<VerticalLayoutGroup>();
                        if (layout == null)
                        {
                            layout = buttons.gameObject.AddComponent<VerticalLayoutGroup>();
                            layout.spacing = 40; // 4K scaled
                            layout.childAlignment = TextAnchor.MiddleCenter;
                            layout.childControlWidth = false;
                            layout.childControlHeight = false;
                        }
                        
                        // Attack Button - 4K scaled
                        CreateButton(buttons, "AttackButton", "通常攻撃", Vector2.zero);
                        var atkBtn = buttons.Find("AttackButton")?.GetComponent<RectTransform>();
                        if (atkBtn != null) atkBtn.sizeDelta = new Vector2(480, 120);
                        
                        // Skill Button - 4K scaled
                        CreateButton(buttons, "SkillButton", "特殊効果", Vector2.zero);
                        var skillBtn = buttons.Find("SkillButton")?.GetComponent<RectTransform>();
                        if (skillBtn != null) skillBtn.sizeDelta = new Vector2(480, 120);
                    }
                    
                    actionUI.gameObject.SetActive(false); // Hide by default
                }

                Debug.Log("Battle UI Construction Complete.");
            }
            catch (global::System.Exception ex)
            {
               // Suppress or log warning
               Debug.LogWarning($"BattleUIConstructor skipped due to: {ex.GetType().Name} - {ex.Message}");
            }
        }

        private static void CreateBackground(Transform parent)
        {
            var bg = CreateArea("BackgroundLayer", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            if (bg != null)
            {
                bg.SetAsFirstSibling(); // Send to back
                
                // Add Drop Zone for Global/Field Cards (Special)
                var dropZone = bg.gameObject.AddComponent<Game.CardDropZone>();
                // This zone will accept Special cards via CanAcceptCard logic
                
                var img = bg.GetComponent<Image>();
                if (img == null) img = bg.gameObject.AddComponent<Image>();
                
                // Load generated sprite
                Sprite bgSprite = Resources.Load<Sprite>("UI/battle_bg_fantasy");
                if (bgSprite != null)
                {
                    img.sprite = bgSprite;
                    img.color = Color.white;
                }
                else
                {
                    // Fallback
                     img.color = new Color(0.95f, 0.92f, 0.88f, 1.0f); 
                }
            }
        }

        private static Transform CreateArea(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
        {
            if (parent == null)
            {
                Debug.LogWarning($"BattleUIConstructor: Parent is null for area '{name}'. Skipping.");
                return null;
            }

            var tf = parent.Find(name);
            if (tf == null)
            {
                var go = new GameObject(name);
                tf = go.transform;
                tf.SetParent(parent, false);
            }
            
            var rect = tf.GetComponent<RectTransform>();
            if (rect == null) rect = tf.gameObject.AddComponent<RectTransform>();

            // Only set layout if likely new or reset needed. 
            // We'll trust user if it exists, but usually we enforce anchors for "Areas".
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            return tf;
        }

        private static void ConstructPlayerArea(Transform root, bool isPlayer1)
        {
            if (root == null) return;

            // Define vertical direction (1 for P1/Bottom, -1 for P2/Top)
            float vertDir = isPlayer1 ? 1 : -1;
            
            // Layout Constants for Genshin Style - Refined
            Color goldTint = new Color(0.8f, 0.7f, 0.4f, 0.2f); // Clearer Gold fill
            Color frameColor = new Color(0.6f, 0.5f, 0.2f, 0.8f); // Stronger Gold/Brown frame
            Color darkFill = new Color(0.1f, 0.1f, 0.1f, 0.6f); // Contrast for text
            Color textColor = new Color(0.2f, 0.2f, 0.2f, 1f); // Dark Text for light bg

            // 1. Hand Area (Bottom/Top Center) - Balanced for 4K
            Vector2 handAnchor = isPlayer1 ? new Vector2(0.5f, 0) : new Vector2(0.5f, 1);
            Vector2 handPos = new Vector2(0, 100 * vertDir); // Close to edge
            var handArea = CreateArea("HandArea", root, handAnchor, handAnchor, handPos, new Vector2(2400, 500));
            if (handArea != null)
            {
                 // Layout Group
                 var layout = handArea.GetComponent<HorizontalLayoutGroup>();
                 if (layout == null) layout = handArea.gameObject.AddComponent<HorizontalLayoutGroup>();
                 
                 // Update properties
                 layout.childAlignment = TextAnchor.MiddleCenter;
                 layout.spacing = -30; // Slight overlap
                 layout.childControlHeight = false; 
                 layout.childControlWidth = false;
            }

            // 2. Primary Zone (The Focus - Character Cards) - Smaller, closer to center
            Vector2 zonePos = new Vector2(0, 550 * vertDir); // Positioned toward center but within bounds
            var primaryZone = CreateArea("PrimaryZone", root, handAnchor, handAnchor, zonePos, new Vector2(1800, 500));
            
            if (primaryZone != null)
            {
                // No main background, just slots
                for (int i = 0; i < 3; i++)
                {
                     // Evenly spaced slots
                    float xOffset = (i - 1) * 550; // Spacing for slots
                    var slot = CreateArea($"Slot_{i}", primaryZone, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(xOffset, 0), new Vector2(500, 480)); // Card slot
                    
                    if (slot != null)
                    {
                        AddBackground(slot, goldTint);
                        AddOutline(slot, frameColor, 6);

                        var dropZone = slot.gameObject.AddComponent<Game.CardDropZone>();
                        
                        var status = CreateArea("StatusPanel", slot, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -40), new Vector2(0, 80));
                        if(status != null) AddBackground(status, darkFill);
                    }
                }
            }

            // 3. Player Info (Sidebars) - Positioned near edge
            Vector2 infoAnchor = isPlayer1 ? new Vector2(0, 0) : new Vector2(0, 1);
            Vector2 infoPos = isPlayer1 ? new Vector2(200, 200) : new Vector2(200, -200);
            
            var infoArea = CreateArea("PlayerInfo", root, infoAnchor, infoAnchor, infoPos, new Vector2(400, 200));
            if (infoArea != null)
            {
                AddBackground(infoArea, new Color(1f, 1f, 1f, 0.9f));
                AddOutline(infoArea, frameColor, 4);
                
                CreateText(infoArea, "NameText", isPlayer1 ? "Player" : "Enemy", 48, new Vector2(0, 50));
                var nameTxt = infoArea.Find("NameText")?.GetComponent<TextMeshProUGUI>();
                if (nameTxt != null) nameTxt.color = textColor;

                CreateText(infoArea, "HPText", "HP: 20", 64, new Vector2(0, -30));
                var hpTxt = infoArea.Find("HPText")?.GetComponent<TextMeshProUGUI>();
                if (hpTxt != null) hpTxt.color = new Color(0.8f, 0.2f, 0.2f);
            }

            // 4. Deck & Grave (Right side)
            Vector2 deckAnchor = isPlayer1 ? new Vector2(1, 0) : new Vector2(1, 1);
            Vector2 deckPos = isPlayer1 ? new Vector2(-180, 200) : new Vector2(-180, -200);
            
            // Deck
            var deck = CreateArea("Deck", root, deckAnchor, deckAnchor, deckPos + new Vector2(-90, 0), new Vector2(150, 200));
            if (deck != null)
            {
                AddBackground(deck, new Color(0.4f, 0.3f, 0.2f, 0.8f));
                AddOutline(deck, frameColor, 4);
                CreateText(deck, "Label", "DECK", 32, Vector2.zero);
            }

            // Grave
            var grave = CreateArea("Grave", root, deckAnchor, deckAnchor, deckPos + new Vector2(90, 0), new Vector2(150, 200));
            if (grave != null)
            {
                AddBackground(grave, new Color(0.2f, 0.2f, 0.2f, 0.8f));
                CreateText(grave, "Label", "GRAVE", 32, Vector2.zero);
            }
        }
        
        private static void AddBackground(Transform tf, Color color)
        {
            if (tf == null) return;
            var img = tf.GetComponent<Image>();
            if (img == null) img = tf.gameObject.AddComponent<Image>();
            img.color = color;
        }

        private static void AddOutline(Transform tf, Color color, int thickness)
        {
            if (tf == null) return;
            var outline = tf.GetComponent<Outline>();
            if (outline == null) outline = tf.gameObject.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(thickness, -thickness);
        }

        private static void CreateText(Transform parent, string name, string defaultText, int fontSize, Vector2 pos)
        {
             if (parent == null) return;

             var tf = parent.Find(name);
             if (tf == null)
             {
                 var go = new GameObject(name);
                 tf = go.transform;
                 tf.SetParent(parent, false);
             }
             
             var rect = tf.GetComponent<RectTransform>();
             if (rect == null) rect = tf.gameObject.AddComponent<RectTransform>();
             rect.anchoredPosition = pos;

             var tmp = tf.GetComponent<TextMeshProUGUI>();
             if (tmp == null) tmp = tf.gameObject.AddComponent<TextMeshProUGUI>();
             
             if (string.IsNullOrEmpty(tmp.text)) tmp.text = defaultText;
             tmp.fontSize = fontSize;
             tmp.alignment = TextAlignmentOptions.Center;
             tmp.color = Color.white;
             // Add Shadow for readability
             // if (tf.GetComponent<Shadow>() == null) tf.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(1, -1);
        }

        private static void CreateButton(Transform parent, string name, string label, Vector2 pos)
        {
             if (parent == null) return;

             var tf = parent.Find(name);
             if (tf == null)
             {
                 var go = new GameObject(name);
                 tf = go.transform;
                 tf.SetParent(parent, false);
                 
                 var img = go.AddComponent<Image>();
                 img.color = new Color(1f, 0.9f, 0.8f); // Light button
                 
                 var btn = go.AddComponent<Button>();
             }
             
             var rect = tf.GetComponent<RectTransform>();
             if (rect == null) rect = tf.gameObject.AddComponent<RectTransform>();
             rect.anchoredPosition = pos;
             rect.sizeDelta = new Vector2(320, 100); // 4K scaled default

             // Label - 4K scaled font
             CreateText(tf, "Label", label, 40, Vector2.zero);
             var txt = tf.Find("Label").GetComponent<TextMeshProUGUI>();
             if (txt != null) txt.color = Color.black; // Dark text on light button
        }
#endif
    }
}
