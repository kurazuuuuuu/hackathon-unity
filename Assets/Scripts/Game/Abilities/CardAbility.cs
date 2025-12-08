using UnityEngine;
using Game.Battle;

namespace Game.Abilities
{
    /// <summary>
    /// 能力発動時のコンテキスト情報
    /// </summary>
    public class BattleContext
    {
        public CardBase SourceCard;
        public CardBase TargetCard;
        public Player SourcePlayer;
        public Player TargetPlayer;
        public BattleManager BattleManager;

        public BattleContext(CardBase source, CardBase target, Player sourcePlayer, BattleManager battleManager)
        {
            SourceCard = source;
            TargetCard = target;
            SourcePlayer = sourcePlayer;
            BattleManager = battleManager;
            
            // ターゲットプレイヤーの解決（基本は相手）
            if (battleManager != null)
            {
                TargetPlayer = battleManager.GetOpponent(); // Context作成時にSourcePlayerの対戦相手を取得
                // ※ もしSourcePlayerがBattleManagerの管理外ならnullになる可能性があるため注意
                if (TargetPlayer == SourcePlayer) // GetOpponentがCurrentPlayer依存の場合のケア
                {
                     // SourcePlayerがP1ならP2、P2ならP1
                     TargetPlayer = (SourcePlayer == battleManager.Player1) ? battleManager.Player2 : battleManager.Player1;
                }
            }
        }
    }

    /// <summary>
    /// カードの能力を表す基底クラス
    /// </summary>
    public abstract class CardAbility : ScriptableObject
    {
        [TextArea]
        [SerializeField] private string description;

        public string Description => description;

        /// <summary>
        /// 能力を発動する
        /// </summary>
        public abstract void Activate(BattleContext context);
        
        /// <summary>
        /// 旧APIとの互換性用（削除予定）
        /// </summary>
        public virtual void Activate(Card user, Card target = null)
        {
            // BattleManagerの参照が取れないため、完全な動作は保証しない
            Debug.LogWarning("Deprecated Activate called. Please use Activate(BattleContext).");
        }
    }
}
