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
    [SerializeField] private TextMeshProUGUI abilityText;
    [SerializeField] private TextMeshProUGUI chargeText;

    [Header("Drag Settings")]
    [SerializeField] private bool isDraggable = true;

    // カードの基本情報
    public string Name { get; private set; }
    public CardType Type { get; private set; }
    public int Power { get; private set; }
    public int Heal { get; private set; }
    public CardAbility Ability { get; private set; }
    public int Charge { get; private set; }
    public int Cost { get; private set; }

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

        UpdateUI();
    }

    /// <summary>
    /// CardDataを使用してカードを初期化する（ガチャ結果表示用）
    /// レアリティに応じた見た目も適用する
    /// </summary>
    public void InitializeForGacha(CardData data)
    {
        Initialize(data);
        
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
        
        var visualizer = GetComponent<Game.UI.CardVisualizer>();
        if (visualizer == null)
        {
            visualizer = gameObject.AddComponent<Game.UI.CardVisualizer>();
        }
        visualizer.ApplyRarityStyle(data.Rarity);
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

        if (healText != null)
        {
            healText.text = $"H: {Heal}";
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
