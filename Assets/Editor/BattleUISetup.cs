using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class BattleUISetup
{
    [MenuItem("Tools/Setup Battle UI")]
    public static void Setup()
    {
        Debug.Log("Starting UI Setup...");
        
        // 1. Setup Sprite
        string path = "Assets/Textures/UIFrame.png";
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 100;
            importer.spriteBorder = new Vector4(16, 16, 16, 16); // 9-slice assumption
            importer.filterMode = FilterMode.Point; // Pixel art
            importer.SaveAndReimport();
        }
        Sprite frameSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

        // 2. Find Canvas
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found!");
            return;
        }
        
        // Ensure CanvasScaler is set up for pixel art or consistent UI
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // 3. Setup Player 1 Panel (Bottom Left)
        // Anchor 0,0 is Bottom Left
        SetupPlayerPanel(canvas.transform, "Player1", frameSprite, new Vector2(0, 0), new Vector2(50, 50));

        // 4. Setup Player 2 Panel (Top Right)
        // Anchor 1,1 is Top Right
        SetupPlayerPanel(canvas.transform, "Player2", frameSprite, new Vector2(1, 1), new Vector2(-50, -50));

        // 5. Style Turn Text
        TextMeshProUGUI turnText = FindText("TurnText");
        if (turnText != null)
        {
            turnText.fontSize = 64;
            turnText.color = Color.yellow;
            turnText.enableVertexGradient = true;
            // turnText.fontSharedMaterial.EnableKeyword("OUTLINE_ON"); // Often causes issues if material is shared
            turnText.outlineWidth = 0.2f;
            turnText.outlineColor = Color.black;
            
            RectTransform rt = turnText.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f); // Top
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, -100); // Below top edge
        }

        Debug.Log("UI Setup Complete.");
    }

    private static void SetupPlayerPanel(Transform canvas, string prefix, Sprite bgSprite, Vector2 anchor, Vector2 anchoredPos)
    {
        // Create or Find Panel
        string panelName = prefix + "Panel";
        Transform existingPanel = canvas.Find(panelName);
        GameObject panelObj;
        
        if (existingPanel == null)
        {
            panelObj = new GameObject(panelName);
            panelObj.transform.SetParent(canvas, false);
            Image img = panelObj.AddComponent<Image>();
            img.sprite = bgSprite;
            img.type = Image.Type.Sliced;
            img.color = new Color(1, 1, 1, 0.9f);
        }
        else
        {
            panelObj = existingPanel.gameObject;
            Image img = panelObj.GetComponent<Image>();
            if (img == null) img = panelObj.AddComponent<Image>();
            img.sprite = bgSprite;
        }

        RectTransform rt = panelObj.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor; // Pivot at corner
        rt.anchoredPosition = anchoredPos; // Offset from corner
        rt.sizeDelta = new Vector2(400, 150);

        // Move Texts into Panel
        // Note: Text names are usually "Player1Name", "Player1HP"
        MoveTextToPanel(prefix + "Name", panelObj.transform, new Vector2(20, 30), 36);
        MoveTextToPanel(prefix + "HP", panelObj.transform, new Vector2(20, -30), 48);
    }

    private static void MoveTextToPanel(string textName, Transform panel, Vector2 offset, float fontSize)
    {
        TextMeshProUGUI text = FindText(textName);
        if (text != null)
        {
            text.transform.SetParent(panel, false);
            RectTransform rt = text.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(0, 0.5f);
            rt.pivot = new Vector2(0, 0.5f);
            rt.anchoredPosition = offset;
            rt.sizeDelta = new Vector2(350, 50);
            
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Left;
            text.color = Color.white;
        }
    }

    private static TextMeshProUGUI FindText(string name)
    {
        // Find anywhere in canvas
        Canvas c = Object.FindAnyObjectByType<Canvas>();
        if (c == null) return null;
        
        foreach (var t in c.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (t.name == name) return t;
        }
        return null;
    }
}