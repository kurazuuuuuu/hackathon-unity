using System;
using System.Collections;
using UnityEngine;
using Game;

namespace Game.Battle
{
    /// <summary>
    /// バトル全体を管理するマネージャー
    /// </summary>
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        [Header("VFX Settings")]
        [SerializeField] public GameObject AttackVFX;
        [SerializeField] public GameObject DamageVFX;
        [SerializeField] public GameObject SkillActivateVFX;
        [SerializeField] public GameObject SkillHitVFX;

        [Header("State")]
        [SerializeField] private BattleState currentState = BattleState.NotStarted;
        
        [Header("Timing")]
        [SerializeField] private float turnTransitionDelay = 1.5f; // ターン切り替えのディレイ（秒）

        // プレイヤー
        private Player player1;
        private Player player2;
        private Player currentPlayer;
        private Player winner;

        // 依存関係
        private CardManager cardManager;

        [Header("UI References")]
        [SerializeField] private Transform p1HandArea;
        [SerializeField] private Transform p2HandArea;
        [SerializeField] private PrimaryCardZone p1PrimaryZone;
        [SerializeField] private PrimaryCardZone p2PrimaryZone;
        
        // Use Setter for Builder script interaction
        public void SetUIReferences(Transform h1, Transform h2, PrimaryCardZone z1, PrimaryCardZone z2)
        {
            p1HandArea = h1;
            p2HandArea = h2;
            p1PrimaryZone = z1;
            p2PrimaryZone = z2;
        }

        // プロパティ
        public BattleState CurrentState => currentState;
        public Player Player1 => player1;
        public Player Player2 => player2;
        public Player CurrentPlayer => currentPlayer;
        public Player Winner => winner;

        // イベント
        public event Action OnBattleStart;
        public event Action<Player> OnTurnStart;
        public event Action<Player> OnTurnEnd;
        public event Action<Player> OnBattleEnd;
        public event Action<Player, int> OnPlayerDamaged;
        public event Action<Player, int> OnPlayerHealed;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            cardManager = FindAnyObjectByType<CardManager>();
            if (cardManager == null)
            {
                Debug.LogError("CardManager not found in the scene!");
            }
            else
            {
                Debug.Log("BattleManager: CardManager found.");
            }
        }

        /// <summary>
        /// バトルを開始
        /// </summary>
        public void StartBattle(Player p1, Player p2)
        {
            Debug.Log("BattleManager: StartBattle called.");
            player1 = p1;
            player2 = p2;
            
            // Assign UI
            if (p1HandArea != null) player1.SetUI(p1HandArea, p1PrimaryZone);
            if (p2HandArea != null) player2.SetUI(p2HandArea, p2PrimaryZone);

            // デッキ初期化
            if (cardManager != null)
            {
                Debug.Log("BattleManager: Initializing decks...");
                player1.InitializeDeck(cardManager);
                player2.InitializeDeck(cardManager);
            }
            else
            {
                Debug.LogError("BattleManager: cardManager is null in StartBattle!");
            }

            currentState = BattleState.DeterminingOrder;
            DetermineFirstPlayer();

            currentState = BattleState.PlayerTurn;
            
            // Draw 5 cards initially for both players
            for (int i = 0; i < 5; i++)
            {
                player1.DrawCard(cardManager);
                player2.DrawCard(cardManager);
            }
            
            OnBattleStart?.Invoke();
            StartTurn();
        }

        /// <summary>
        /// 先攻を決定
        /// Bot対戦の場合はプレイヤー（Player1）が確定先攻
        /// </summary>
        private void DetermineFirstPlayer()
        {
            // Bot対戦の場合はPlayer1が先攻
            if (player2.IsBot)
            {
                currentPlayer = player1;
                Debug.Log($"{currentPlayer.Name} が先攻（Bot対戦のため）");
            }
            else
            {
                // PvPの場合はランダム
                currentPlayer = UnityEngine.Random.Range(0, 2) == 0 ? player1 : player2;
                Debug.Log($"{currentPlayer.Name} が先攻（ランダム）");
            }

            currentPlayer.IsMyTurn = true;
            GetOpponent().IsMyTurn = false;
        }

        /// <summary>
        /// ターン開始
        /// </summary>
        private void StartTurn()
        {
            Debug.Log($"=== {currentPlayer.Name} のターン ===");
            
            // ドロー
            if (cardManager != null)
            {
                currentPlayer.DrawCard(cardManager);
            }

            OnTurnStart?.Invoke(currentPlayer);
        }

        /// <summary>
        /// カードを使用
        /// </summary>
        public void PlayCard(Card card, Card target = null)
        {
            if (currentState != BattleState.PlayerTurn || currentPlayer == null)
            {
                Debug.LogWarning("現在はプレイヤーターンではありません、またはプレイヤーが初期化されていません。");
                return;
            }

            CardAction.PlayCard(currentPlayer, card, target);
            
            // 敗北チェック
            if (CheckDefeat())
            {
                EndBattle();
                return;
            }

            // Action Cost Check
            // Default Action Cost is 1. If 0, do not end turn.
            if (card.ActionCost > 0)
            {
                EndTurn();
            }
        }

        /// <summary>
        /// ターンをスキップ
        /// </summary>
        public void SkipTurn()
        {
            if (currentState != BattleState.PlayerTurn || currentPlayer == null)
            {
                Debug.LogWarning("現在はプレイヤーターンではありません、またはプレイヤーが初期化されていません。");
                return;
            }

            int healAmount = currentPlayer.HealOnSkip();
            OnPlayerHealed?.Invoke(currentPlayer, healAmount);
            
            // 敗北チェック（毒ダメージなどを考慮する場合ここでもチェック）
            if (CheckDefeat())
            {
                EndBattle();
                return;
            }
            
            EndTurn();
        }

        /// <summary>
        /// ターン終了
        /// </summary>
        private void EndTurn()
        {
            OnTurnEnd?.Invoke(currentPlayer);
            
            // プレイヤー交代
            currentPlayer.IsMyTurn = false;
            currentPlayer = GetOpponent();
            currentPlayer.IsMyTurn = true;

            // ディレイ付きでターン開始
            StartCoroutine(StartTurnWithDelay());
        }
        
        private IEnumerator StartTurnWithDelay()
        {
            yield return new WaitForSeconds(turnTransitionDelay);
            
            // バトルが終了していたらターン開始しない
            if (currentState == BattleState.BattleEnd)
            {
                Debug.Log("[BattleManager] バトル終了済みのためターン開始をスキップ");
                yield break;
            }
            
            StartTurn();
        }

        /// <summary>
        /// 敗北チェック
        /// </summary>
        private bool CheckDefeat()
        {
            if (player1.IsDefeated)
            {
                winner = player2;
                return true;
            }
            if (player2.IsDefeated)
            {
                winner = player1;
                return true;
            }
            return false;
        }

        /// <summary>
        /// バトル終了
        /// </summary>
        private void EndBattle()
        {
            // すべてのコルーチンを停止
            StopAllCoroutines();
            
            currentState = BattleState.BattleEnd;
            Debug.Log($"=== バトル終了 ===\n勝者: {winner.Name}");
            
            // イベント発火（リザルト表示用）
            OnBattleEnd?.Invoke(winner);
        }

        /// <summary>
        /// 相手プレイヤーを取得
        /// </summary>
        public Player GetOpponent()
        {
            return currentPlayer == player1 ? player2 : player1;
        }
    }
}
