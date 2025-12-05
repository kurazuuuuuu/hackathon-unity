namespace Game
{
    /// <summary>
    /// カードの種類を表す列挙型
    /// </summary>
    public enum CardType
    {
        Primary,  // 主力資格カード (☆5: 5A〜5E)
        Support,  // サポート資格カード (☆3〜4: 3A〜3E, 4A〜4F)
        Special   // 特殊カード (☆3: 3F〜3P) ※所持追加効果なし
    }
}
