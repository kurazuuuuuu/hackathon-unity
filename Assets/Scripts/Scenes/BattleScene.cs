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

        [SerializeField] private TextMeshProUGUI turnText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Game.UI.TypewriterText messageTypewriter; // タイプライター効果用
        [SerializeField] private Button skipButton;
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private Button returnButton;

        [Header("Debug")]
        [SerializeField] public bool useDebugPlayers = true;
        [SerializeField] private List<CardData> debugPrimaryCardsP1 = new List<CardData>(); // P1の主力カード手動設定

        [Header("Action Selection")]
        [SerializeField] private Game.UI.BattleActionSelectionUI actionSelectionUI;
        
        [Header("Result UI")]
        [SerializeField] private Game.UI.BattleResultUI resultUI;

        private void Awake()
        {
            // SerializeFieldが未設定の場合、名前でUI要素を自動取得
            AutoBindUIElements();

            // Create Result UI if not assigned
            if (resultUI == null)
            {
                var resultObj = new GameObject("BattleResultUI");
                resultObj.transform.SetParent(transform.Find("Canvas")); // Ensure it's under Canvas
                if (resultObj.transform.parent == null) resultObj.transform.SetParent(transform); // Fallback
                
                resultUI = resultObj.AddComponent<UI.BattleResultUI>();
                
                // Set RectTransform to stretch
                RectTransform rect = resultObj.GetComponent<RectTransform>();
                if (rect == null) rect = resultObj.AddComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.sizeDelta = Vector2.zero;
            }
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
            
            // --- Atomic UI Binding ---
            // Find HandAreas and Zones created by Atomic UI
            if (battleManager != null)
            {
                var p1Area = GameObject.Find("Player1Area");
                var p2Area = GameObject.Find("Player2Area");
                
                Transform h1 = null, h2 = null;
                Game.PrimaryCardZone z1 = null, z2 = null;

                if (p1Area != null)
                {
                    var handObj = FindDeepChild(p1Area.transform, "HandArea");
                    if (handObj != null) h1 = handObj;
                    
                    var zoneObj = p1Area.transform.Find("Player1_PrimaryZone");
                    if (zoneObj != null) z1 = zoneObj.GetComponent<Game.PrimaryCardZone>();
                }
                if (p2Area != null)
                {
                    // P2 Hand might be "Player2_HandVisuals" or not spawned if hidden logic? 
                    // Per HUDOrganism, we created "Player2_HandVisuals". 
                    // But BattleManager might need a transform even if visuals are fake?
                    // Actually Player.cs likely needs a place to put cards.
                    // If we use "Atomic" logic, P2 hand is hidden.
                    // But we should assign a transform (even if off-screen) so logic doesn't crash.
                    // Let's look for "Player2_HandVisuals" as a placeholder or create a dummy if needed.
                    var handObj = FindDeepChild(p2Area.transform, "Player2_HandVisuals");
                    if (handObj != null) h2 = handObj;
                    
                    var zoneObj = p2Area.transform.Find("Player2_PrimaryZone");
                    if (zoneObj != null) z2 = zoneObj.GetComponent<Game.PrimaryCardZone>();
                }

                battleManager.SetUIReferences(h1, h2, z1, z2);
            }
        }
        
        private Transform FindDeepChild(Transform parent, string name)
        {
            foreach(Transform child in parent)
            {
                if(child.name == name) return child;
                var result = FindDeepChild(child, name);
                if (result != null) return result;
            }
            return null;
        }

        private void OnEnable()
        {
            if (battleManager != null)
            {
                battleManager.OnBattleStart += OnBattleStart;
                battleManager.OnTurnStart += OnTurnStart;
                battleManager.OnBattleEnd += HandleBattleEnd; // Subscribing to new handler
            }
        }

        private void OnDisable()
        {
            if (battleManager != null)
            {
                battleManager.OnBattleStart -= OnBattleStart;
                battleManager.OnTurnStart -= OnTurnStart;
                battleManager.OnBattleEnd -= HandleBattleEnd; // Unsubscribing from new handler
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

            // 結果パネルを非表示 (This will be handled by resultUI now, but keeping for now as per instruction's diff context)
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
            var player2 = new Player("Bot", 5); // Bot対戦相手
            player2.IsBot = true; // Botとしてマーク

            // BotAIコンポーネントが存在することを確認
            var botAI = FindAnyObjectByType<BotAI>();
            if (botAI == null)
            {
                var botAIGO = new GameObject("BotAI");
                botAI = botAIGO.AddComponent<BotAI>();
                Debug.Log("[BattleScene] BotAI component created.");
            }

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
            // TypewriterTextコンポーネントを使用（存在する場合）
            if (messageTypewriter == null && messageText != null)
            {
                messageTypewriter = messageText.GetComponent<Game.UI.TypewriterText>();
            }
            
            if (messageTypewriter != null)
            {
                messageTypewriter.ShowText(message);
            }
            else if (messageText != null)
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
            
            // Highlight selected card
            HighlightCard(card);
            
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
                    // Dehighlight all cards when action is selected
                    DehighlightAllCards();
                    
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
            // Start attack animation
            StartCoroutine(AnimateAttack(attacker, target));
        }
        
        private global::System.Collections.IEnumerator AnimateAttack(CardBase attacker, CardBase target)
        {
            int damage = attacker.Power;
            
            // Get transforms
            Transform attackerTransform = attacker.transform;
            Transform targetTransform = target.transform;
            
            Vector3 attackerOriginalPos = attackerTransform.position;
            Vector3 targetPos = targetTransform.position;
            
            // Move attacker towards target
            float duration = 0.3f;
            float elapsed = 0f;
            
            // Start Move
            if (battleManager != null) PlayVFX(battleManager.AttackVFX, attackerOriginalPos);
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                attackerTransform.position = Vector3.Lerp(attackerOriginalPos, targetPos, t);
                yield return null;
            }
            
            // Apply damage
            var primaryTarget = target.GetComponent<PrimaryCard>();
            if (primaryTarget != null)
            {
                // Hit VFX
                if (battleManager != null) PlayVFX(battleManager.DamageVFX, targetPos);
                
                primaryTarget.TakeDamage(damage);
                ShowMessage($"{attacker.Name} が {target.Name} に {damage} ダメージ!");
            }
            
            // Return attacker to original position
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                attackerTransform.position = Vector3.Lerp(targetPos, attackerOriginalPos, t);
                yield return null;
            }
            
            attackerTransform.position = attackerOriginalPos;
            
            // End turn
            battleManager?.SkipTurn();
        }
        
        private void HighlightCard(CardBase card)
        {
            if (card == null) return;
            
            // Find all primary cards in the scene
            var allPrimaryCards = FindObjectsByType<PrimaryCard>(FindObjectsSortMode.None);
            
            foreach (var primaryCard in allPrimaryCards)
            {
                var canvasGroup = primaryCard.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                    canvasGroup = primaryCard.gameObject.AddComponent<CanvasGroup>();
                
                // Dim other cards, keep selected card bright
                if (primaryCard == card || primaryCard.gameObject == card.gameObject)
                {
                    canvasGroup.alpha = 1.0f;
                    
                    // Move selected card forward
                    var rectTransform = primaryCard.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        Vector3 pos = rectTransform.localPosition;
                        pos.z = -50;
                        rectTransform.localPosition = pos;
                        
                        rectTransform.localScale = Vector3.one * 1.1f;
                    }
                }
                else if (!primaryCard.IsDead)
                {
                    canvasGroup.alpha = 0.6f;
                }
            }
        }

        private void HandleBattleEnd(Player winner)
        {
            // 結果パネルがnullの場合は何もしない（Startで生成されているはず）
            if (resultUI != null)
            {
                bool isWin = winner == battleManager.Player1; // Player1 is usually the local player
                resultUI.Show(isWin);
            }
            
            // Disable input or other UI if necessary
            if (actionSelectionUI != null) actionSelectionUI.Close();
        }

        private void OnBattleEnd(Player winner)
        {
            HandleBattleEnd(winner);
        }
        
        private void DehighlightAllCards()
        {
            var allPrimaryCards = FindObjectsByType<PrimaryCard>(FindObjectsSortMode.None);
            
            foreach (var primaryCard in allPrimaryCards)
            {
                var canvasGroup = primaryCard.GetComponent<CanvasGroup>();
                if (canvasGroup != null && !primaryCard.IsDead)
                {
                    canvasGroup.alpha = 1.0f;
                }
                
                // Reset position and scale
                var rectTransform = primaryCard.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    Vector3 pos = rectTransform.localPosition;
                    pos.z = 0;
                    rectTransform.localPosition = pos;
                    
                    rectTransform.localScale = Vector3.one;
                }
            }
        }

        private void ExecuteSkill(CardBase source, CardBase target)
        {
            StartCoroutine(AnimateSkill(source, target));
        }

        private global::System.Collections.IEnumerator AnimateSkill(CardBase source, CardBase target)
        {
            var context = new Game.Abilities.BattleContext(source, target, battleManager?.CurrentPlayer, battleManager);
            
            // Skill Activate VFX
            if (battleManager != null) PlayVFX(battleManager.SkillActivateVFX, source.transform.position);
            
            ShowMessage($"{source.Name} が {target.Name} に特殊効果を発動！");
            yield return new WaitForSeconds(0.5f);

            // Activate Ability
            source.Ability?.Activate(context);
            
            // Skill Hit VFX (on target if applicable)
            if (target != null && battleManager != null)
            {
                PlayVFX(battleManager.SkillHitVFX, target.transform.position);
            }

            yield return new WaitForSeconds(0.5f);
            
            // End turn
            battleManager?.SkipTurn();
        }

        private void PlayVFX(GameObject prefab, Vector3 position)
        {
            if (prefab == null) return;
            
            // Canvasの子として生成（UI用パーティクル対応）
            var canvas = FindAnyObjectByType<Canvas>();
            Transform parent = canvas != null ? canvas.transform : null;
            
            GameObject vfx = Instantiate(prefab, position, Quaternion.identity, parent);
            
            // RectTransformの場合は位置を調整
            var rectTransform = vfx.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.position = position;
            }
            
            // 最前面に表示
            vfx.transform.SetAsLastSibling();
            
            // パーティクルを明示的に再生
            var ps = vfx.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Clear();
                ps.Play();
                Destroy(vfx, ps.main.duration + ps.main.startLifetime.constantMax + 0.5f);
            }
            else
            {
                // 子にあるパーティクルも確認
                var childPs = vfx.GetComponentInChildren<ParticleSystem>();
                if (childPs != null)
                {
                    childPs.Clear();
                    childPs.Play();
                    Destroy(vfx, childPs.main.duration + childPs.main.startLifetime.constantMax + 0.5f);
                }
                else
                {
                    Destroy(vfx, 2.0f);
                }
            }
        }
    }
}
