using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace Game
{
    /// <summary>
    /// 主力資格カード用MonoBehaviour
    /// 体力表示を追加、クリックでアクション選択
    /// </summary>
    public class PrimaryCard : CardBase, IPointerClickHandler
    {
        [Header("Primary Card UI")]
        [SerializeField] private TextMeshProUGUI healthText;

        private void Awake()
        {
            // Ensure this GameObject has an Image for raycast detection
            var img = GetComponent<UnityEngine.UI.Image>();
            if (img == null)
            {
                img = gameObject.AddComponent<UnityEngine.UI.Image>();
                img.color = new Color(0, 0, 0, 0); // Fully transparent
            }
            img.raycastTarget = true;
        }
        
        /// <summary>
        /// 体力値
        /// </summary>
        public int Health { get; private set; }
        
        /// <summary>
        /// 現在の体力（バトル中に変動）
        /// </summary>
        public int CurrentHealth { get; private set; }
        
        /// <summary>
        /// カードが倒されたかどうか
        /// </summary>
        public bool IsDead => CurrentHealth <= 0;
        
        /// <summary>
        /// PrimaryCardDataを使用して初期化
        /// </summary>
        public void Initialize(PrimaryCardData data)
        {
            base.Initialize(data);
            
            this.Health = data.Health;
            this.CurrentHealth = data.Health;
            
            UpdateUI();
        }
        
        /// <summary>
        /// CardDataBaseからの初期化（互換性用）
        /// </summary>
        public override void Initialize(CardDataBase data)
        {
            if (data is PrimaryCardData primaryData)
            {
                Initialize(primaryData);
            }
            else
            {
                base.Initialize(data);
                Debug.LogWarning($"PrimaryCard initialized with non-PrimaryCardData: {data.CardId}");
            }
        }
        
        protected override void UpdateUI()
        {
            base.UpdateUI();
            
            if (healthText != null)
            {
                healthText.text = CurrentHealth.ToString();
                Debug.Log($"[PrimaryCard] UpdateUI: {gameObject.name} - HP: {CurrentHealth}");
            }
            else
            {
                Debug.LogWarning($"[PrimaryCard] healthText is null on {gameObject.name}");
            }
        }
        
        /// <summary>
        /// ダメージを受ける
        /// </summary>
        public void TakeDamage(int amount)
        {
            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            UpdateUI();
        }
        
        /// <summary>
        /// 回復する
        /// </summary>
        public void Heal(int amount)
        {
            CurrentHealth = Mathf.Min(Health, CurrentHealth + amount);
            UpdateUI();
        }
        
        /// <summary>
        /// 体力をリセット
        /// </summary>
        public void ResetHealth()
        {
            CurrentHealth = Health;
            UpdateUI();
        }
        
        /// <summary>
        /// 生存しているか
        /// </summary>
        public bool IsAlive => CurrentHealth > 0;

        /// <summary>
        /// クリック時にアクション選択UIを開く
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"[PrimaryCard] Clicked: {gameObject.name}");
            
            // Check if in target selection mode
            var targetMode = FindAnyObjectByType<Game.UI.TargetSelectionMode>();
            if (targetMode != null && targetMode.IsSelecting)
            {
                // Select this card as target - use this (CardBase) directly
                targetMode.SelectTarget(this);
                return;
            }
            
            // Normal mode: Show action selection
            var battleScene = FindAnyObjectByType<Game.Scenes.BattleScene>();
            if (battleScene != null)
            {
                Debug.Log($"[PrimaryCard] Calling ShowActionSelection for card: {Name}");
                battleScene.ShowActionSelection(this);
            }
            else
            {
                Debug.LogWarning("[PrimaryCard] BattleScene not found in scene!");
            }
        }
    }
}
