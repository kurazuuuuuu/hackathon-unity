using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

namespace Game.Editor
{
    /// <summary>
    /// カードPrefab用のテンプレートオブジェクトを生成するエディタツール
    /// </summary>
    public class CardTemplateGenerator : MonoBehaviour
    {
        [MenuItem("Game/Create Card Templates/Primary Card Template")]
        public static void CreatePrimaryCardTemplate()
        {
            // Root object
            GameObject root = new GameObject("PrimaryCard_Template");
            RectTransform rootRt = root.AddComponent<RectTransform>();
            rootRt.sizeDelta = new Vector2(180, 250);
            
            // Background Image
            Image bgImage = root.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.3f, 1f);
            
            // Add PrimaryCard component
            root.AddComponent<PrimaryCard>();
            root.AddComponent<CardDragHandler>();
            
            // --- Card Image (background) ---
            GameObject cardImageObj = CreateChild(root, "CardImage");
            RectTransform cardImgRt = cardImageObj.GetComponent<RectTransform>();
            cardImgRt.anchorMin = Vector2.zero;
            cardImgRt.anchorMax = Vector2.one;
            cardImgRt.offsetMin = new Vector2(5, 5);
            cardImgRt.offsetMax = new Vector2(-5, -5);
            Image cardImg = cardImageObj.AddComponent<Image>();
            cardImg.color = new Color(0.3f, 0.3f, 0.4f, 1f);
            cardImg.preserveAspect = true;
            
            // --- Card Content Container ---
            GameObject content = CreateChild(root, "Content");
            VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.spacing = 5;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            
            RectTransform contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = Vector2.zero;
            contentRt.anchorMax = Vector2.one;
            contentRt.offsetMin = Vector2.zero;
            contentRt.offsetMax = Vector2.zero;
            
            // --- Name Text ---
            GameObject nameObj = CreateTextChild(content, "NameText", "Card Name", 18, FontStyles.Bold);
            LayoutElement nameLE = nameObj.AddComponent<LayoutElement>();
            nameLE.preferredHeight = 30;
            
            // --- Health Text (Primary only) ---
            GameObject healthObj = CreateTextChild(content, "HealthText", "HP: 100/100", 14, FontStyles.Normal);
            LayoutElement healthLE = healthObj.AddComponent<LayoutElement>();
            healthLE.preferredHeight = 20;
            TMP_Text healthText = healthObj.GetComponent<TMP_Text>();
            healthText.color = new Color(0.4f, 1f, 0.4f);
            
            // --- Power Text ---
            GameObject powerObj = CreateTextChild(content, "PowerText", "ATK: 50", 14, FontStyles.Normal);
            LayoutElement powerLE = powerObj.AddComponent<LayoutElement>();
            powerLE.preferredHeight = 20;
            TMP_Text powerText = powerObj.GetComponent<TMP_Text>();
            powerText.color = new Color(1f, 0.5f, 0.5f);
            
            // --- Cost Text ---
            GameObject costObj = CreateTextChild(content, "CostText", "COST: 10", 12, FontStyles.Normal);
            LayoutElement costLE = costObj.AddComponent<LayoutElement>();
            costLE.preferredHeight = 18;
            
            // --- Ability Text ---
            GameObject abilityObj = CreateTextChild(content, "AbilityText", "Ability description...", 11, FontStyles.Italic);
            LayoutElement abilityLE = abilityObj.AddComponent<LayoutElement>();
            abilityLE.preferredHeight = 40;
            abilityLE.flexibleHeight = 1;
            TMP_Text abilityText = abilityObj.GetComponent<TMP_Text>();
            abilityText.color = new Color(0.8f, 0.8f, 0.6f);
            abilityText.enableWordWrapping = true;
            abilityText.overflowMode = TextOverflowModes.Ellipsis;
            
            // --- Charge Text (Corner) ---
            GameObject chargeObj = CreateTextChild(root, "ChargeText", "0", 12, FontStyles.Bold);
            RectTransform chargeRt = chargeObj.GetComponent<RectTransform>();
            chargeRt.anchorMin = new Vector2(1, 1);
            chargeRt.anchorMax = new Vector2(1, 1);
            chargeRt.pivot = new Vector2(1, 1);
            chargeRt.anchoredPosition = new Vector2(-5, -5);
            chargeRt.sizeDelta = new Vector2(30, 20);
            TMP_Text chargeText = chargeObj.GetComponent<TMP_Text>();
            chargeText.alignment = TextAlignmentOptions.Right;
            
