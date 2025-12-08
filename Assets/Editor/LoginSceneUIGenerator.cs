using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using Game.Scenes;

namespace Game.Editor
{
    /// <summary>
    /// LoginシーンのUI要素を自動生成するエディタツール
    /// </summary>
    public class LoginSceneUIGenerator : UnityEditor.Editor
    {
        [MenuItem("Tools/Generate Login Scene UI")]
        public static void GenerateLoginUI()
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

            // 背景パネル
            var background = CreatePanel(canvas.transform, "Background", new Color(0.1f, 0.1f, 0.15f, 1f));
            var backgroundRect = background.GetComponent<RectTransform>();
            SetStretch(backgroundRect);

            // ログインパネル
            var loginPanel = CreatePanel(canvas.transform, "LoginPanel", new Color(0.15f, 0.15f, 0.2f, 0.95f));
            var loginPanelRect = loginPanel.GetComponent<RectTransform>();
            loginPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
            loginPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
            loginPanelRect.sizeDelta = new Vector2(400, 500);
            loginPanelRect.anchoredPosition = Vector2.zero;

            // タイトル
            var titleText = CreateText(loginPanel.transform, "TitleText", "ログイン", 32, FontStyles.Bold);
            var titleRect = titleText.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.sizeDelta = new Vector2(350, 50);
            titleRect.anchoredPosition = new Vector2(0, -40);

            // メールアドレス入力（サインアップ用 - 今はログインにも使用）
            var emailInput = CreateInputField(loginPanel.transform, "EmailInput", "メールアドレス");
            var emailRect = emailInput.GetComponent<RectTransform>();
            emailRect.anchorMin = new Vector2(0.5f, 1f);
            emailRect.anchorMax = new Vector2(0.5f, 1f);
            emailRect.sizeDelta = new Vector2(350, 50);
            emailRect.anchoredPosition = new Vector2(0, -130);

            // パスワード入力
            var passwordInput = CreateInputField(loginPanel.transform, "PasswordInput", "パスワード", TMP_InputField.ContentType.Password);
            var passwordRect = passwordInput.GetComponent<RectTransform>();
            passwordRect.anchorMin = new Vector2(0.5f, 1f);
            passwordRect.anchorMax = new Vector2(0.5f, 1f);
            passwordRect.sizeDelta = new Vector2(350, 50);
            passwordRect.anchoredPosition = new Vector2(0, -200);

            // パスワード確認入力（サインアップ用）
            var confirmPasswordInput = CreateInputField(loginPanel.transform, "ConfirmPasswordInput", "パスワード確認", TMP_InputField.ContentType.Password);
            var confirmPasswordRect = confirmPasswordInput.GetComponent<RectTransform>();
            confirmPasswordRect.anchorMin = new Vector2(0.5f, 1f);
            confirmPasswordRect.anchorMax = new Vector2(0.5f, 1f);
            confirmPasswordRect.sizeDelta = new Vector2(350, 50);
            confirmPasswordRect.anchoredPosition = new Vector2(0, -270);

            // ログインボタン
            var loginButton = CreateButton(loginPanel.transform, "LoginButton", "ログイン", new Color(0.2f, 0.6f, 0.9f, 1f));
            var loginButtonRect = loginButton.GetComponent<RectTransform>();
            loginButtonRect.anchorMin = new Vector2(0.5f, 1f);
            loginButtonRect.anchorMax = new Vector2(0.5f, 1f);
            loginButtonRect.sizeDelta = new Vector2(350, 50);
            loginButtonRect.anchoredPosition = new Vector2(0, -340);

            // サインアップボタン
            var signUpButton = CreateButton(loginPanel.transform, "SignUpButton", "アカウント作成", new Color(0.2f, 0.7f, 0.4f, 1f));
            var signUpButtonRect = signUpButton.GetComponent<RectTransform>();
            signUpButtonRect.anchorMin = new Vector2(0.5f, 1f);
            signUpButtonRect.anchorMax = new Vector2(0.5f, 1f);
            signUpButtonRect.sizeDelta = new Vector2(350, 50);
            signUpButtonRect.anchoredPosition = new Vector2(0, -340);
            signUpButton.SetActive(false); // 初期は非表示

            // エラーテキスト
            var errorText = CreateText(loginPanel.transform, "ErrorText", "", 14, FontStyles.Normal);
            var errorTextTMP = errorText.GetComponent<TextMeshProUGUI>();
            errorTextTMP.color = Color.red;
            errorTextTMP.alignment = TextAlignmentOptions.Center;
            var errorRect = errorText.GetComponent<RectTransform>();
            errorRect.anchorMin = new Vector2(0.5f, 1f);
            errorRect.anchorMax = new Vector2(0.5f, 1f);
            errorRect.sizeDelta = new Vector2(350, 40);
            errorRect.anchoredPosition = new Vector2(0, -400);

