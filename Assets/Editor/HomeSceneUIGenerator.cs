using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using Game.Scenes;

namespace Game.Editor
{
    /// <summary>
    /// HomeシーンのUI要素を自動生成するエディタツール
    /// </summary>
    public class HomeSceneUIGenerator : UnityEditor.Editor
    {
        [MenuItem("Tools/Generate Home Scene UI")]
        public static void GenerateHomeUI()
        {
            // Canvasを探す、なければ作成
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                var canvasGO = new GameObject("Canvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasGO.AddComponent<GraphicRaycaster>();
            }

            // 背景
            var background = CreatePanel(canvas.transform, "Background", new Color(0.1f, 0.1f, 0.15f, 1f));
            SetStretch(background.GetComponent<RectTransform>());

            // Main Menu Panel (Center)
            var mainMenuPanel = CreatePanel(canvas.transform, "MainMenuPanel", new Color(0, 0, 0, 0));
            var mainMenuRect = mainMenuPanel.GetComponent<RectTransform>();
            mainMenuRect.anchorMin = new Vector2(0.5f, 0.5f);
            mainMenuRect.anchorMax = new Vector2(0.5f, 0.5f);
            mainMenuRect.sizeDelta = new Vector2(800, 600);
            mainMenuRect.anchoredPosition = Vector2.zero;

            // Buttons Layout
            // Matching Button
            var matchingButton = CreateButton(mainMenuPanel.transform, "MatchingButton", "マッチング対戦", new Color(0.8f, 0.2f, 0.2f, 1f));
            var matchingRect = matchingButton.GetComponent<RectTransform>();
            matchingRect.anchorMin = new Vector2(0.5f, 0.5f);
            matchingRect.anchorMax = new Vector2(0.5f, 0.5f);
            matchingRect.sizeDelta = new Vector2(300, 80);
            matchingRect.anchoredPosition = new Vector2(0, 100);

            // Bot Battle Button
            var botButton = CreateButton(mainMenuPanel.transform, "BotBattleButton", "Bot対戦", new Color(0.2f, 0.6f, 0.8f, 1f));
            var botRect = botButton.GetComponent<RectTransform>();
            botRect.anchorMin = new Vector2(0.5f, 0.5f);
            botRect.anchorMax = new Vector2(0.5f, 0.5f);
            botRect.sizeDelta = new Vector2(300, 60);
            botRect.anchoredPosition = new Vector2(0, 10);

            // Deck Edit Button
            var deckButton = CreateButton(mainMenuPanel.transform, "DeckEditButton", "デッキ編成", new Color(0.2f, 0.8f, 0.4f, 1f));
            var deckRect = deckButton.GetComponent<RectTransform>();
            deckRect.anchorMin = new Vector2(0.5f, 0.5f);
            deckRect.anchorMax = new Vector2(0.5f, 0.5f);
            deckRect.sizeDelta = new Vector2(300, 60);
            deckRect.anchoredPosition = new Vector2(0, -70);

            // Gacha Button
            var gachaButton = CreateButton(mainMenuPanel.transform, "GachaButton", "ガチャ", new Color(0.6f, 0.2f, 0.8f, 1f));
            var gachaRect = gachaButton.GetComponent<RectTransform>();
            gachaRect.anchorMin = new Vector2(0.5f, 0.5f);
            gachaRect.anchorMax = new Vector2(0.5f, 0.5f);
            gachaRect.sizeDelta = new Vector2(300, 60);
            gachaRect.anchoredPosition = new Vector2(0, -150);


            // Header Panel (Top Right)
            var headerPanel = CreatePanel(canvas.transform, "HeaderPanel", new Color(0, 0, 0, 0));
            var headerRect = headerPanel.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(1f, 1f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.sizeDelta = new Vector2(200, 100);
            headerRect.anchoredPosition = new Vector2(-20, -20);
            headerRect.pivot = new Vector2(1, 1);

            // Settings Button (Icon-like)
            var settingsButton = CreateButton(headerPanel.transform, "SettingsButton", "設定", new Color(0.4f, 0.4f, 0.45f, 1f));
            var settingsRect = settingsButton.GetComponent<RectTransform>();
            settingsRect.anchorMin = new Vector2(1f, 1f);
            settingsRect.anchorMax = new Vector2(1f, 1f);
            settingsRect.sizeDelta = new Vector2(80, 80);
            settingsRect.anchoredPosition = Vector2.zero;


            // Settings Panel (Overlay) - Initially Inactive
            var settingsPanel = CreatePanel(canvas.transform, "SettingsPanel", new Color(0, 0, 0, 0.8f));
            SetStretch(settingsPanel.GetComponent<RectTransform>());
            settingsPanel.SetActive(false);

            var settingsContent = CreatePanel(settingsPanel.transform, "Content", new Color(0.2f, 0.2f, 0.25f, 1f));
            var contentRect = settingsContent.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.5f, 0.5f);
            contentRect.anchorMax = new Vector2(0.5f, 0.5f);
            contentRect.sizeDelta = new Vector2(400, 300);
            contentRect.anchoredPosition = Vector2.zero;

            // Settings Title
            var settingsTitle = CreateText(settingsContent.transform, "Title", "設定", 24, FontStyles.Bold);
            var titleRect = settingsTitle.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.sizeDelta = new Vector2(300, 50);
            titleRect.anchoredPosition = new Vector2(0, -30);

            // Username Text
            var usernameText = CreateText(settingsContent.transform, "UsernameText", "ユーザー名: Loading...", 18, FontStyles.Normal);
            var userRect = usernameText.GetComponent<RectTransform>();
            userRect.anchorMin = new Vector2(0.5f, 1f);
            userRect.anchorMax = new Vector2(0.5f, 1f);
            userRect.sizeDelta = new Vector2(350, 40);
            userRect.anchoredPosition = new Vector2(0, -100);

            // Logout Button
            var logoutButton = CreateButton(settingsContent.transform, "LogoutButton", "ログアウト", new Color(0.8f, 0.3f, 0.3f, 1f));
            var logoutRect = logoutButton.GetComponent<RectTransform>();
            logoutRect.anchorMin = new Vector2(0.5f, 0f);
            logoutRect.anchorMax = new Vector2(0.5f, 0f);
            logoutRect.sizeDelta = new Vector2(200, 50);
            logoutRect.anchoredPosition = new Vector2(0, 80);

            // Close Settings Button
            var closeSettingsButton = CreateButton(settingsContent.transform, "CloseButton", "閉じる", new Color(0.5f, 0.5f, 0.5f, 1f));
            var closeRect = closeSettingsButton.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(0.5f, 0f);
            closeRect.anchorMax = new Vector2(0.5f, 0f);
            closeRect.sizeDelta = new Vector2(200, 50);
            closeRect.anchoredPosition = new Vector2(0, 20);


            // Assign to HomeScene
            var homeScene = FindObjectOfType<HomeScene>();
            if (homeScene == null)
            {
                var sceneGO = new GameObject("HomeScene");
                homeScene = sceneGO.AddComponent<HomeScene>();
            }

            var serializedObject = new SerializedObject(homeScene);
            
            // Navigation
            serializedObject.FindProperty("battleButton").objectReferenceValue = matchingButton.GetComponent<Button>();
            serializedObject.FindProperty("botBattleButton").objectReferenceValue = botButton.GetComponent<Button>();
            serializedObject.FindProperty("deckEditButton").objectReferenceValue = deckButton.GetComponent<Button>();
            serializedObject.FindProperty("gachaButton").objectReferenceValue = gachaButton.GetComponent<Button>();

            // Header
            serializedObject.FindProperty("settingsButton").objectReferenceValue = settingsButton.GetComponent<Button>();

            // Settings
            serializedObject.FindProperty("settingsPanel").objectReferenceValue = settingsPanel;
            serializedObject.FindProperty("closeSettingsButton").objectReferenceValue = closeSettingsButton.GetComponent<Button>();
            serializedObject.FindProperty("usernameText").objectReferenceValue = usernameText.GetComponent<TextMeshProUGUI>();
            serializedObject.FindProperty("logoutButton").objectReferenceValue = logoutButton.GetComponent<Button>();

            serializedObject.ApplyModifiedProperties();

            Debug.Log("Home Scene UI generated successfully!");
            EditorUtility.DisplayDialog("完了", "HomeシーンのUIを生成しました", "OK");
        }

