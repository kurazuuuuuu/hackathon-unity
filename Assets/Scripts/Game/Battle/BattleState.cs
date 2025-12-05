namespace Game.Battle
{
    /// <summary>
    /// バトルの状態を表す列挙型
    /// </summary>
    public enum BattleState
    {
        NotStarted,       // バトル未開始
        DeterminingOrder, // 先攻決定中
        PlayerTurn,       // プレイヤーのターン
        OpponentTurn,     // 相手のターン
        BattleEnd         // バトル終了
    }
}
