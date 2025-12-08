using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game.Data;
using Game.Network;

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

        public async Task<CardData> PullSingle(UserData user)
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
                 await ApiClient.Instance.SaveUserData(user);
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
                 await ApiClient.Instance.SaveUserData(user);
            }

            return result;
        }

        public async Task<List<CardData>> PullTen(UserData user)
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
                 await ApiClient.Instance.SaveUserData(user);
            }

            user.GachaTickets -= TICKET_COST_TEN;

            List<CardData> results = new List<CardData>();
            bool hasHighRarity = false;

            for (int i = 0; i < 10; i++)
            {
                CardData card = null;

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
                 await ApiClient.Instance.SaveUserData(user);
            }

            return results;
        }

        private CardData PullInternal(UserData user)
        {
            float current5StarRate = user.IsFirstGacha ? rateTable.Rate5StarFirstTime : rateTable.Rate5Star;
            float roll = Random.value; // 0.0 to 1.0

            CardData resultCard = null;

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

        private CardData Select5Star()
        {
            // すり抜け判定 (Spook Check)
            // 50% chance to pick from Featured, 50% from Standard
            // If Standard pool is empty, force Featured. If Featured empty, force Standard.
            
            bool isFeatured = Random.value >= rateTable.SpookRate;
            
            List<CardData> targetPool = null;

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

        private CardData Select4Star()
        {
            // 50% Support, 50% Special
            return SelectFromMixedPool(rateTable.Pool4StarSupport, rateTable.Pool4StarSpecial);
        }

        private CardData Select3Star()
        {
            // 50% Support, 50% Special
            return SelectFromMixedPool(rateTable.Pool3StarSupport, rateTable.Pool3StarSpecial);
        }

        private CardData SelectFromMixedPool(List<CardData> poolA, List<CardData> poolB)
        {
            List<CardData> targetPool = null;
            
            // Assuming 50/50 split if both pools are available
            bool pickA = Random.value < 0.5f;

            if (poolA != null && poolA.Count > 0 && poolB != null && poolB.Count > 0)
            {
                targetPool = pickA ? poolA : poolB;
            }
            else if (poolA != null && poolA.Count > 0)
            {
                targetPool = poolA;
            }
            else
            {
                targetPool = poolB;
            }

            return PickRandom(targetPool);
        }

        private CardData PullGuaranteed4Star(UserData user)
        {
            // 4-Star or Higher Guarantee
            // Logic: 
            // - 5-Star Rate: Standard (3% or 6%)
            // - 4-Star Rate: Remainder (97% or 94%)
            // - 3-Star Rate: 0%

            float current5StarRate = user.IsFirstGacha ? rateTable.Rate5StarFirstTime : rateTable.Rate5Star;
            float roll = Random.value; 

            CardData resultCard = null;

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

            // Real Cards
            rateTable.Featured5Stars = new List<CardData>();
            rateTable.Standard5Stars = new List<CardData>();
            rateTable.Pool4StarSupport = new List<CardData>();
            rateTable.Pool4StarSpecial = new List<CardData>();
            rateTable.Pool3StarSupport = new List<CardData>();
            rateTable.Pool3StarSpecial = new List<CardData>();

            // Load 3-Stars (3x)
            var cards3 = Resources.LoadAll<CardData>("Cards/3x");
            foreach(var c in cards3)
            {
                InjectRarity(c, 3);
                if (c.CardType == CardType.Support) rateTable.Pool3StarSupport.Add(c);
                else rateTable.Pool3StarSpecial.Add(c);
            }

            // Load 4-Stars (4x)
            var cards4 = Resources.LoadAll<CardData>("Cards/4x");
            foreach(var c in cards4)
            {
                InjectRarity(c, 4);
                if (c.CardType == CardType.Support) rateTable.Pool4StarSupport.Add(c);
                else rateTable.Pool4StarSpecial.Add(c);
            }

            // Load 5-Stars (5x)
            var cards5 = Resources.LoadAll<CardData>("Cards/5x");
            foreach(var c in cards5)
            {
                InjectRarity(c, 5);
                // 5A, 5B, 5C as Featured (based on user request in previous turn diff)
                if (c.name == "5A" || c.name == "5B" || c.name == "5C") rateTable.Featured5Stars.Add(c);
                else rateTable.Standard5Stars.Add(c);
            }
            
            Debug.Log($"GachaManager data loaded from Resources. ☆5: {rateTable.Featured5Stars.Count+rateTable.Standard5Stars.Count}, ☆4: {rateTable.Pool4StarSupport.Count+rateTable.Pool4StarSpecial.Count}, ☆3: {rateTable.Pool3StarSupport.Count+rateTable.Pool3StarSpecial.Count}");
        }

        private void InjectRarity(CardData card, int rarity)
        {
            var field = typeof(CardData).GetField("rarity", global::System.Reflection.BindingFlags.NonPublic | global::System.Reflection.BindingFlags.Instance);
            if(field != null) field.SetValue(card, rarity);
        }

        private CardData PickRandom(List<CardData> pool)
        {
            if (pool == null || pool.Count == 0) return null;
            int index = Random.Range(0, pool.Count);
            return pool[index];
        }
    }
}