            // --- Rarity Indicator (Bottom) ---
            GameObject rarityBar = CreateChild(root, "RarityBar");
            RectTransform rarityRt = rarityBar.GetComponent<RectTransform>();
            rarityRt.anchorMin = new Vector2(0, 0);
            rarityRt.anchorMax = new Vector2(1, 0);
            rarityRt.pivot = new Vector2(0.5f, 0);
            rarityRt.anchoredPosition = Vector2.zero;
            rarityRt.sizeDelta = new Vector2(0, 5);
            Image rarityImg = rarityBar.AddComponent<Image>();
            rarityImg.color = new Color(1f, 0.85f, 0f); // Gold for 5-star
            
            // Link SerializedFields
            LinkPrimaryCardReferences(root);
            
            // Select in hierarchy
            Selection.activeGameObject = root;
            
            Debug.Log("PrimaryCard Template created! Assign SerializedField references and save as Prefab.");
        }
        
        [MenuItem("Game/Create Card Templates/Support Card Template")]
        public static void CreateSupportCardTemplate()
        {
            // Root object
            GameObject root = new GameObject("SupportCard_Template");
            RectTransform rootRt = root.AddComponent<RectTransform>();
            rootRt.sizeDelta = new Vector2(140, 200);
            
            // Background Image
            Image bgImage = root.AddComponent<Image>();
            bgImage.color = new Color(0.25f, 0.2f, 0.3f, 1f);
            
            // Add SupportCard component
            root.AddComponent<SupportCard>();
            root.AddComponent<CardDragHandler>();
            
            // --- Card Image (background) ---
            GameObject cardImageObj = CreateChild(root, "CardImage");
            RectTransform cardImgRt = cardImageObj.GetComponent<RectTransform>();
            cardImgRt.anchorMin = Vector2.zero;
            cardImgRt.anchorMax = Vector2.one;
            cardImgRt.offsetMin = new Vector2(4, 4);
            cardImgRt.offsetMax = new Vector2(-4, -4);
            Image cardImg = cardImageObj.AddComponent<Image>();
            cardImg.color = new Color(0.35f, 0.3f, 0.4f, 1f);
            cardImg.preserveAspect = true;
            
            // --- Card Content Container ---
            GameObject content = CreateChild(root, "Content");
            VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.spacing = 4;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            
            RectTransform contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = Vector2.zero;
            contentRt.anchorMax = Vector2.one;
            contentRt.offsetMin = Vector2.zero;
            contentRt.offsetMax = Vector2.zero;
            
            // --- Name Text ---
            GameObject nameObj = CreateTextChild(content, "NameText", "Support Card", 16, FontStyles.Bold);
            LayoutElement nameLE = nameObj.AddComponent<LayoutElement>();
            nameLE.preferredHeight = 25;
            
            // --- Power Text ---
            GameObject powerObj = CreateTextChild(content, "PowerText", "ATK: 30", 13, FontStyles.Normal);
            LayoutElement powerLE = powerObj.AddComponent<LayoutElement>();
            powerLE.preferredHeight = 18;
            TMP_Text powerText = powerObj.GetComponent<TMP_Text>();
            powerText.color = new Color(1f, 0.6f, 0.6f);
            
            // --- Cost Text ---
            GameObject costObj = CreateTextChild(content, "CostText", "COST: 5", 11, FontStyles.Normal);
            LayoutElement costLE = costObj.AddComponent<LayoutElement>();
            costLE.preferredHeight = 16;
            
            // --- Ability Text ---
            GameObject abilityObj = CreateTextChild(content, "AbilityText", "Effect...", 10, FontStyles.Italic);
            LayoutElement abilityLE = abilityObj.AddComponent<LayoutElement>();
            abilityLE.preferredHeight = 35;
            abilityLE.flexibleHeight = 1;
            TMP_Text abilityText = abilityObj.GetComponent<TMP_Text>();
            abilityText.color = new Color(0.8f, 0.8f, 0.7f);
            abilityText.enableWordWrapping = true;
            
            // --- Charge Text ---
            GameObject chargeObj = CreateTextChild(root, "ChargeText", "0", 11, FontStyles.Bold);
            RectTransform chargeRt = chargeObj.GetComponent<RectTransform>();
            chargeRt.anchorMin = new Vector2(1, 1);
            chargeRt.anchorMax = new Vector2(1, 1);
            chargeRt.pivot = new Vector2(1, 1);
            chargeRt.anchoredPosition = new Vector2(-4, -4);
            chargeRt.sizeDelta = new Vector2(25, 18);
            TMP_Text chargeText = chargeObj.GetComponent<TMP_Text>();
            chargeText.alignment = TextAlignmentOptions.Right;
            
            // --- Rarity Indicator ---
            GameObject rarityBar = CreateChild(root, "RarityBar");
            RectTransform rarityRt = rarityBar.GetComponent<RectTransform>();
            rarityRt.anchorMin = new Vector2(0, 0);
            rarityRt.anchorMax = new Vector2(1, 0);
            rarityRt.pivot = new Vector2(0.5f, 0);
            rarityRt.anchoredPosition = Vector2.zero;
            rarityRt.sizeDelta = new Vector2(0, 4);
            Image rarityImg = rarityBar.AddComponent<Image>();
            rarityImg.color = new Color(0.2f, 0.9f, 0.9f); // Cyan for 3-star
            
            // Link SerializedFields
            LinkSupportCardReferences(root);
            
            Selection.activeGameObject = root;
            
            Debug.Log("SupportCard Template created! Assign SerializedField references and save as Prefab.");
        }
        
