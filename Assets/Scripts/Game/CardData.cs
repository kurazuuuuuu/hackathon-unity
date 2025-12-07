using UnityEngine;
using Game.Abilities;

namespace Game
{
    /// <summary>
    /// カードの基本データを管理するScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "NewCardData", menuName = "Game/Card Data")]
    public class CardData : ScriptableObject
    {
        [Header("Card ID")] // カードID (5x: 主力, 4x/3x: サポート, 3F~: 特殊)
        [SerializeField] private string cardId;

        [Header("Card Type")] //カードタイプ
        [SerializeField] private CardType cardType;

        [Header("Basic Info")] //基礎ステータス
        [SerializeField] private string cardName;
        [SerializeField] private int power; //攻撃力
        [SerializeField] private int heal; //回復力

        [Header("Ability")] //特殊効果
        [SerializeField] private CardAbility ability;

        [Header("Charge")] //凸数
        [SerializeField] private int charge;

        [Header("Cost")] //使用時の体力消費量
        [SerializeField] private int cost;

        [Header("Gacha Info")] // ガチャ関連
        [SerializeField] private int rarity; // レアリティ (3, 4, 5)

        public string CardId => !string.IsNullOrEmpty(cardId) ? cardId : name;
        public string CardName => cardName;
        public CardType CardType => cardType;
        public int Power => power;
        public int Heal => heal;
        public CardAbility Ability => ability;
        public int Charge => charge;
        public int Cost => cost;
        public int Rarity => rarity;
    }
}
