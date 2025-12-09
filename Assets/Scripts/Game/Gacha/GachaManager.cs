using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game.Data;
using Game.Network;
using Game.UI;

namespace Game.Gacha
{
    public class GachaManager : MonoBehaviour
    {
        public static GachaManager Instance { get; private set; }

        [SerializeField] private GachaRateTable rateTable;
        [SerializeField] private bool isDebugMode = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                
                // Keep existing RateTable if assigned via Inspector, otherwise auto-load
                if (rateTable == null || (rateTable.Featured5Stars == null || rateTable.Featured5Stars.Count == 0))
                {
                    LoadFromResources();
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void Initialize(GachaRateTable table)
        {
            rateTable = table;
        }

        public const int TICKET_COST_SINGLE = 1;
        public const int TICKET_COST_TEN = 10;

        public async Task<CardDataBase> PullSingle(UserData user)
        {
            if (rateTable == null)
            {
                LoadFromResources();
                if (rateTable == null)
                {
                    Debug.LogError("GachaRateTable is not assigned and auto-load failed!");
                    return null;
                }
            }

            // Ticket Check
            if (user.GachaTickets < TICKET_COST_SINGLE)
            {
                Debug.LogWarning("Not enough Gacha Tickets!");
                return null;
            }

            // Save Before Pull (if not debug)
            if (!isDebugMode && ApiClient.Instance != null)
            {
                 SaveOverlayManager.Instance?.Show("保存中...");
                 await ApiClient.Instance.SaveUserData(user);
                 SaveOverlayManager.Instance?.Hide();
            }

            user.GachaTickets -= TICKET_COST_SINGLE;

            var result = PullInternal(user);
            if (result != null)
            {
                user.AddCard(result.CardId);
            }

            // Save After Pull (if not debug)
            if (!isDebugMode && ApiClient.Instance != null)
            {
                 SaveOverlayManager.Instance?.Show("保存中...");
                 await ApiClient.Instance.SaveUserData(user);
                 SaveOverlayManager.Instance?.Hide();
            }

            return result;
        }

        public async Task<List<CardDataBase>> PullTen(UserData user)
        {
            if (rateTable == null)
            {
                LoadFromResources();
                if (rateTable == null)
                {
                    Debug.LogError("GachaRateTable is not assigned and auto-load failed!");
                    return null;
                }
            }

            // Ticket Check
            if (user.GachaTickets < TICKET_COST_TEN)
            {
                Debug.LogWarning("Not enough Gacha Tickets!");
                return null;
            }

            // Save Before Pull
            if (!isDebugMode && ApiClient.Instance != null)
            {
                 SaveOverlayManager.Instance?.Show("保存中...");
                 await ApiClient.Instance.SaveUserData(user);
                 SaveOverlayManager.Instance?.Hide();
            }

            user.GachaTickets -= TICKET_COST_TEN;

            List<CardDataBase> results = new List<CardDataBase>();
            bool hasHighRarity = false;

            for (int i = 0; i < 10; i++)
            {
                CardDataBase card = null;

                // If it's the 10th pull (index 9) and we haven't seen a >= 4 star card yet,
                // force the guarantee logic.
                if (i == 9 && !hasHighRarity)
                {
                    card = PullGuaranteed4Star(user);
                }
                else
                {
                    card = PullInternal(user);
                }

                if (card != null)
                {
                    if (card.Rarity >= 4)
                    {
                        hasHighRarity = true;
                    }

                    user.AddCard(card.CardId);
                    results.Add(card);
                }
            }

            // Save After Pull
            if (!isDebugMode && ApiClient.Instance != null)
            {
                 SaveOverlayManager.Instance?.Show("保存中...");
                 await ApiClient.Instance.SaveUserData(user);
                 SaveOverlayManager.Instance?.Hide();
            }

            return results;
        }

        private CardDataBase PullInternal(UserData user)
        {
            float current5StarRate = user.IsFirstGacha ? rateTable.Rate5StarFirstTime : rateTable.Rate5Star;
            float roll = Random.value; // 0.0 to 1.0

            CardDataBase resultCard = null;

            if (roll < current5StarRate)
            {
                // 5-Star Logic
                resultCard = Select5Star();
            }
            else if (roll < current5StarRate + rateTable.Rate4Star)
            {
                // 4-Star Logic
                resultCard = Select4Star();
            }
            else
            {
                // 3-Star Logic
                resultCard = Select3Star();
            }
            
            // 初回ボーナス消費
            if (user.IsFirstGacha)
            {
                user.IsFirstGacha = false;
                user.MarkUpdated();
            }

            return resultCard;
        }

        private CardDataBase Select5Star()
        {
            // すり抜け判定 (Spook Check)
            // 50% chance to pick from Featured, 50% from Standard
            // If Standard pool is empty, force Featured. If Featured empty, force Standard.
            
            bool isFeatured = Random.value >= rateTable.SpookRate;
            
            List<CardDataBase> targetPool = null;

            if (rateTable.Featured5Stars != null && rateTable.Featured5Stars.Count > 0 &&
                rateTable.Standard5Stars != null && rateTable.Standard5Stars.Count > 0)
            {
                targetPool = isFeatured ? rateTable.Featured5Stars : rateTable.Standard5Stars;
            }
            else if (rateTable.Featured5Stars != null && rateTable.Featured5Stars.Count > 0)
            {
                targetPool = rateTable.Featured5Stars;
            }
            else
            {
                targetPool = rateTable.Standard5Stars;
            }

            return PickRandom(targetPool);
        }

        private CardDataBase Select4Star()
        {
            // 50% Support, 50% Special
            return SelectFromMixedPool(rateTable.Pool4StarSupport, rateTable.Pool4StarSpecial);
        }

        private CardDataBase Select3Star()
        {
            // 50% Support, 50% Special
            return SelectFromMixedPool(rateTable.Pool3StarSupport, rateTable.Pool3StarSpecial);
        }

        private CardDataBase SelectFromMixedPool(List<CardDataBase> poolA, List<CardDataBase> poolB)
        {
            bool hasA = poolA != null && poolA.Count > 0;
            bool hasB = poolB != null && poolB.Count > 0;
            
            // If both pools are empty, return null (caller should handle this)
            if (!hasA && !hasB)
            {
                Debug.LogWarning("SelectFromMixedPool: Both pools are empty!");
                return null;
            }
            
            // If only one pool has cards, use that
            if (hasA && !hasB) return PickRandom(poolA);
            if (!hasA && hasB) return PickRandom(poolB);
            
            // Both pools have cards, pick randomly
            bool pickA = Random.value < 0.5f;
            return PickRandom(pickA ? poolA : poolB);
        }

        private CardDataBase PullGuaranteed4Star(UserData user)
        {
            // 4-Star or Higher Guarantee
            // Logic: 
            // - 5-Star Rate: Standard (3% or 6%)
            // - 4-Star Rate: Remainder (97% or 94%)
            // - 3-Star Rate: 0%

            float current5StarRate = user.IsFirstGacha ? rateTable.Rate5StarFirstTime : rateTable.Rate5Star;
            float roll = Random.value; 

            CardDataBase resultCard = null;

            if (roll < current5StarRate)
            {
                 resultCard = Select5Star();
            }
            else
            {
                 resultCard = Select4Star();
            }
            
            // Consume First Bonus if applicable (even on guarantee pull, it counts as a pull)
            if (user.IsFirstGacha)
            {
                user.IsFirstGacha = false;
                user.MarkUpdated();
            }

            return resultCard;
        }

        public void LoadFromResources()
        {
            if (rateTable == null)
            {
                rateTable = ScriptableObject.CreateInstance<GachaRateTable>();
                rateTable.Rate5Star = 0.03f;
                rateTable.Rate5StarFirstTime = 0.06f;
                rateTable.Rate4Star = 0.15f;
                rateTable.SpookRate = 0.5f;
            }

            // Real Cards - use CardDataBase to support PrimaryCardData
            rateTable.Featured5Stars = new List<CardDataBase>();
            rateTable.Standard5Stars = new List<CardDataBase>();
            rateTable.Pool4StarSupport = new List<CardDataBase>();
            rateTable.Pool4StarSpecial = new List<CardDataBase>();
            rateTable.Pool3StarSupport = new List<CardDataBase>();
            rateTable.Pool3StarSpecial = new List<CardDataBase>();

            // Load 3-Stars (3x)
            var cards3 = Resources.LoadAll<CardDataBase>("Cards/3x");
            foreach(var c in cards3)
            {
                if (c.CardType == CardType.Support) rateTable.Pool3StarSupport.Add(c);
                else rateTable.Pool3StarSpecial.Add(c);
            }

            // Load 4-Stars (4x)
            var cards4 = Resources.LoadAll<CardDataBase>("Cards/4x");
            foreach(var c in cards4)
            {
                if (c.CardType == CardType.Support) rateTable.Pool4StarSupport.Add(c);
                else rateTable.Pool4StarSpecial.Add(c);
            }

            // Load 5-Stars (5x) - These are PrimaryCardData
            var cards5 = Resources.LoadAll<CardDataBase>("Cards/5x");
            foreach(var c in cards5)
            {
                // 5A, 5B, 5C as Featured
                if (c.name == "5A" || c.name == "5B" || c.name == "5C") rateTable.Featured5Stars.Add(c);
                else rateTable.Standard5Stars.Add(c);
            }
            
            Debug.Log($"GachaManager data loaded from Resources. ☆5: {rateTable.Featured5Stars.Count+rateTable.Standard5Stars.Count}, ☆4: {rateTable.Pool4StarSupport.Count+rateTable.Pool4StarSpecial.Count}, ☆3: {rateTable.Pool3StarSupport.Count+rateTable.Pool3StarSpecial.Count}");
        }

        private CardDataBase PickRandom(List<CardDataBase> pool)
        {
            if (pool == null || pool.Count == 0) return null;
            int index = Random.Range(0, pool.Count);
            return pool[index];
        }
    }
}