            // モード切り替えボタン
            var switchModeButton = CreateButton(loginPanel.transform, "SwitchModeButton", "", new Color(0.3f, 0.3f, 0.35f, 0.5f));
            var switchModeRect = switchModeButton.GetComponent<RectTransform>();
            switchModeRect.anchorMin = new Vector2(0.5f, 0f);
            switchModeRect.anchorMax = new Vector2(0.5f, 0f);
            switchModeRect.sizeDelta = new Vector2(200, 40);
            switchModeRect.anchoredPosition = new Vector2(0, 30);

            // モード切り替えテキスト
            var switchModeText = switchModeButton.GetComponentInChildren<TextMeshProUGUI>();
            if (switchModeText != null)
            {
                switchModeText.text = "新規登録はこちら";
                switchModeText.fontSize = 14;
            }

            // ローディングパネル
            var loadingPanel = CreatePanel(canvas.transform, "LoadingPanel", new Color(0, 0, 0, 0.7f));
            SetStretch(loadingPanel.GetComponent<RectTransform>());
            var loadingText = CreateText(loadingPanel.transform, "LoadingText", "読み込み中...", 24, FontStyles.Normal);
            loadingPanel.SetActive(false);

            // Verification Panel (initially hidden)
            var verificationPanel = CreatePanel(canvas.transform, "VerificationPanel", new Color(0, 0, 0, 0.85f));
            var verificationPanelRect = verificationPanel.GetComponent<RectTransform>();
            SetStretch(verificationPanelRect);
            verificationPanel.SetActive(false);

            var verificationContainer = CreatePanel(verificationPanel.transform, "Container", new Color(0.15f, 0.15f, 0.2f, 0.95f));
            var verificationContainerRect = verificationContainer.GetComponent<RectTransform>();
            verificationContainerRect.anchorMin = new Vector2(0.5f, 0.5f);
            verificationContainerRect.anchorMax = new Vector2(0.5f, 0.5f);
            verificationContainerRect.sizeDelta = new Vector2(400, 350);
            verificationContainerRect.anchoredPosition = Vector2.zero;

            // Verification Message
            var verificationMessage = CreateText(verificationContainer.transform, "MessageText", "確認コードを入力してください", 18, FontStyles.Normal);
            var verificationMessageRect = verificationMessage.GetComponent<RectTransform>();
            verificationMessageRect.anchorMin = new Vector2(0.5f, 1f);
            verificationMessageRect.anchorMax = new Vector2(0.5f, 1f);
            verificationMessageRect.sizeDelta = new Vector2(350, 60);
            verificationMessageRect.anchoredPosition = new Vector2(0, -50);

            // Verification Code Input
            var verificationCodeInput = CreateInputField(verificationContainer.transform, "CodeInput", "確認コード");
            var verificationCodeRect = verificationCodeInput.GetComponent<RectTransform>();
            verificationCodeRect.anchorMin = new Vector2(0.5f, 1f);
            verificationCodeRect.anchorMax = new Vector2(0.5f, 1f);
            verificationCodeRect.sizeDelta = new Vector2(350, 50);
            verificationCodeRect.anchoredPosition = new Vector2(0, -130);

            // Verify Button
            var verifyButton = CreateButton(verificationContainer.transform, "VerifyButton", "認証する", new Color(0.2f, 0.6f, 0.9f, 1f));
            var verifyButtonRect = verifyButton.GetComponent<RectTransform>();
            verifyButtonRect.anchorMin = new Vector2(0.5f, 1f);
            verifyButtonRect.anchorMax = new Vector2(0.5f, 1f);
            verifyButtonRect.sizeDelta = new Vector2(350, 50);
            verifyButtonRect.anchoredPosition = new Vector2(0, -210);

            // Back to SignUp Button
            var backToSignUpButton = CreateButton(verificationContainer.transform, "BackToSignUpButton", "戻る", new Color(0.5f, 0.5f, 0.5f, 1f));
            var backToSignUpButtonRect = backToSignUpButton.GetComponent<RectTransform>();
            backToSignUpButtonRect.anchorMin = new Vector2(0.5f, 1f);
            backToSignUpButtonRect.anchorMax = new Vector2(0.5f, 1f);
            backToSignUpButtonRect.sizeDelta = new Vector2(350, 50);
            backToSignUpButtonRect.anchoredPosition = new Vector2(0, -280);


