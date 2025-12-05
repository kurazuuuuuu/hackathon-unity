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

    // カードの基本情報
    public string Name { get; private set; }
    public int Power { get; private set; }
    public int Heal { get; private set; }
    public CardAbility Ability { get; private set; }
    public int Charge { get; private set; }

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
        this.Power = data.Power;
        this.Heal = data.Heal;
        this.Ability = data.Ability;
        this.Charge = data.Charge;

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
            abilityText.text = Ability.Description;
        }

        if (chargeText != null) // Optional
        {
            chargeText.text = $"{Charge}";
        }
    }

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
