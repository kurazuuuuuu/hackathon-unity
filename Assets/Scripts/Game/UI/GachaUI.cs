using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Game.Gacha;
using Game.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using Game.System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.UI
{
    public class GachaUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button btnSinglePull;
        [SerializeField] private Button btnTenPull;
        [SerializeField] private Button btnBack; // ホーム画面に戻る
        [SerializeField] private Text txtStatus; // Optional: To show status/results
        [SerializeField] private Text txtTicketCount; // Shows current tickets
        [SerializeField] private GachaDirector ticketDirector; // New Dependency

        [Header("Debug User Data")]
        [SerializeField] private bool useTestProfile = true;
        private UserData currentUser;

        private void Start()
        {
            btnBack.onClick.AddListener(OnBack);
            // Fix: Remove accidental Animator that might reset position
            var anim = GetComponent<Animator>();
            if (anim != null) Destroy(anim);

            EnsureManagerExists();
            InitializeUser();
            SetupButtons();
            SetInteractable(true);
            UpdateTicketUI();
        }

        private void EnsureManagerExists()
        {
            if (GachaManager.Instance == null)
            {
                var mgr = FindObjectOfType<GachaManager>();
                if (mgr != null) return; // Instance prop might not be set yet if script exec order issue
                
                Debug.Log("GachaUI: No GachaManager found. Creating one automatically.");
                GameObject go = new GameObject("GachaManager_Auto");
                go.AddComponent<GachaManager>();
            }
        }

        private void InitializeUser()
        {
            currentUser = new UserData("Player");
            currentUser.GachaTickets = 100; // Default fallback

            if (useTestProfile)
            {
                string testDataPath = Path.Combine(Application.persistentDataPath, "test_user_profile.json");
                if (File.Exists(testDataPath))
                {
                    try
                    {
                        string json = File.ReadAllText(testDataPath);
                        UserGameProfileDto profile = JsonUtility.FromJson<UserGameProfileDto>(json);
                        if (profile != null)
                        {
                            currentUser.FromApiProfile(profile);
                            Debug.Log($"GachaUI: Loaded test user. Tickets: {currentUser.GachaTickets}, Owned: {currentUser.OwnedCards.Count}");
                        }
                    }
                    catch (global::System.Exception e)
                    {
                        Debug.LogWarning($"GachaUI: Failed to load test profile: {e.Message}");
                    }
                }
            }
            UpdateTicketUI();
        }

        private void SetupButtons()
        {
            if (btnBack != null)
            {
                btnBack.onClick.RemoveAllListeners();
                btnBack.onClick.AddListener(OnBack);
            }
            if (btnSinglePull != null)
            {
                btnSinglePull.onClick.RemoveAllListeners();
                btnSinglePull.onClick.AddListener(OnSinglePull);
            }

            if (btnTenPull != null)
            {
                btnTenPull.onClick.RemoveAllListeners();
                btnTenPull.onClick.AddListener(OnTenPull);
            }
        }

        public async void OnSinglePull()
        {
            if (GachaManager.Instance == null)
            {
                Debug.LogError("GachaUI: GachaManager.Instance is null! Cannot pull.");
                return;
            }
            
            SetInteractable(false);
            Debug.Log("GachaUI: Pulling Single...");
            
            var card = await GachaManager.Instance.PullSingle(currentUser);
            
            if (card != null)
            {
                Debug.Log($"GachaUI: Pulled {card.CardName} (☆{card.Rarity})");
                PlaySequence(new List<CardData>{card});
            }
            else
            {
                UpdateStatus("Pull Failed!");
                SetInteractable(true);
            }
            // Logic moved to Director for display
            UpdateTicketUI();
        }

        private void PlaySequence(List<CardData> cards)
        {
            if (ticketDirector != null)
            {
                ticketDirector.PlayGachaSequence(cards, () => {
                    SetInteractable(true);
                    UpdateTicketUI();
                });
            }
            else
            {
                // Fallback if no director
                UpdateStatus("Pull Done (No Director)");
                SetInteractable(true);
                UpdateTicketUI();
            }
        }

        public async void OnTenPull()
        {
            if (GachaManager.Instance == null)
            {
                Debug.LogError("GachaUI: GachaManager.Instance is null! Cannot pull.");
                return;
            }

            SetInteractable(false);
            Debug.Log("GachaUI: Pulling 10...");

            var cards = await GachaManager.Instance.PullTen(currentUser);

            if (cards != null && cards.Count > 0)
            {
                Debug.Log($"GachaUI: 10-Pull Complete. Got {cards.Count} cards.");
                PlaySequence(cards);
            }
            else
            {
                Debug.LogWarning("GachaUI: 10-Pull Failed");
                UpdateStatus("10-Pull Failed!");
                SetInteractable(true);
            }
            UpdateTicketUI();
        }

        private void SetInteractable(bool state)
        {
            if (btnSinglePull != null) btnSinglePull.interactable = state;
            if (btnTenPull != null) btnTenPull.interactable = state;
        }

        private void UpdateStatus(string msg)
        {
            if (txtStatus != null) txtStatus.text = msg;
        }

        private void UpdateTicketUI()
        {
            if (txtTicketCount != null && currentUser != null)
            {
                txtTicketCount.text = $"Tickets: {currentUser.GachaTickets}";
            }
        }

        private async void OnBack()
        {
            if (SceneController.Instance != null)
            {
                await SceneController.Instance.GoToHome();
            }
        }

        // --- Editor Tools ---

#if UNITY_EDITOR
        [ContextMenu("Create Buttons")]
        private void CreateButtons()
        {
            // Helper to create Button with Text
            GameObject CreateButton(string name, string label, Color color, Vector2 pos)
            {
                GameObject btnObj = new GameObject(name);
                btnObj.transform.SetParent(this.transform, false);
                
                Image img = btnObj.AddComponent<Image>();
                img.color = color;

                Button btn = btnObj.AddComponent<Button>();
                
                
                RectTransform rect = btnObj.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(160, 50);
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = pos;

                // Create Text Child (Using Legacy Text for simplicity as requested 'Texture later', ensuring no TMP dependency issues specifically for this simple request)
                // If TMP is Standard, we should use it, but user said 'Simple single color' for now.
                GameObject txtObj = new GameObject("Text");
                txtObj.transform.SetParent(btnObj.transform, false);
                Text text = txtObj.AddComponent<Text>();
                text.text = label;
                text.alignment = TextAnchor.MiddleCenter;
                text.color = Color.white;
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); // Fallback
                if (text.font == null) text.font = Resources.FindObjectsOfTypeAll<Font>()[0]; // Grab any font

                RectTransform txtRect = txtObj.GetComponent<RectTransform>();
                txtRect.anchorMin = Vector2.zero;
                txtRect.anchorMax = Vector2.one;
                txtRect.sizeDelta = Vector2.zero;

                return btnObj;
            }

            if (btnSinglePull == null)
            {
                var go = CreateButton("Btn_SinglePull", "1 Pull", Color.blue, new Vector2(-100, 0));
                btnSinglePull = go.GetComponent<Button>();
            }

            if (btnTenPull == null)
            {
                var go = CreateButton("Btn_TenPull", "10 Pull", Color.red, new Vector2(100, 0));
                btnTenPull = go.GetComponent<Button>();
            }
            


            if (txtTicketCount == null)
            {
                // Create Ticket Counter Box
                GameObject boxObj = new GameObject("Img_TicketBG");
                boxObj.transform.SetParent(this.transform, false);
                
                Image img = boxObj.AddComponent<Image>();
                img.color = new Color(0.2f, 0.2f, 0.2f, 1f); // Dark Grey

                RectTransform rect = boxObj.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(200, 40);
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(0, 100);

                // Create Text
                GameObject txtObj = new GameObject("Text_Tickets");
                txtObj.transform.SetParent(boxObj.transform, false);
                Text text = txtObj.AddComponent<Text>();
                text.text = "Tickets: ---";
                text.alignment = TextAnchor.MiddleCenter;
                text.color = Color.white;
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (text.font == null) text.font = Resources.FindObjectsOfTypeAll<Font>()[0];

                RectTransform txtRect = txtObj.GetComponent<RectTransform>();
                txtRect.anchorMin = Vector2.zero;
                txtRect.anchorMax = Vector2.one;
                txtRect.sizeDelta = Vector2.zero;
                
                txtTicketCount = text;
            }
            
            // Mark scene dirty
            if (ticketDirector == null)
            {
                ticketDirector = GetComponentInChildren<GachaDirector>();
                if (ticketDirector == null)
                {
                    GameObject dirObj = new GameObject("GachaDirector");
                    dirObj.transform.SetParent(this.transform, false);
                    ticketDirector = dirObj.AddComponent<GachaDirector>();
                    // It will require SetupComponents
                }
            }

            // Mark scene dirty
            EditorUtility.SetDirty(this);
        }
#endif
    }
}