            // LoginSceneコンポーネントを探して設定
            var loginScene = FindObjectOfType<LoginScene>();
            if (loginScene == null)
            {
                var loginSceneGO = new GameObject("LoginScene");
                loginScene = loginSceneGO.AddComponent<LoginScene>();
            }

            // SerializedObjectで参照を設定
            var serializedObject = new SerializedObject(loginScene);
            serializedObject.FindProperty("emailInput").objectReferenceValue = emailInput.GetComponent<TMP_InputField>();
            serializedObject.FindProperty("passwordInput").objectReferenceValue = passwordInput.GetComponent<TMP_InputField>();
            serializedObject.FindProperty("confirmPasswordInput").objectReferenceValue = confirmPasswordInput.GetComponent<TMP_InputField>();
            serializedObject.FindProperty("loginButton").objectReferenceValue = loginButton.GetComponent<Button>();
            serializedObject.FindProperty("signUpButton").objectReferenceValue = signUpButton.GetComponent<Button>();
            serializedObject.FindProperty("switchModeButton").objectReferenceValue = switchModeButton.GetComponent<Button>();
            serializedObject.FindProperty("switchModeText").objectReferenceValue = switchModeText;
            serializedObject.FindProperty("errorText").objectReferenceValue = errorText.GetComponent<TextMeshProUGUI>();
            serializedObject.FindProperty("titleText").objectReferenceValue = titleText.GetComponent<TextMeshProUGUI>();
            serializedObject.FindProperty("loadingPanel").objectReferenceValue = loadingPanel;
            
            // Verification UI
            serializedObject.FindProperty("verificationPanel").objectReferenceValue = verificationPanel;
            serializedObject.FindProperty("verificationCodeInput").objectReferenceValue = verificationCodeInput.GetComponent<TMP_InputField>();
            serializedObject.FindProperty("verifyButton").objectReferenceValue = verifyButton.GetComponent<Button>();
            serializedObject.FindProperty("backToSignUpButton").objectReferenceValue = backToSignUpButton.GetComponent<Button>();
            serializedObject.FindProperty("verificationMessageText").objectReferenceValue = verificationMessage.GetComponent<TextMeshProUGUI>();

            serializedObject.ApplyModifiedProperties();

            Debug.Log("Login Scene UI generated successfully!");
            EditorUtility.DisplayDialog("完了", "LoginシーンのUIを生成しました", "OK");
        }

        [MenuItem("Tools/Generate Verification UI Only")]
        public static void GenerateVerificationUIOnly()
        {
            var loginScene = FindObjectOfType<LoginScene>();
            if (loginScene == null)
            {
                EditorUtility.DisplayDialog("エラー", "LoginSceneが見つかりません。シーンを開いているか確認してください。", "OK");
                return;
            }

            var serializedObject = new SerializedObject(loginScene);
            var verificationPanelProperty = serializedObject.FindProperty("verificationPanel");

            if (verificationPanelProperty.objectReferenceValue != null)
            {
                EditorUtility.DisplayDialog("情報", "VerificationPanelは既に設定されています。", "OK");
                return;
            }

            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("エラー", "Canvasが見つかりません。", "OK");
                return;
            }

            // Verification Panel (initially hidden)
            var verificationPanel = CreatePanel(canvas.transform, "VerificationPanel", new Color(0, 0, 0, 0.85f));
            var verificationPanelRect = verificationPanel.GetComponent<RectTransform>();
            SetStretch(verificationPanelRect);
            verificationPanel.SetActive(false);

            var verificationContainer = CreatePanel(verificationPanel.transform, "Container", new Color(0.15f, 0.15f, 0.2f, 0.95f));
            var verificationContainerRect = verificationContainer.GetComponent<RectTransform>();
            verificationContainerRect.anchorMin = new Vector2(0.5f, 0.5f);
            verificationContainerRect.anchorMax = new Vector2(0.5f, 0.5f);
            verificationContainerRect.sizeDelta = new Vector2(400, 350);
            verificationContainerRect.anchoredPosition = Vector2.zero;

            // Verification Message
            var verificationMessage = CreateText(verificationContainer.transform, "MessageText", "確認コードを入力してください", 18, FontStyles.Normal);
            var verificationMessageRect = verificationMessage.GetComponent<RectTransform>();
            verificationMessageRect.anchorMin = new Vector2(0.5f, 1f);
            verificationMessageRect.anchorMax = new Vector2(0.5f, 1f);
            verificationMessageRect.sizeDelta = new Vector2(350, 60);
            verificationMessageRect.anchoredPosition = new Vector2(0, -50);

