using UnityEngine;

namespace Game.Battle
{
    /// <summary>
    /// カードの使用処理を管理する静的クラス
    /// </summary>
    public static class CardAction
    {
        /// <summary>
        /// カードを使用（タイプに応じて処理を分岐）
        /// </summary>
        /// <summary>
        /// カードを使用（タイプに応じて処理を分岐）
        /// </summary>
        public static void PlayCard(Player player, Card card, Card target = null)
        {
            if (card == null)
            {
                Debug.LogError("カードがnullです");
                return;
            }

            // コストチェック
            var cardBase = card.GetComponent<CardBase>();
            if (cardBase == null || !player.CanPlayCard(cardBase))
            {
                Debug.LogWarning($"{player.Name} は体力が足りません（必要: {card.Cost}, 現在: {player.CurrentHP}）");
                return;
            }

            switch (card.Type)
            {
                case CardType.Primary:
                    // 主力カードは最初から場にあるため、手札からプレイすることはないはず
                    // もしプレイされた場合は何もしないか、警告を出す
                    Debug.LogWarning("主力カードは手札からプレイできません");
                    break;
                case CardType.Support:
                    if (target == null)
                    {
                        Debug.LogWarning("サポートカードには対象が必要です");
                        return;
                    }
                    if (target.Type != CardType.Primary)
                    {
                        Debug.LogWarning("サポートカードは主力カードに対してのみ使用できます");
                        return;
                    }
                    PlaySupportCard(player, card, target);
                    break;
                case CardType.Special:
                    PlaySpecialCard(player, card);
                    break;
            }
        }

        /// <summary>
        /// サポートカードを主力カードに使用
        /// </summary>
        public static void PlaySupportCard(Player player, Card support, Card target)
        {
            // コストを消費
            player.ConsumeHP(support.Cost);

            // 対象の主力カードに効果を付与
            if (support.Ability != null)
            {
                 var context = new Game.Abilities.BattleContext(support.GetComponent<CardBase>(), target.GetComponent<CardBase>(), player, BattleManager.Instance);
                 support.Ability.Activate(context);
            }
            else
            {
                 // 旧・またはアビリティなし
                 Debug.Log($"{support.Name} has no ability.");
                 support.UseAbility(target); // Fallback to old method if any logic remains there
            }

            // 手札から削除
            var supportBase = support.GetComponent<CardBase>();
            if (supportBase != null)
                player.Hand.Remove(supportBase);
            // 墓地へ送るなどの処理が必要ならここに追加

            Debug.Log($"{player.Name} がサポートカード [{support.Name}] を [{target.Name}] に使用（コスト: {support.Cost}）");
        }

        /// <summary>
        /// 特殊カードを使用
        /// </summary>
        public static void PlaySpecialCard(Player player, Card card)
        {
            // コストを消費
            player.ConsumeHP(card.Cost);

            // 自分自身または全体に効果
            if (card.Ability != null)
            {
                 var context = new Game.Abilities.BattleContext(card.GetComponent<CardBase>(), null, player, BattleManager.Instance);
                 card.Ability.Activate(context);
            }
            else
            {
                 card.UseAbility();
            }

            // 手札から削除
            var cardBase2 = card.GetComponent<CardBase>();
            if (cardBase2 != null)
                player.Hand.Remove(cardBase2);

            Debug.Log($"{player.Name} が特殊カード [{card.Name}] を使用（コスト: {card.Cost}）");
        }
    }
}