        #region Helper Methods (Same as LoginSceneUIGenerator logic but can be refactored to common utils later)

        private static GameObject CreatePanel(Transform parent, string name, Color color)
        {
            // Check if exists
            var existing = parent.Find(name);
            if (existing != null) return existing.gameObject;

            var panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            var image = panel.AddComponent<Image>();
            image.color = color;
            return panel;
        }

        private static GameObject CreateText(Transform parent, string name, string text, int fontSize, FontStyles style)
        {
             // Check if exists
            var existing = parent.Find(name);
            if (existing != null) return existing.gameObject;

            var textGO = new GameObject(name);
            textGO.transform.SetParent(parent, false);
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            return textGO;
        }

        private static GameObject CreateButton(Transform parent, string name, string text, Color color)
        {
             // Check if exists
            var existing = parent.Find(name);
            if (existing != null) return existing.gameObject;

            var buttonGO = new GameObject(name);
            buttonGO.transform.SetParent(parent, false);

            var image = buttonGO.AddComponent<Image>();
            image.color = color;

            var button = buttonGO.AddComponent<Button>();
            button.targetGraphic = image;

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform, false);
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 20;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            var textRect = textGO.GetComponent<RectTransform>();
            SetStretch(textRect);

            return buttonGO;
        }

        private static void SetStretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
        #endregion
    }
}