            // Verification Code Input
            var verificationCodeInput = CreateInputField(verificationContainer.transform, "CodeInput", "確認コード");
            var verificationCodeRect = verificationCodeInput.GetComponent<RectTransform>();
            verificationCodeRect.anchorMin = new Vector2(0.5f, 1f);
            verificationCodeRect.anchorMax = new Vector2(0.5f, 1f);
            verificationCodeRect.sizeDelta = new Vector2(350, 50);
            verificationCodeRect.anchoredPosition = new Vector2(0, -130);

            // Verify Button
            var verifyButton = CreateButton(verificationContainer.transform, "VerifyButton", "認証する", new Color(0.2f, 0.6f, 0.9f, 1f));
            var verifyButtonRect = verifyButton.GetComponent<RectTransform>();
            verifyButtonRect.anchorMin = new Vector2(0.5f, 1f);
            verifyButtonRect.anchorMax = new Vector2(0.5f, 1f);
            verifyButtonRect.sizeDelta = new Vector2(350, 50);
            verifyButtonRect.anchoredPosition = new Vector2(0, -210);

            // Back to SignUp Button
            var backToSignUpButton = CreateButton(verificationContainer.transform, "BackToSignUpButton", "戻る", new Color(0.5f, 0.5f, 0.5f, 1f));
            var backToSignUpButtonRect = backToSignUpButton.GetComponent<RectTransform>();
            backToSignUpButtonRect.anchorMin = new Vector2(0.5f, 1f);
            backToSignUpButtonRect.anchorMax = new Vector2(0.5f, 1f);
            backToSignUpButtonRect.sizeDelta = new Vector2(350, 50);
            backToSignUpButtonRect.anchoredPosition = new Vector2(0, -280);

            // Verification UI
            verificationPanelProperty.objectReferenceValue = verificationPanel;
            serializedObject.FindProperty("verificationCodeInput").objectReferenceValue = verificationCodeInput.GetComponent<TMP_InputField>();
            serializedObject.FindProperty("verifyButton").objectReferenceValue = verifyButton.GetComponent<Button>();
            serializedObject.FindProperty("backToSignUpButton").objectReferenceValue = backToSignUpButton.GetComponent<Button>();
            serializedObject.FindProperty("verificationMessageText").objectReferenceValue = verificationMessage.GetComponent<TextMeshProUGUI>();

            serializedObject.ApplyModifiedProperties();

            Debug.Log("Verification UI generated successfully!");
            EditorUtility.DisplayDialog("完了", "Verification UIのみを追加生成しました", "OK");
        }

        private static GameObject CreatePanel(Transform parent, string name, Color color)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            var image = panel.AddComponent<Image>();
            image.color = color;
            return panel;
        }

        private static GameObject CreateText(Transform parent, string name, string text, int fontSize, FontStyles style)
        {
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

        private static GameObject CreateInputField(Transform parent, string name, string placeholder, TMP_InputField.ContentType contentType = TMP_InputField.ContentType.Standard)
        {
            var inputGO = new GameObject(name);
            inputGO.transform.SetParent(parent, false);

            var image = inputGO.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.25f, 1f);

            var inputField = inputGO.AddComponent<TMP_InputField>();
            inputField.contentType = contentType;

            // Text Area
            var textArea = new GameObject("Text Area");
            textArea.transform.SetParent(inputGO.transform, false);
            var textAreaRect = textArea.AddComponent<RectTransform>();
            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.offsetMin = new Vector2(10, 5);
            textAreaRect.offsetMax = new Vector2(-10, -5);

            // Placeholder
            var placeholderGO = new GameObject("Placeholder");
            placeholderGO.transform.SetParent(textArea.transform, false);
            var placeholderText = placeholderGO.AddComponent<TextMeshProUGUI>();
            placeholderText.text = placeholder;
            placeholderText.fontSize = 18;
            placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            placeholderText.alignment = TextAlignmentOptions.Left;
            var placeholderRect = placeholderGO.GetComponent<RectTransform>();
            SetStretch(placeholderRect);

            // Text
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(textArea.transform, false);
            var inputText = textGO.AddComponent<TextMeshProUGUI>();
            inputText.fontSize = 18;
            inputText.color = Color.white;
            inputText.alignment = TextAlignmentOptions.Left;
            var textRect = textGO.GetComponent<RectTransform>();
            SetStretch(textRect);

            inputField.textViewport = textAreaRect;
            inputField.textComponent = inputText;
            inputField.placeholder = placeholderText;

            return inputGO;
        }

        private static GameObject CreateButton(Transform parent, string name, string text, Color color)
        {
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
    }
}
