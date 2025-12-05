using System;
using UnityEngine;

namespace Game.Battle
{
    /// <summary>
    /// バトル全体を管理するマネージャー
    /// </summary>
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        [Header("Battle State")]
        [SerializeField] private BattleState currentState = BattleState.NotStarted;

        // プレイヤー
        private Player player1;
        private Player player2;
        private Player currentPlayer;
        private Player winner;

        // 依存関係
        private CardManager cardManager;

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
            OnBattleStart?.Invoke();
            StartTurn();
        }

        /// <summary>
        /// 先攻を決定（資格数が少ない方が先攻）
        /// </summary>
        private void DetermineFirstPlayer()
        {
            if (player1.OwnedQualificationCount < player2.OwnedQualificationCount)
            {
                currentPlayer = player1;
                Debug.Log($"{player1.Name} が先攻（資格数: {player1.OwnedQualificationCount} < {player2.OwnedQualificationCount}）");
            }
            else if (player1.OwnedQualificationCount > player2.OwnedQualificationCount)
            {
                currentPlayer = player2;
                Debug.Log($"{player2.Name} が先攻（資格数: {player2.OwnedQualificationCount} < {player1.OwnedQualificationCount}）");
            }
            else
            {
                // 同数の場合はランダム
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

            EndTurn();
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
            currentState = BattleState.BattleEnd;
            Debug.Log($"=== バトル終了 ===\n勝者: {winner.Name}");
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
