using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Battle
{
    /// <summary>
    /// シンプルなBot AI
    /// ターン開始時にランダムにカードをプレイするか、スキップする
    /// </summary>
    public class BotAI : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float thinkingDelay = 1.5f; // AIの「考え中」演出用ディレイ

        private BattleManager battleManager;
        private CardManager cardManager;
        private Coroutine currentThinkingCoroutine;

        private void Start()
        {
            battleManager = BattleManager.Instance;
            cardManager = FindAnyObjectByType<CardManager>();

            if (battleManager != null)
            {
                battleManager.OnTurnStart += OnTurnStart;
            }
        }

        private void OnDestroy()
        {
            if (battleManager != null)
            {
                battleManager.OnTurnStart -= OnTurnStart;
            }
        }

        private void OnTurnStart(Player player)
        {
            // Botプレイヤーのターンかチェック
            if (player == null || !player.IsBot) return;

            Debug.Log($"[BotAI] {player.Name} のターン開始 - AI思考中...");

            // 既存のコルーチンを停止
            if (currentThinkingCoroutine != null)
            {
                StopCoroutine(currentThinkingCoroutine);
            }

            currentThinkingCoroutine = StartCoroutine(ThinkAndAct(player));
        }

        private IEnumerator ThinkAndAct(Player player)
        {
            // 演出用の待機
            yield return new WaitForSeconds(thinkingDelay);

            // バトルが終了していないかチェック
            if (battleManager.CurrentState != BattleState.PlayerTurn)
            {
                Debug.Log("[BotAI] バトルが終了しているためアクションをスキップ");
                yield break;
            }

            // 手札からプレイ可能なカードを探す
            CardBase cardToPlay = null;
            CardBase targetCard = null;

            foreach (var cardBase in player.Hand)
            {
                if (cardBase == null) continue;

                // コスト確認
                if (!player.CanPlayCard(cardBase)) continue;

                var card = cardBase.GetComponent<Card>();
                if (card == null) continue;

                // 主力カードは手札からプレイ不可
                if (card.Type == CardType.Primary) continue;

                // サポートカードの場合、ターゲットを選択
                if (card.Type == CardType.Support)
                {
                    // 自分の主力カードからランダムに選択
                    var targets = GetValidTargets(player);
                    if (targets.Count > 0)
                    {
                        cardToPlay = cardBase;
                        targetCard = targets[Random.Range(0, targets.Count)];
                        break;
                    }
                }
                else if (card.Type == CardType.Special)
                {
                    // 特殊カードはターゲット不要
                    cardToPlay = cardBase;
                    break;
                }
            }

            // カードをプレイするかスキップするか決定
            if (cardToPlay != null)
            {
                var card = cardToPlay.GetComponent<Card>();
                Card target = targetCard?.GetComponent<Card>();

                Debug.Log($"[BotAI] {player.Name} がカード [{card.Name}] をプレイ");
                battleManager.PlayCard(card, target);
            }
            else
            {
                // 手札にプレイ可能なカードがない場合、主力カードで攻撃を試みる
                bool attacked = TryAttackWithPrimaryCard(player);
                
                if (!attacked)
                {
                    // 攻撃もできない場合はスキップ
                    Debug.Log($"[BotAI] {player.Name} はターンをスキップ");
                    battleManager.SkipTurn();
                }
            }

            currentThinkingCoroutine = null;
        }
        
        /// <summary>
        /// 主力カードで相手の主力カードを攻撃
        /// </summary>
        private bool TryAttackWithPrimaryCard(Player player)
        {
            // 自分の生存している主力カードを取得
            var myPrimaryCards = GetValidTargets(player);
            if (myPrimaryCards.Count == 0)
            {
                Debug.Log("[BotAI] 攻撃可能な主力カードがありません");
                return false;
            }
            
            // 相手の生存している主力カードを取得
            var opponent = battleManager.GetOpponent();
            var enemyPrimaryCards = GetValidTargets(opponent);
            if (enemyPrimaryCards.Count == 0)
            {
                Debug.Log("[BotAI] 攻撃対象の敵主力カードがありません");
                return false;
            }
            
            // ランダムに攻撃元と攻撃対象を選択
            var attacker = myPrimaryCards[Random.Range(0, myPrimaryCards.Count)];
            var target = enemyPrimaryCards[Random.Range(0, enemyPrimaryCards.Count)];
            
            Debug.Log($"[BotAI] {player.Name} の [{attacker.Name}] が [{target.Name}] を攻撃！");
            
            // 攻撃を実行
            var attackerPrimary = attacker as PrimaryCard;
            var targetPrimary = target as PrimaryCard;
            
            if (attackerPrimary != null && targetPrimary != null)
            {
                int damage = attackerPrimary.Power;
                targetPrimary.TakeDamage(damage);
                Debug.Log($"[BotAI] {target.Name} に {damage} ダメージ！");
                
                // ターン終了
                battleManager.SkipTurn();
                return true;
            }
            
            
            return false;
        }

        /// <summary>
        /// サポートカードのターゲットとして有効な主力カードを取得
        /// </summary>
        private List<CardBase> GetValidTargets(Player player)
        {
            var targets = new List<CardBase>();

            foreach (var card in player.PrimaryCardsInPlay)
            {
                var primaryCard = card as PrimaryCard;
                // 生存している主力カードのみ
                if (primaryCard != null && !primaryCard.IsDead)
                {
                    targets.Add(card);
                }
            }

            return targets;
        }
    }
}
