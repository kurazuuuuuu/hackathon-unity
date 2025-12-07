using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Data;

namespace Game.Battle
{
    /// <summary>
    /// プレイヤーの状態を管理するクラス
    /// </summary>
    [Serializable]
    public class Player
    {
        public const int MAX_HP = 20;
        public const int SKIP_HEAL_MIN = 3;
        public const int SKIP_HEAL_MAX = 5;

        [Header("Player Info")]
        public string Name;
        public string PlayerId;

        [Header("HP")]
        public int MaxHP = MAX_HP;
        public int CurrentHP;

        [Header("Qualification")]
        public int OwnedQualificationCount; // 実際に所有している資格の数

        [Header("Deck")]
        public DeckData Deck;
        public List<Card> Hand = new List<Card>();

        // 場に出ているカード
        public List<Card> PrimaryCardsInPlay = new List<Card>();

        // 山札 (カードIDのリスト)
        public List<string> DrawPile = new List<string>();

        // 状態
        public bool IsDefeated => CurrentHP <= 0;
        public bool IsMyTurn { get; set; }

        public Player()
        {
            CurrentHP = MAX_HP;
        }

        public Player(string name, int qualificationCount)
        {
            Name = name;
            OwnedQualificationCount = qualificationCount;
            CurrentHP = MAX_HP;
            MaxHP = MAX_HP;
        }

        [Header("Zones")]
        [SerializeField] private PrimaryCardZone primaryZone;

        public void SetPrimaryZone(PrimaryCardZone zone)
        {
            primaryZone = zone;
        }

        /// <summary>
        /// デッキを初期化（主力カード配置、山札作成）
        /// </summary>
        public void InitializeDeck(CardManager cardManager)
        {
            if (Deck == null)
            {
                // デバッグ用：デッキがない場合はランダムに作成
                CreateDebugDeck(cardManager);
            }

            Debug.Log($"{Name} のデッキ初期化: 主力 {Deck.PrimaryCards.Count} 枚, サポート {Deck.SupportCards.Count} 枚");

            // 主力カードを場に配置
            // PrimaryCardZoneがある場合はそちらを使用
            if (primaryZone != null)
            {
                primaryZone.Clear();
                foreach (var cardId in Deck.PrimaryCards)
                {
                    bool success = primaryZone.PlaceCardById(cardId);
                    if (!success)
                    {
                        Debug.LogWarning($"{Name}: 主力カード {cardId} の配置に失敗しました");
                    }
                    else 
                    {
                         // Zone内で生成されたカードを取得してPrimaryCardsInPlayに追加する処理が必要だが
                         // 現状のPrimaryCardZoneは内部でリストを持っているので、
                         // Player側で持つ必要性は薄れるかもしれない。
                         // いったんリンクだけしておく、あるいはPrimaryCardZoneから取得できるようにする
                    }
                }
                
                // PrimaryCardsInPlayを更新
                PrimaryCardsInPlay.Clear();
                PrimaryCardsInPlay.AddRange(primaryZone.PlacedCards);
            }
            else
            {
                // 旧ロジック（バックアップ）
                foreach (var cardId in Deck.PrimaryCards)
                {
                    var card = cardManager.SpawnCard(cardId);
                    if (card != null)
                    {
                        PrimaryCardsInPlay.Add(card);
                    }
                }
            }

            // サポート・特殊カードを山札に追加してシャッフル
            DrawPile.Clear();
            DrawPile.AddRange(Deck.SupportCards);
            ShuffleDrawPile();

            Debug.Log($"{Name} の山札作成完了: {DrawPile.Count} 枚");
        }

        /// <summary>
        /// デバッグ用デッキ作成
        /// </summary>
        private void CreateDebugDeck(CardManager cardManager)
        {
            Deck = new DeckData("Debug Deck");
            
            // 主力カードを追加
            for (int i = 0; i < DeckData.PRIMARY_CARD_COUNT; i++)
            {
                var cardData = cardManager.GetRandomPrimaryCard();
                if (cardData != null) 
                {
                    Deck.AddPrimaryCard(cardData.CardId);
                }
                else
                {
                    Debug.LogError("デバッグデッキ構築エラー: 主力カードが不足しています");
                }
            }
            
            // サポート・特殊カードを追加
            for (int i = 0; i < DeckData.SUPPORT_CARD_COUNT; i++)
            {
                var cardData = cardManager.GetRandomSupportCard();
                if (cardData != null)
                {
                    Deck.AddSupportCard(cardData.CardId);
                }
                 else
                {
                    Debug.LogError("デバッグデッキ構築エラー: サポートカードが不足しています");
                }
            }
        }

        /// <summary>
        /// 山札をシャッフル
        /// </summary>
        private void ShuffleDrawPile()
        {
            for (int i = 0; i < DrawPile.Count; i++)
            {
                string temp = DrawPile[i];
                int randomIndex = UnityEngine.Random.Range(i, DrawPile.Count);
                DrawPile[i] = DrawPile[randomIndex];
                DrawPile[randomIndex] = temp;
            }
        }

        /// <summary>
        /// カードを引く
        /// </summary>
        public void DrawCard(CardManager cardManager)
        {
            if (DrawPile.Count == 0)
            {
                Debug.Log($"{Name} の山札がありません");
                return;
            }

            string cardId = DrawPile[0];
            DrawPile.RemoveAt(0);

            var card = cardManager.SpawnCard(cardId);
            if (card != null)
            {
                Hand.Add(card);
                Debug.Log($"{Name} がカードを引きました: {card.Name}");
            }
        }

        /// <summary>
        /// 体力を消費
        /// </summary>
        public void ConsumeHP(int amount)
        {
            CurrentHP = Mathf.Max(0, CurrentHP - amount);
            Debug.Log($"{Name} の体力: {CurrentHP + amount} -> {CurrentHP} (-{amount})");
        }

        /// <summary>
        /// 体力を回復
        /// </summary>
        public void HealHP(int amount)
        {
            int before = CurrentHP;
            CurrentHP = Mathf.Min(MaxHP, CurrentHP + amount);
            Debug.Log($"{Name} の体力: {before} -> {CurrentHP} (+{amount})");
        }

        /// <summary>
        /// スキップ時の回復（3~5ランダム）
        /// </summary>
        public int HealOnSkip()
        {
            int healAmount = UnityEngine.Random.Range(SKIP_HEAL_MIN, SKIP_HEAL_MAX + 1);
            HealHP(healAmount);
            return healAmount;
        }

        /// <summary>
        /// カードを使用（コスト消費）
        /// </summary>
        public bool CanPlayCard(Card card)
        {
            if (card == null) return false;
            
            // コストが体力以下なら使用可能
            if (CurrentHP < card.Cost)
            {
                return false;
            }
            return true;
        }
    }
}
