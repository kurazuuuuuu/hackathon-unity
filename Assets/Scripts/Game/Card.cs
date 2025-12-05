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
    public void UseAbility()
    {
        if (Ability != null)
        {
            Ability.Activate(this);
        }
        else
        {
            Debug.Log($"{Name} has no ability.");
        }
    }
}
