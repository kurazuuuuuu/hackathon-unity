using System;
using UnityEngine;
using TMPro;
using Game;
using Game.Abilities;

public class Card : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI powerText;
    [SerializeField] private TextMeshProUGUI healText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI abilityText;
    [SerializeField] private TextMeshProUGUI chargeText;

    [Header("Drag Settings")]
    [SerializeField] private bool isDraggable = true;

    // カードの基本情報
    public string Name { get; private set; }
    public CardType Type { get; private set; }
    public int Power { get; private set; }
    
    // Health (Primary cards have HP)
    public int MaxHealth { get; private set; }
    public int CurrentHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0;

    public int Heal { get; private set; }
    public CardAbility Ability { get; private set; }
    public int Charge { get; private set; }
    public int Cost { get; private set; }
    public int ActionCost { get; private set; }

    // ドラッグ状態
    public bool IsDraggable
    {
        get => isDraggable;
        set
        {
            if (isDraggable != value)
            {
                isDraggable = value;
                OnDraggableChanged?.Invoke(isDraggable);
            }
        }
    }

    public bool IsDragging { get; private set; } = false;

    // イベント
    public event Action<bool> OnDraggableChanged;
    public event Action OnDragStart;
    public event Action OnDragEnd;
    public event Action<int> OnHealthChanged; // Event for health updates

    /// <summary>
    /// CardDataを使用してカードを初期化する
    /// </summary>
    /// <param name="data">カードデータ</param>
    public void Initialize(CardData data)
    {
        if (data == null)
        {
            Debug.LogError("CardData is null!");
            return;
        }

        this.Name = data.CardName;
        this.Type = data.CardType;
        this.Power = data.Power;
        this.Heal = data.Heal;
        this.Ability = data.Ability;
        this.Charge = data.Charge;
        this.Cost = data.Cost;
        this.ActionCost = data.ActionCost;

        // Initialize Health
        this.MaxHealth = data.Health;
        this.CurrentHealth = this.MaxHealth;

        UpdateUI();
    }
    
    /// <summary>
    /// CardDataBaseを使用してカードを初期化する（新API）
    /// </summary>
    public void Initialize(Game.CardDataBase data)
    {
        if (data == null)
        {
            Debug.LogError("CardDataBase is null!");
            return;
        }
        
        // CardDataの場合は既存メソッドを使用
        if (data is CardData legacyData)
        {
            Initialize(legacyData);
            return;
        }

        this.Name = data.CardName;
        this.Type = data.CardType;
        this.Power = data.Power;
        this.Heal = 0; // CardDataBaseにはHealがない
        this.Ability = data.Ability;
        this.Charge = data.Charge;
        this.Cost = data.Cost;
        this.ActionCost = data.ActionCost;
        
        // Handle Primary Card Data (New API)
        if (data is PrimaryCardData primaryData)
        {
            this.MaxHealth = primaryData.Health;
            this.CurrentHealth = this.MaxHealth;
        }
        else
        {
            this.MaxHealth = 0; 
            this.CurrentHealth = 0;
        }

        UpdateUI();
    }

    /// <summary>
    /// CardDataを使用してカードを初期化する（ガチャ結果表示用）
    /// レアリティに応じた見た目も適用する
    /// </summary>
    public void InitializeForGacha(CardData data)
    {
        Initialize(data);
        
        // Load card image
        LoadCardImage(data.CardId, data.Rarity);
        
        // Apply rarity visual style
        var visualizer = GetComponent<Game.UI.CardVisualizer>();
        if (visualizer == null)
        {
            visualizer = gameObject.AddComponent<Game.UI.CardVisualizer>();
        }
        visualizer.ApplyRarityStyle(data.Rarity);
    }
    
    /// <summary>
    /// CardDataBaseを使用してカードを初期化する（ガチャ結果表示用・新API）
    /// </summary>
    public void InitializeForGacha(Game.CardDataBase data)
    {
        Initialize(data);
        
        // Load card image
        LoadCardImage(data.CardId, data.Rarity);
        
        var visualizer = GetComponent<Game.UI.CardVisualizer>();
        if (visualizer == null)
        {
            visualizer = gameObject.AddComponent<Game.UI.CardVisualizer>();
        }
        visualizer.ApplyRarityStyle(data.Rarity);
    }
    
    /// <summary>
    /// カード画像をResources/Textures/Cards/からロード
    /// </summary>
    private void LoadCardImage(string cardId, int rarity)
    {
        // Find CardImage child
        Transform cardImageTransform = transform.Find("CardImage");
        if (cardImageTransform == null)
        {
            // Try Content/CardImage
            Transform content = transform.Find("Content");
            if (content != null) cardImageTransform = content.Find("CardImage");
        }
        
        if (cardImageTransform == null) return;
        
        var cardImage = cardImageTransform.GetComponent<UnityEngine.UI.Image>();
        if (cardImage == null) return;
        
        // Build path
        string path = $"Textures/Cards/{rarity}x/{cardId}";
        
        // Try single sprite first
        Sprite sprite = Resources.Load<Sprite>(path);
        
        // If null, try LoadAll (for Multiple sprite mode textures)
        if (sprite == null)
        {
            Sprite[] sprites = Resources.LoadAll<Sprite>(path);
            if (sprites != null && sprites.Length > 0)
            {
                sprite = sprites[0];
            }
        }
        
        if (sprite != null)
        {
            cardImage.sprite = sprite;
            cardImage.color = Color.white;
            Debug.Log($"[Card] Loaded card image: {path}");
        }
        else
        {
            // Fallback
            Sprite fallback = Resources.Load<Sprite>("Cards/bg_card_test");
            if (fallback != null)
            {
                cardImage.sprite = fallback;
                cardImage.color = Color.white;
                Debug.Log($"[Card] Using fallback image for: {cardId}");
            }
        }
    }

    private void Awake()
    {
        // Auto-bind UI elements if missing (Fix for Prefabs with disconnected Card component)
        if (nameText == null) nameText = FindUI<TextMeshProUGUI>("NameText");
        if (powerText == null) powerText = FindUI<TextMeshProUGUI>("PowerText");
        if (healText == null) healText = FindUI<TextMeshProUGUI>("HealthText"); // Primary uses HealthText
        if (healText == null) healText = FindUI<TextMeshProUGUI>("HealText");   // Support might use HealText
        if (abilityText == null) abilityText = FindUI<TextMeshProUGUI>("AbilityText");
        if (chargeText == null) chargeText = FindUI<TextMeshProUGUI>("ChargeText");
        if (costText == null) costText = FindUI<TextMeshProUGUI>("CostText");
    }

    private T FindUI<T>(string name) where T : Component
    {
        // Try to find under Content first (Standard structure)
        Transform content = transform.Find("Content");
        if (content != null)
        {
            Transform target = content.Find(name);
            if (target != null) return target.GetComponent<T>();
        }

        // Fallback to recursive search
        var components = GetComponentsInChildren<T>(true);
        foreach (var c in components)
        {
             if (c.gameObject.name == name) return c;
        }
        return null;
    }

    private void UpdateUI()
    {
        if (nameText != null)
        {
            nameText.text = Name;
        }

        if (powerText != null)
        {
            powerText.text = $"P: {Power}";
        }

        if (costText != null)
        {
            costText.text = $"{Cost}";
        }

        if (healText != null)
        {
            // If it's a Primary card, show current health only
            if (Type == CardType.Primary)
            {
                 healText.text = $"{CurrentHealth}";
            }
            else
            {
                 healText.text = $"H: {Heal}";
            }
        }

        if (abilityText != null)
        {
            abilityText.text = Ability?.Description ?? "";
        }

        if (chargeText != null) // Optional
        {
            chargeText.text = $"{Charge}";
        }
    }

    // Status Effects
    public System.Collections.Generic.List<Game.Battle.StatusEffects.StatusEffect> StatusEffects { get; private set; } = new System.Collections.Generic.List<Game.Battle.StatusEffects.StatusEffect>();
    
    // Logic
    public int TurnsInHand { get; set; } = 0;

    public void AddStatus(Game.Battle.StatusEffects.StatusEffect effect)
    {
        if (effect == null) return;
        
        var instance = effect.Clone();
        instance.Initialize(this);
        StatusEffects.Add(instance);
        UpdateUI(); // Show status icon?
        Debug.Log($"{Name} gained status: {effect.DisplayName}");
    }

    public void RemoveStatus(Game.Battle.StatusEffects.StatusEffect effect)
    {
        if (StatusEffects.Contains(effect))
        {
            StatusEffects.Remove(effect);
            UpdateUI();
        }
    }
    
    public void OnTurnStart()
    {
        // Iterate backwards to allow removal
        for (int i = StatusEffects.Count - 1; i >= 0; i--)
        {
            StatusEffects[i].OnTurnStart();
        }
    }
    
    public void OnTurnEnd()
    {
        for (int i = StatusEffects.Count - 1; i >= 0; i--)
        {
            StatusEffects[i].OnTurnEnd();
        }
    }

    public void TakeDamage(int damage)
    {
        if (IsDead) return;
        
        // If this is a PrimaryCard, delegate to its TakeDamage method
        var primaryCard = GetComponent<PrimaryCard>();
        if (primaryCard != null)
        {
            primaryCard.TakeDamage(damage);
            return;
        }

        // Hook: OnTakeDamage
        for (int i = StatusEffects.Count - 1; i >= 0; i--)
        {
            StatusEffects[i].OnTakeDamage(ref damage);
        }

        CurrentHealth -= damage;
        if (CurrentHealth < 0) CurrentHealth = 0;
        
        UpdateUI();
        OnHealthChanged?.Invoke(CurrentHealth);

        if (IsDead)
        {
            Debug.Log($"{Name} has been defeated!");
            // Handle death logic via events or manager
        }
    }
    
    // Hook for dealing damage (called by CardAction/Ability)
    public int CalculateDamage(int baseDamage)
    {
        int finalDamage = baseDamage;
         for (int i = StatusEffects.Count - 1; i >= 0; i--)
        {
            StatusEffects[i].OnDealDamage(ref finalDamage);
        }
        return finalDamage;
    }

    public void RecoverHealth(int amount)
    {
        if (IsDead) return; // Usually dead cards don't heal, but game rules might vary

        CurrentHealth += amount;
        if (CurrentHealth > MaxHealth) CurrentHealth = MaxHealth;

        UpdateUI();
        OnHealthChanged?.Invoke(CurrentHealth);
    }

    #region Drag Methods
    /// <summary>
    /// ドラッグを開始
    /// </summary>
    public void StartDrag()
    {
        if (!isDraggable) return;

        IsDragging = true;
        OnDragStart?.Invoke();
    }

    /// <summary>
    /// ドラッグを終了
    /// </summary>
    public void EndDrag()
    {
        IsDragging = false;
        OnDragEnd?.Invoke();
    }

    /// <summary>
    /// ドラッグを有効化
    /// </summary>
    public void EnableDrag()
    {
        IsDraggable = true;
    }

    /// <summary>
    /// ドラッグを無効化
    /// </summary>
    public void DisableDrag()
    {
        IsDraggable = false;
        if (IsDragging)
        {
            EndDrag();
        }
    }
    #endregion

    /// <summary>
    /// アビリティを発動する
    /// </summary>
    public void UseAbility(Card target = null)
    {
        if (Ability != null)
        {
            Ability.Activate(this, target);
        }
        else
        {
            Debug.Log($"{Name} has no ability.");
        }
    }
}
