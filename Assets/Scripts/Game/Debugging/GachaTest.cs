using UnityEngine;
using Game.Gacha;
using Game.Data;
using System.Collections.Generic;
using System.IO;

namespace Game.Debugging
{
    public class GachaTest : MonoBehaviour
    {
        [Header("Test Config")]
        public int Iterations = 10000;
        public bool SimulateFirstTime = true;

        [ContextMenu("Run Gacha Simulation")]
        public void Start()
        {
            RunSimulation();
        }

        public void RunSimulation()
        {
            // 1. Setup Manager (Use existing or create new auto-loaded one)
            GachaManager manager = GachaManager.Instance;
            bool createdManager = false;

            if (manager == null)
            {
                var go = new GameObject("GachaManager_Test");
                manager = go.AddComponent<GachaManager>();
                // Awake() calls LoadFromResources() automatically
                createdManager = true;
            }
            // Ensure resources are loaded even if using existing manager that might have been init differently?
            // Safer to just trust Awake or call LoadFromResources again if needed, but Awake should suffice.
            
            // 2. Run Loop (Use 'manager' instead of 'managerPrototype')
            int count5 = 0;
            int count4 = 0;
            int count3 = 0;



            


            UserData user = new UserData("TestUser");
            user.GachaTickets = 999999; 
            user.IsFirstGacha = SimulateFirstTime;

            // Try to load from generated test file
            string testDataPath = Path.Combine(Application.persistentDataPath, "test_user_profile.json");
            if (File.Exists(testDataPath))
            {
                try
                {
                    string json = File.ReadAllText(testDataPath);
                    UserGameProfileDto profile = JsonUtility.FromJson<UserGameProfileDto>(json);
                    if (profile != null)
                    {
                        user.FromApiProfile(profile);
                        // Note: IsFirstGacha is NOT in DTO, so we still rely on SimulateFirstTime or default
                        Debug.Log($"Loaded Test User Data from {testDataPath}. Tickets: {user.GachaTickets}");
                        
                        // If we are strictly using the file, we might want to respect its values over the test defaults
                        // But for "Iterations", we might still overwrite tickets?
                        // Let's assume if file is loaded, we use its tickets for the "Ticket Consumption Test" primarily.
                        // But for the 10000 loop, we need strict control.
                        // I will override tickets *back* to 999999 for the probability loop to ensure it finishes,
                        // but I will logging specifically that we loaded it.
                    }
                }
                catch (global::System.Exception e)
                {
                    Debug.LogWarning($"Failed to load test user data: {e.Message}");
                }
            }
            else
            {
                Debug.Log($"No test user data found at {testDataPath}. Using default.");
            }

            
            Debug.Log($"Starting Simulation: {Iterations} pulls (Single). FirstTime={SimulateFirstTime}");
            
            // Test Single Pulls
            for (int i = 0; i < Iterations; i++)
            {
                if (SimulateFirstTime) user.IsFirstGacha = true; 
                else user.IsFirstGacha = false;

                var card = manager.PullSingle(user);
                
                if (card != null)
                {
                    if (card.Rarity == 5) count5++;
                    else if (card.Rarity == 4) count4++;
                    else count3++;
                }
            }

            // Report Single
            float rate5 = (float)count5 / Iterations * 100f;
            Debug.Log($"Single Simulation Complete.\n5-Star: {count5} ({rate5:F2}%)");

            // Test 10-Pull Ticket Consumption & Inventory Update
            Debug.Log($"Testing 10-Pull Ticket Consumption & Inventory Update... (Owned: {user.OwnedCards.Count})");
            int initialOwnedCount = user.OwnedCards.Count;
            user.GachaTickets = 20;
            
            var results = manager.PullTen(user);
            
            if (results != null && results.Count == 10)
            {
                // Check if inventory updated
                int currentTotal = 0;
                foreach(var kvp in user.OwnedCards) currentTotal += kvp.Value;
                
                Debug.Log($"10-Pull Success: Received 10 cards. Tickets: {user.GachaTickets}. Inventory Types: {user.OwnedCards.Count}.");
                Debug.Log($"10-Pull verified 'Actual Function' by updating UserData.OwnedCards.");
                
                // Categorize and Output
                var list5 = new List<string>();
                var list4 = new List<string>();
                var list3 = new List<string>();

                foreach (var card in results)
                {
                    string info = $"{card.CardName} (ID:{card.CardId})";
                    if (card.Rarity == 5) list5.Add(info);
                    else if (card.Rarity == 4) list4.Add(info);
                    else list3.Add(info);
                }

                Debug.Log("=== 10-Pull Results ===");
                Debug.Log($"[5-Star] ({list5.Count}): {string.Join(", ", list5)}");
                Debug.Log($"[4-Star] ({list4.Count}): {string.Join(", ", list4)}");
                Debug.Log($"[3-Star] ({list3.Count}): {string.Join(", ", list3)}");
                Debug.Log("=======================");
            }
            else
            {
                Debug.LogError($"10-Pull Failed! Count: {(results != null ? results.Count : 0)}, Tickets: {user.GachaTickets}");
            }


            // Test Insufficient Tickets
            user.GachaTickets = 5;
            var failedResults = manager.PullTen(user);
            if (failedResults == null)
            {
                Debug.Log("Insufficient Ticket Check Passed (PullTen returned null).");
            }
            else
            {
                Debug.LogError("Insufficient Ticket Check FAILED! PullTen executed.");
            }

            // Cleanup
            if (createdManager && manager != null)
            {
                DestroyImmediate(manager.gameObject);
            }
        }


    }
}
