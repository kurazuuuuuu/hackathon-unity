using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Abilities;

namespace Game
{
    /// <summary>
    /// カードの基底MonoBehaviourクラス
    /// すべてのカードタイプで共通のロジックを定義
    /// </summary>
    public abstract class CardBase : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] protected TextMeshProUGUI nameText;
        [SerializeField] protected TextMeshProUGUI powerText;
        [SerializeField] protected TextMeshProUGUI costText;
        [SerializeField] protected TextMeshProUGUI abilityText;
        [SerializeField] protected TextMeshProUGUI chargeText;
        
        [Header("Card Image")]
        [SerializeField] protected Image cardImage;        // カード背景画像
        [SerializeField] protected Image backgroundImage;  // 背景（フォールバック用）

        [Header("Drag Settings")]
        [SerializeField] protected bool isDraggable = true;

        // カードの基本情報
        public string CardId { get; protected set; }
        public string Name { get; protected set; }
        public CardType Type { get; protected set; }
        public int Power { get; protected set; }
        public int Cost { get; protected set; }
        public CardAbility Ability { get; protected set; }
        public CardAbility PassiveEffect { get; protected set; }
        public int Charge { get; protected set; }
        public int Rarity { get; protected set; }

        // ドラッグ状態
        public bool IsDraggable
        {
            get => isDraggable;
            set
            {
                if (isDraggable != value)
                {
                    isDraggable = value;
                    OnDraggableChanged?.Invoke(isDraggable);
                }
            }
        }

        public bool IsDragging { get; private set; } = false;

        // イベント
        public event Action<bool> OnDraggableChanged;
        public event Action OnDragStart;
        public event Action OnDragEnd;

        protected virtual void Start()
        {
            // Force layout rebuild after frames to ensure GridLayoutGroup has set our size
            StartCoroutine(RebuildLayoutDelayed());
        }

        private IEnumerator RebuildLayoutDelayed()
        {
            // Wait for GridLayoutGroup to set our size
            yield return null;
            yield return null; // Extra frame for good measure
            
            RebuildAllLayouts();
            
            // One more rebuild after a short delay
            yield return new WaitForSeconds(0.1f);
            RebuildAllLayouts();
        }

        private void RebuildAllLayouts()
        {
            Canvas.ForceUpdateCanvases();
            
            RectTransform rect = GetComponent<RectTransform>();
            if (rect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            }
            
            // Recursively rebuild all child LayoutGroups
            foreach (var layoutGroup in GetComponentsInChildren<LayoutGroup>(true))
            {
                RectTransform layoutRect = layoutGroup.GetComponent<RectTransform>();
                if (layoutRect != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRect);
                }
            }
        }

        /// <summary>
        /// CardDataBaseを使用してカードを初期化する
        /// </summary>
        public virtual void Initialize(CardDataBase data)
        {
            if (data == null)
            {
                Debug.LogError("CardDataBase is null!");
                return;
            }

            this.CardId = data.CardId;
            this.Name = data.CardName;
            this.Type = data.CardType;
            this.Power = data.Power;
            this.Cost = data.Cost;
            this.Ability = data.Ability;
            this.PassiveEffect = data.PassiveEffect;
            this.Charge = data.Charge;
            this.Rarity = data.Rarity;

            // CardImageを透明に設定（フレームを見せるため）
            HideCardImage();
            
            UpdateUI();
        }
        
        /// <summary>
        /// CardImageを透明にする
        /// </summary>
        protected void HideCardImage()
        {
            if (cardImage == null)
            {
                Transform cardImageTransform = transform.Find("CardImage");
                if (cardImageTransform != null)
                {
                    cardImage = cardImageTransform.GetComponent<Image>();
                }
            }
            
            if (cardImage != null)
            {
                cardImage.color = Color.clear; // 透明に
            }
        }

        /// <summary>
        /// ガチャ結果表示用の初期化（レアリティスタイル適用）
        /// </summary>
        public virtual void InitializeForGacha(CardDataBase data)
        {
            Initialize(data);
            
            // Load card background image (with fallback)
            LoadCardImage(data.CardId);
            
            // Apply rarity style (glow effect)
            var visualizer = GetComponent<UI.CardVisualizer>();
            if (visualizer == null)
            {
                visualizer = gameObject.AddComponent<UI.CardVisualizer>();
            }
            visualizer.ApplyRarityStyle(data.Rarity);
            
            // Disable drag handler completely for gacha display
            var dragHandler = GetComponent<CardDragHandler>();
            if (dragHandler != null)
            {
                dragHandler.enabled = false;
            }
        }
        
        /// <summary>
        /// カード画像をResources/Textures/Cards/からロード
        /// パス: Textures/Cards/{rarity}x/{cardId}
        /// </summary>
        protected virtual void LoadCardImage(string cardId)
        {
            // cardImageが設定されていない場合、子オブジェクトから探す
            if (cardImage == null)
            {
                Transform cardImageTransform = transform.Find("CardImage");
                if (cardImageTransform != null)
                {
                    cardImage = cardImageTransform.GetComponent<Image>();
                }
            }
            
            // それでもnullなら処理しない（フレームのImageを変更しないため）
            if (cardImage == null)
            {
                return; // Silent return - CardImage is optional
            }
            
            // カードIDからパスを生成
            int rarity = CardHelper.GetRarityFromId(cardId);
            string path = $"Textures/Cards/{rarity}x/{cardId}";
            
            Sprite sprite = Resources.Load<Sprite>(path);
            
            if (sprite != null)
            {
                cardImage.sprite = sprite;
                cardImage.color = Color.white;
                Debug.Log($"[CardBase] Loaded card image: {path}");
            }
            else
            {
                // Fallback to default card background
                Sprite fallback = Resources.Load<Sprite>("Cards/bg_card_test");
                if (fallback != null)
                {
                    cardImage.sprite = fallback;
                    cardImage.color = Color.white;
                    Debug.Log($"[CardBase] Using fallback image for: {cardId}");
                }
            }
        }

        /// <summary>
        /// UIを更新（派生クラスでオーバーライド可能）
        /// </summary>
        protected virtual void UpdateUI()
        {
            if (nameText != null) nameText.text = Name;
            if (powerText != null) powerText.text = $"ATK: {Power}";
            if (costText != null) costText.text = $"COST: {Cost}";
            if (abilityText != null) abilityText.text = Ability?.Description ?? "";
            if (chargeText != null) chargeText.text = $"{Charge}";
        }

        #region Drag Methods
        public void StartDrag()
        {
            if (!isDraggable) return;
            IsDragging = true;
            OnDragStart?.Invoke();
        }

        public void EndDrag()
        {
            IsDragging = false;
            OnDragEnd?.Invoke();
        }

        public void EnableDrag()
        {
            IsDraggable = true;
        }

        public void DisableDrag()
        {
            IsDraggable = false;
            if (IsDragging) EndDrag();
        }
        #endregion

        /// <summary>
        /// アビリティを発動する
        /// 注意: CardAbility.ActivateはCard型を期待するため、
        /// CardBaseから直接呼び出す場合は派生クラスでオーバーライドが必要
        /// </summary>
        public virtual void UseAbility(CardBase target = null)
        {
            if (Ability != null)
            {
                // CardAbilityはCard型を期待するため、警告を出す
                Debug.LogWarning($"CardBase.UseAbility called. Override in derived class for proper Card type support.");
            }
            else
            {
                Debug.Log($"{Name} has no ability.");
            }
        }
    }
}
