using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Battle;
using Game.System;
using Game.Data;

namespace Game.Scenes
{
    /// <summary>
    /// バトル画面
    /// </summary>
    public class BattleScene : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private CardManager cardManager;

        [Header("Player UI")]
        [SerializeField] private TextMeshProUGUI player1NameText;
        [SerializeField] private TextMeshProUGUI player1HPText;
        [SerializeField] private TextMeshProUGUI player2NameText;
        [SerializeField] private TextMeshProUGUI player2HPText;

        [Header("Battle UI")]
        [SerializeField] private TextMeshProUGUI turnText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button skipButton;
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private Button returnButton;

        [Header("Debug")]
        [SerializeField] private bool useDebugPlayers = true;
        [SerializeField] private List<CardData> debugPrimaryCardsP1 = new List<CardData>(); // P1の主力カード手動設定

        private void Awake()
        {
            // SerializeFieldが未設定の場合、名前でUI要素を自動取得
            AutoBindUIElements();
        }

        /// <summary>
        /// UI要素を名前で自動バインド
        /// </summary>
        private void AutoBindUIElements()
        {
            // Canvas内のすべてのTextMeshProUGUIを取得（非アクティブ含む）
            var allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var text in allTexts)
            {
                switch (text.gameObject.name)
                {
                    case "Player1Name":
                        if (player1NameText == null) player1NameText = text;
                        break;
                    case "Player1HP":
                        if (player1HPText == null) player1HPText = text;
                        break;
                    case "Player2Name":
                        if (player2NameText == null) player2NameText = text;
                        break;
                    case "Player2HP":
                        if (player2HPText == null) player2HPText = text;
                        break;
                    case "TurnText":
                        if (turnText == null) turnText = text;
                        break;
                    case "MessageText":
                        if (messageText == null) messageText = text;
                        break;
                    case "ResultText":
                        if (resultText == null) resultText = text;
                        break;
                }
            }

            // ボタンを名前で取得
            var allButtons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var button in allButtons)
            {
                switch (button.gameObject.name)
                {
                    case "SkipButton":
                        if (skipButton == null) skipButton = button;
                        break;
                    case "ReturnButton":
                        if (returnButton == null) returnButton = button;
                        break;
                }
            }

            // ResultPanelを取得
            if (resultPanel == null)
            {
                var panel = GameObject.Find("ResultPanel");
                if (panel == null)
                {
                    // 非アクティブの場合、親から検索
                    var canvas = FindAnyObjectByType<Canvas>();
                    if (canvas != null)
                    {
                        var panelTransform = canvas.transform.Find("ResultPanel");
                        if (panelTransform != null)
                        {
                            resultPanel = panelTransform.gameObject;
                        }
                    }
                }
                else
                {
                    resultPanel = panel;
                }
            }
        }

        private void Start()
        {
            // ボタン設定
            skipButton?.onClick.AddListener(OnSkipButtonClicked);
            returnButton?.onClick.AddListener(OnReturnButtonClicked);

            // バトルマネージャー取得
            if (battleManager == null)
            {
                battleManager = FindAnyObjectByType<BattleManager>();
            }

            // イベント購読
            if (battleManager != null)
            {
                battleManager.OnBattleStart += OnBattleStart;
                battleManager.OnTurnStart += OnTurnStart;
                battleManager.OnBattleEnd += OnBattleEnd;
            }

            // 結果パネルを非表示
            if (resultPanel != null)
            {
                resultPanel.SetActive(false);
            }

            // デバッグ用：自動でバトル開始
            if (useDebugPlayers)
            {
                StartDebugBattle();
            }
        }

        private void OnDestroy()
        {
            if (battleManager != null)
            {
                battleManager.OnBattleStart -= OnBattleStart;
                battleManager.OnTurnStart -= OnTurnStart;
                battleManager.OnBattleEnd -= OnBattleEnd;
            }
        }

        /// <summary>
        /// デバッグ用バトル開始
        /// </summary>
        private void StartDebugBattle()
        {
            var player1 = new Player("プレイヤー1", 3); // 資格3つ
            var player2 = new Player("プレイヤー2", 5); // 資格5つ

            // インスペクターで設定されたカードがある場合、P1のデッキを作成
            if (debugPrimaryCardsP1 != null && debugPrimaryCardsP1.Count > 0)
            {
                Debug.Log($"Debug: Setting up fixed deck for {player1.Name} with {debugPrimaryCardsP1.Count} cards.");
                player1.Deck = new DeckData("Debug Fixed Deck");
                
                // 設定された順序で追加
                foreach (var cardData in debugPrimaryCardsP1)
                {
                    if (cardData != null)
                    {
                        // IDが空の場合は名前をフォールバックとして使用（CardData修正済みだが念のため）
                        string id = !string.IsNullOrEmpty(cardData.CardId) ? cardData.CardId : cardData.name;
                        player1.Deck.AddPrimaryCard(id);
                    }
                }
            }

            battleManager.StartBattle(player1, player2);
        }

        private void OnBattleStart()
        {
            ShowMessage("バトル開始！");
            UpdateUI();
        }

        private void OnTurnStart(Player player)
        {
            if (turnText != null)
            {
                turnText.text = $"{player.Name} のターン";
            }
            ShowMessage($"{player.Name} のターンです");
            UpdateUI();
        }

        private void OnBattleEnd(Player winner)
        {
            if (resultPanel != null)
            {
                resultPanel.SetActive(true);
            }
            if (resultText != null)
            {
                resultText.text = $"{winner.Name} の勝利！";
            }
            if (skipButton != null)
            {
                skipButton.gameObject.SetActive(false);
            }
        }

        private void OnSkipButtonClicked()
        {
            // 現在のプレイヤーがスキップ
            battleManager?.SkipTurn();
        }

        private async void OnReturnButtonClicked()
        {
            if (SceneController.Instance != null)
            {
                await SceneController.Instance.GoToHome();
            }
        }

        private void UpdateUI()
        {
            if (battleManager == null) return;

            var p1 = battleManager.Player1;
            var p2 = battleManager.Player2;

            if (p1 != null)
            {
                if (player1NameText != null) player1NameText.text = p1.Name;
                if (player1HPText != null) player1HPText.text = $"HP: {p1.CurrentHP}/{p1.MaxHP}";
            }

            if (p2 != null)
            {
                if (player2NameText != null) player2NameText.text = p2.Name;
                if (player2HPText != null) player2HPText.text = $"HP: {p2.CurrentHP}/{p2.MaxHP}";
            }
        }

        private void ShowMessage(string message)
        {
            if (messageText != null)
            {
                messageText.text = message;
            }
            Debug.Log($"[Battle] {message}");
        }
    }
}