        private static GameObject CreateChild(GameObject parent, string name)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent.transform, false);
            child.AddComponent<RectTransform>();
            return child;
        }
        
        private static GameObject CreateTextChild(GameObject parent, string name, string text, int fontSize, FontStyles style)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);
            
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 20);
            
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            
            return obj;
        }
        
        private static void LinkPrimaryCardReferences(GameObject root)
        {
            PrimaryCard card = root.GetComponent<PrimaryCard>();
            if (card == null) return;
            
            // Use SerializedObject to set private fields
            SerializedObject so = new SerializedObject(card);
            
            Transform content = root.transform.Find("Content");
            if (content != null)
            {
                SetSerializedField(so, "nameText", content.Find("NameText")?.GetComponent<TextMeshProUGUI>());
                SetSerializedField(so, "healthText", content.Find("HealthText")?.GetComponent<TextMeshProUGUI>());
                SetSerializedField(so, "powerText", content.Find("PowerText")?.GetComponent<TextMeshProUGUI>());
                SetSerializedField(so, "costText", content.Find("CostText")?.GetComponent<TextMeshProUGUI>());
                SetSerializedField(so, "abilityText", content.Find("AbilityText")?.GetComponent<TextMeshProUGUI>());
            }
            SetSerializedField(so, "cardImage", root.transform.Find("CardImage")?.GetComponent<Image>());
            SetSerializedField(so, "backgroundImage", root.GetComponent<Image>());
            SetSerializedField(so, "chargeText", root.transform.Find("ChargeText")?.GetComponent<TextMeshProUGUI>());
            
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        
        private static void LinkSupportCardReferences(GameObject root)
        {
            SupportCard card = root.GetComponent<SupportCard>();
            if (card == null) return;
            
            SerializedObject so = new SerializedObject(card);
            
            Transform content = root.transform.Find("Content");
            if (content != null)
            {
                SetSerializedField(so, "nameText", content.Find("NameText")?.GetComponent<TextMeshProUGUI>());
                SetSerializedField(so, "powerText", content.Find("PowerText")?.GetComponent<TextMeshProUGUI>());
                SetSerializedField(so, "costText", content.Find("CostText")?.GetComponent<TextMeshProUGUI>());
                SetSerializedField(so, "abilityText", content.Find("AbilityText")?.GetComponent<TextMeshProUGUI>());
            }
            SetSerializedField(so, "cardImage", root.transform.Find("CardImage")?.GetComponent<Image>());
            SetSerializedField(so, "backgroundImage", root.GetComponent<Image>());
            SetSerializedField(so, "chargeText", root.transform.Find("ChargeText")?.GetComponent<TextMeshProUGUI>());
            
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        
        private static void SetSerializedField(SerializedObject so, string fieldName, Object value)
        {
            SerializedProperty prop = so.FindProperty(fieldName);
            if (prop != null && value != null)
            {
                prop.objectReferenceValue = value;
            }
        }
    }
}
