using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Battle;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Debugging
{
    /// <summary>
    /// Battleシーンを初期化するエディターツール
    /// </summary>
    public class BattleSceneInitializer : MonoBehaviour
    {
#if UNITY_EDITOR
        [MenuItem("Tools/Battle/Initialize Battle Scene")]
        public static void InitializeBattleScene()
        {
            // Canvas設定
            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas != null)
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                
                var scaler = canvas.GetComponent<CanvasScaler>();
                if (scaler != null)
                {
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(3840, 2160);
                    scaler.matchWidthOrHeight = 0.5f;
                }
                
                EditorUtility.SetDirty(canvas);
                Debug.Log("Canvas設定完了");
            }

            // 日本語フォントを適用
            ApplyJapaneseFont();

            // CardManagerのセットアップ
            SetupCardManager();

            // UI要素の初期化
            InitializeTextElements();
            InitializeButtons();
            
            // Layout (Primary Zones, Deck, Hand)
            SetupBattleLayout();

            Debug.Log("Battleシーン初期化完了！");
        }


        /// <summary>
        /// 日本語フォントをすべてのTextMeshProUGUIに適用
        /// </summary>
        /// <summary>
        /// 日本語フォントをすべてのTextMeshProUGUIに適用
        /// </summary>
        private static void ApplyJapaneseFont()
        {
            // DotGothic16フォントをロード
            var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/DotGothic16-Regular SDF.asset");
            if (fontAsset == null)
            {
                Debug.LogWarning("日本語フォント (DotGothic16-Regular SDF) が見つかりませんでした");
                return;
            }

            // フォントのテクスチャフィルタをPoint（ドット絵用）に設定
            if (fontAsset.material != null && fontAsset.material.mainTexture != null)
            {
                fontAsset.material.mainTexture.filterMode = FilterMode.Point;
            }

            // すべてのTextMeshProUGUIに適用（非アクティブ含む）
            var allTexts = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
            foreach (var tmp in allTexts)
            {
                // シーン上のオブジェクトのみ（プレハブは除外）
                if (tmp.gameObject.scene.name != null && !string.IsNullOrEmpty(tmp.gameObject.scene.name))
                {
                    tmp.font = fontAsset;
                    
                    // マテリアルをフォントアセットのデフォルトに強制リセット
                    // これにより、古いマテリアルプリセット（黄色い文字など）が原因の文字化けを防ぐ
                    tmp.fontSharedMaterial = fontAsset.material;

                    EditorUtility.SetDirty(tmp);
                }
            }
            
            Debug.Log($"日本語フォント適用完了: {fontAsset.name} (FilterMode: Point, Material Reset)");
        }

        private static void SetupBattleLayout()
        {
            var battleManager = FindAnyObjectByType<BattleManager>();
            if (battleManager == null) return;

            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // Player 1 Zone (Bottom/Left is standard, but sketch implies P1 is bottom?) 
            // Sketch shows: Primary (3 cards) -> Deck -> Hand
            // Let's assume Player 1 is "Self/Bottom" and Player 2 is "Opponent/Top" for now, or match existing coordinates.
            // Existing text coordinates: Player1 (-150, -50), Player2 (150, -50). This suggests side-by-side or top-down logic isn't fully clear.
            // Let's try to follow a standard card game layout: P1 Bottom, P2 Top.

            // Layout Container for P1
            var p1Zone = CreatePrimaryZone("Player1_PrimaryZone", new Vector2(0, -200), canvas.transform);
            var p1Deck = CreateDeckVisual("Player1_Deck", new Vector2(400, -300), canvas.transform);
            // Hand area is usually dynamic, handled by CardSpawnArea?

            // Layout Container for P2
            var p2Zone = CreatePrimaryZone("Player2_PrimaryZone", new Vector2(0, 200), canvas.transform);
            // Rotate P2 zone? Usually opponent cards face them.
            
            // Assign to Players
            if (battleManager.Player1 != null) battleManager.Player1.SetPrimaryZone(p1Zone);
            if (battleManager.Player2 != null) battleManager.Player2.SetPrimaryZone(p2Zone);

            Debug.Log("Battle Layout Constructed.");
        }

        private static PrimaryCardZone CreatePrimaryZone(string name, Vector2 position, Transform parent)
        {
            var go = GameObject.Find(name);
            if (go == null)
            {
                go = new GameObject(name);
                go.transform.SetParent(parent, false);
            }

            var rect = go.GetComponent<RectTransform>();
            if (rect == null) rect = go.AddComponent<RectTransform>();
            
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(600, 250); // Big enough for 3 cards

            var zone = go.GetComponent<PrimaryCardZone>();
            if (zone == null) zone = go.AddComponent<PrimaryCardZone>();

            // Clean up old slots if any (optional, but safer to re-create)
            // For now, simpler to check if children exist.
            if (go.transform.childCount == 0)
            {
                // Create 3 slots
                for (int i = 0; i < 3; i++)
                {
                    var slot = new GameObject($"Slot_{i}");
                    slot.transform.SetParent(go.transform, false);
                    var slotRect = slot.AddComponent<RectTransform>();
                    // Horizontal layout: -200, 0, 200
                    slotRect.anchoredPosition = new Vector2((i - 1) * 160, 0); 
                    // Card size is roughly 140x200?
                }
            }
            
            // Assign slots to zone (reflection or manual assignment needed if list is private/serialized)
            // PrimaryCardZone uses GetComponent children or serialized list.
            // Since it's a serialized list, we should try to set it via SerializedObject
            var so = new SerializedObject(zone);
            so.Update();
            var slotsProp = so.FindProperty("cardSlots");
            if (slotsProp != null)
            {
                slotsProp.ClearArray();
                for (int i = 0; i < go.transform.childCount; i++)
                {
                    slotsProp.InsertArrayElementAtIndex(i);
                    slotsProp.GetArrayElementAtIndex(i).objectReferenceValue = go.transform.GetChild(i);
                }
            }
            so.ApplyModifiedProperties();

            return zone;
        }

        private static GameObject CreateDeckVisual(string name, Vector2 position, Transform parent)
        {
             var go = GameObject.Find(name);
            if (go == null)
            {
                go = new GameObject(name);
                go.transform.SetParent(parent, false);
            }
            
            var rect = go.GetComponent<RectTransform>();
            if (rect == null) rect = go.AddComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(120, 160); // Card size

            var img = go.GetComponent<Image>();
            if (img == null) img = go.AddComponent<Image>();
            img.color = new Color(0.3f, 0.3f, 0.3f); // Placeholder gray

            // Optional: Label
            // ...

            return go;
        }



        /// <summary>
        /// CardManagerをセットアップ
        /// </summary>
        private static void SetupCardManager()
        {
            var cardManager = FindAnyObjectByType<CardManager>();
            if (cardManager == null)
            {
                var go = new GameObject("CardManager");
                cardManager = go.AddComponent<CardManager>();
                Debug.Log("CardManagerを作成しました");
            }

            // CardPrefabの設定
            var cardPrefab = AssetDatabase.LoadAssetAtPath<Card>("Assets/Prefabs/Game/Card.prefab");
            if (cardPrefab != null)
            {
                // PrivateフィールドにアクセスするためにSerializedObjectを使用
                var so = new SerializedObject(cardManager);
                so.Update();
                
                var prefabProp = so.FindProperty("cardPrefab");
                if (prefabProp != null) prefabProp.objectReferenceValue = cardPrefab;

                // SpawnParentの設定（Canvasの下に作成）
                var canvas = FindAnyObjectByType<Canvas>();
                if (canvas != null)
                {
                    var spawnParent = canvas.transform.Find("CardSpawnArea");
                    if (spawnParent == null)
                    {
                        var go = new GameObject("CardSpawnArea");
                        go.transform.SetParent(canvas.transform, false);
                        var rect = go.AddComponent<RectTransform>();
                        rect.anchorMin = Vector2.zero;
                        rect.anchorMax = Vector2.one;
                        rect.sizeDelta = Vector2.zero;
                        spawnParent = go.transform;
                    }
                    
                    var parentProp = so.FindProperty("cardSpawnParent");
                    if (parentProp != null) parentProp.objectReferenceValue = spawnParent;
                }

                so.ApplyModifiedProperties();
            }
            else
            {
                Debug.LogError("CardPrefabが見つかりません: Assets/Prefabs/Game/Card.prefab");
            }
        }

        private static void InitializeTextElements()
        {
            SetupText("Player1Name", "プレイヤー1", new Vector2(300, 50), new Vector2(0, 1), new Vector2(-150, -50), 32);
            SetupText("Player1HP", "HP: 20/20", new Vector2(200, 40), new Vector2(0, 1), new Vector2(-150, -100), 28);
            SetupText("Player2Name", "プレイヤー2", new Vector2(300, 50), new Vector2(1, 1), new Vector2(150, -50), 32);
            SetupText("Player2HP", "HP: 20/20", new Vector2(200, 40), new Vector2(1, 1), new Vector2(150, -100), 28);
            SetupText("TurnText", "ターン情報", new Vector2(400, 60), new Vector2(0.5f, 1), new Vector2(0, -30), 36);
            SetupText("MessageText", "メッセージ", new Vector2(600, 100), new Vector2(0.5f, 0.5f), new Vector2(0, 100), 28);
            SetupText("SkipButtonText", "スキップ", new Vector2(180, 50), new Vector2(0.5f, 0.5f), Vector2.zero, 24);
            
            // 非アクティブなResultPanel内の要素
            SetupTextInactive("ResultText", "結果", new Vector2(500, 80), 48);
            SetupTextInactive("ReturnButtonText", "ホームへ戻る", new Vector2(200, 50), 24);
        }

        private static void SetupText(string name, string text, Vector2 size, Vector2 anchor, Vector2 position, float fontSize)
        {
            var go = GameObject.Find(name);
            if (go == null) return;

            var tmp = go.GetComponent<TextMeshProUGUI>();
            var rect = go.GetComponent<RectTransform>();

            if (tmp != null)
            {
                tmp.text = text;
                tmp.fontSize = fontSize;
                tmp.alignment = TextAlignmentOptions.Center;
                EditorUtility.SetDirty(tmp);
            }

            if (rect != null)
            {
                rect.anchorMin = anchor;
                rect.anchorMax = anchor;
                rect.sizeDelta = size;
                rect.anchoredPosition = position;
                EditorUtility.SetDirty(rect);
            }
        }

        private static void SetupTextInactive(string name, string text, Vector2 size, float fontSize)
        {
            var allTexts = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
            foreach (var tmp in allTexts)
            {
                if (tmp.gameObject.name == name)
                {
                    tmp.text = text;
                    tmp.fontSize = fontSize;
                    tmp.alignment = TextAlignmentOptions.Center;
                    
                    var rect = tmp.GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        rect.sizeDelta = size;
                    }
                    
                    EditorUtility.SetDirty(tmp);
                    break;
                }
            }
        }

        private static void InitializeButtons()
        {
            // SkipButton
            SetupButton("SkipButton", new Vector2(200, 60), new Vector2(0.5f, 0), new Vector2(0, 80), new Color(0.2f, 0.6f, 1f));
            
            // ReturnButton (非アクティブ)
            var allButtons = Resources.FindObjectsOfTypeAll<Button>();
            foreach (var btn in allButtons)
            {
                if (btn.gameObject.name == "ReturnButton")
                {
                    var image = btn.GetComponent<Image>();
                    if (image != null)
                    {
                        image.color = new Color(0.2f, 0.8f, 0.4f);
                    }
                    
                    var rect = btn.GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        rect.sizeDelta = new Vector2(250, 60);
                        rect.anchoredPosition = new Vector2(0, -80);
                    }
                    
                    EditorUtility.SetDirty(btn);
                    break;
                }
            }
            
            // ResultPanel
            var allImages = Resources.FindObjectsOfTypeAll<Image>();
            foreach (var img in allImages)
            {
                if (img.gameObject.name == "ResultPanel")
                {
                    img.color = new Color(0, 0, 0, 0.8f);
                    
                    var rect = img.GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        rect.anchorMin = Vector2.zero;
                        rect.anchorMax = Vector2.one;
                        rect.sizeDelta = Vector2.zero;
                        rect.anchoredPosition = Vector2.zero;
                    }
                    
                    EditorUtility.SetDirty(img);
                    break;
                }
            }
        }

        private static void SetupButton(string name, Vector2 size, Vector2 anchor, Vector2 position, Color color)
        {
            var go = GameObject.Find(name);
            if (go == null) return;

            var image = go.GetComponent<Image>();
            var rect = go.GetComponent<RectTransform>();

            if (image != null)
            {
                image.color = color;
                EditorUtility.SetDirty(image);
            }

            if (rect != null)
            {
                rect.anchorMin = anchor;
                rect.anchorMax = anchor;
                rect.sizeDelta = size;
                rect.anchoredPosition = position;
                EditorUtility.SetDirty(rect);
            }
        }
#endif
    }
}
