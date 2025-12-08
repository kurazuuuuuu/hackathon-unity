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
        [SerializeField] public bool useDebugPlayers = true;
        [SerializeField] private List<CardData> debugPrimaryCardsP1 = new List<CardData>(); // P1の主力カード手動設定

        [Header("Action Selection")]
        [SerializeField] private Game.UI.BattleActionSelectionUI actionSelectionUI;

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

        /// <summary>
        /// 主力カードがクリックされた際にアクション選択UIを表示
        /// </summary>
        public void ShowActionSelection(CardBase card)
        {
            Debug.Log($"[BattleScene] ShowActionSelection called for: {card?.Name}");
            
            if (actionSelectionUI == null)
            {
                // Use FindObjectsByType to include inactive objects
                var results = FindObjectsByType<Game.UI.BattleActionSelectionUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                if (results.Length > 0)
                {
                    actionSelectionUI = results[0];
                }
                Debug.Log($"[BattleScene] actionSelectionUI found: {actionSelectionUI != null}");
            }
            
            if (actionSelectionUI != null)
            {
                Debug.Log("[BattleScene] Calling actionSelectionUI.Show()");
                actionSelectionUI.Show(card, (isAttack) =>
                {
                    if (isAttack)
                    {
                        PerformNormalAttack(card);
                    }
                    else
                    {
                        PerformSpecialSkill(card);
                    }
                });
            }
            else
            {
                Debug.LogWarning("[BattleScene] actionSelectionUI is null!");
            }
        }

        private void PerformNormalAttack(CardBase card)
        {
            ShowMessage($"{card.Name} で攻撃！対象を選択してください");
            
            // Get or create TargetSelectionMode
            var targetMode = FindAnyObjectByType<Game.UI.TargetSelectionMode>();
            if (targetMode == null)
            {
                var go = new GameObject("TargetSelectionMode");
                targetMode = go.AddComponent<Game.UI.TargetSelectionMode>();
            }
            
            targetMode.StartTargetSelection(card, true, (target) =>
            {
                if (target != null)
                {
                    ExecuteAttack(card, target);
                }
                else
                {
                    ShowMessage("攻撃がキャンセルされました");
                }
            });
        }

        private void PerformSpecialSkill(CardBase card)
        {
            if (card.Ability != null)
            {
                ShowMessage($"{card.Name} で特殊効果発動！対象を選択してください");
                
                var targetMode = FindAnyObjectByType<Game.UI.TargetSelectionMode>();
                if (targetMode == null)
                {
                    var go = new GameObject("TargetSelectionMode");
                    targetMode = go.AddComponent<Game.UI.TargetSelectionMode>();
                }
                
                targetMode.StartTargetSelection(card, false, (target) =>
                {
                    if (target != null)
                    {
                        ExecuteSkill(card, target);
                    }
                    else
                    {
                        ShowMessage("特殊効果がキャンセルされました");
                    }
                });
            }
            else
            {
                ShowMessage($"{card.Name} には特殊効果がありません");
            }
        }

        private void ExecuteAttack(CardBase attacker, CardBase target)
        {
            int damage = attacker.Power;
            
            // Apply damage to target's PrimaryCard
            var primaryTarget = target.GetComponent<PrimaryCard>();
            if (primaryTarget != null)
            {
                primaryTarget.TakeDamage(damage);
                ShowMessage($"{attacker.Name} が {target.Name} に {damage} ダメージ！");
            }
            
            // End turn
            battleManager?.SkipTurn(); // TODO: Replace with proper end turn logic
        }

        private void ExecuteSkill(CardBase source, CardBase target)
        {
            var context = new Game.Abilities.BattleContext(source, target, battleManager?.CurrentPlayer, battleManager);
            source.Ability?.Activate(context);
            ShowMessage($"{source.Name} が {target.Name} に特殊効果を発動！");
            
            // End turn
            battleManager?.SkipTurn();
        }
    }
}
