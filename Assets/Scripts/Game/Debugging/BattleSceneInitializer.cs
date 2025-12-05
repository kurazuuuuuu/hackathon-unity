using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
                    scaler.referenceResolution = new Vector2(1920, 1080);
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
            
            Debug.Log("Battleシーン初期化完了！");
        }

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

            // すべてのTextMeshProUGUIに適用（非アクティブ含む）
            var allTexts = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
            foreach (var tmp in allTexts)
            {
                // シーン上のオブジェクトのみ（プレハブは除外）
                if (tmp.gameObject.scene.name != null && !string.IsNullOrEmpty(tmp.gameObject.scene.name))
                {
                    tmp.font = fontAsset;
                    EditorUtility.SetDirty(tmp);
                }
            }

            Debug.Log($"日本語フォント適用完了: {fontAsset.name}");
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
