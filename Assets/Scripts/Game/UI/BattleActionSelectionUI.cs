using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Game.Battle;

namespace Game.UI
{
    public class BattleActionSelectionUI : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Button attackButton;
        [SerializeField] private Button skillButton;
        [SerializeField] private Button closeButton; // Click outside to close

        private Action<bool> onSelection; // true=Attack, false=Skill
        private CardBase currentCard;

        private void Awake()
        {
            // Auto-bind CanvasGroup
            if (canvasGroup == null) 
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
            
            // Auto-find buttons if missing
            if (attackButton == null) attackButton = transform.Find("Buttons/AttackButton")?.GetComponent<Button>();
            if (skillButton == null) skillButton = transform.Find("Buttons/SkillButton")?.GetComponent<Button>();
            if (closeButton == null) closeButton = GetComponent<Button>();

            attackButton?.onClick.AddListener(() => OnActionSelected(true));
            skillButton?.onClick.AddListener(() => OnActionSelected(false));
            closeButton?.onClick.AddListener(Close);
            
            // Initially hidden
            Close();
        }

        public void Show(CardBase card, Action<bool> callback)
        {
            currentCard = card;
            onSelection = callback;

            gameObject.SetActive(true);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
            }
        }

        public void Close()
        {
            gameObject.SetActive(false);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
            }
        }

        private void OnActionSelected(bool isAttack)
        {
            onSelection?.Invoke(isAttack);
            Close();
        }
    }
}
