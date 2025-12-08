using UnityEngine;
using TMPro;

namespace Game
{
    /// <summary>
    /// 主力資格カード用MonoBehaviour
    /// 体力表示を追加
    /// </summary>
    public class PrimaryCard : CardBase
    {
        [Header("Primary Card UI")]
        [SerializeField] private TextMeshProUGUI healthText;
        
        /// <summary>
        /// 体力値
        /// </summary>
        public int Health { get; private set; }
        
        /// <summary>
        /// 現在の体力（バトル中に変動）
        /// </summary>
        public int CurrentHealth { get; private set; }
        
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
                healthText.text = $"HP: {CurrentHealth}/{Health}";
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
    }
}
