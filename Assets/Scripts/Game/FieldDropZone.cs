using UnityEngine;
using UnityEngine.EventSystems;
using Game.Battle;

namespace Game
{
    /// <summary>
    /// 特殊カード用のフィールドドロップゾーン
    /// 主力カードをターゲットにしないカードをここにドロップして使用する
    /// </summary>
    public class FieldDropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Visual Feedback")]
        [SerializeField] private Color highlightColor = new Color(0.5f, 0.5f, 1f, 0.3f);
        [SerializeField] private UnityEngine.UI.Image highlightImage;
        
        private Color originalColor;
        
        private void Awake()
        {
            if (highlightImage != null)
            {
                originalColor = highlightImage.color;
            }
        }
        
        public void OnDrop(PointerEventData eventData)
        {
            ShowHighlight(false);
            
            if (eventData.pointerDrag == null) return;
            
            // Get card component
            Card droppedCard = eventData.pointerDrag.GetComponent<Card>();
            if (droppedCard == null) return;
            
            // Only accept Special cards (non-targeted)
            if (droppedCard.Type != CardType.Special)
            {
                Debug.Log($"[FieldDropZone] {droppedCard.Name} は特殊カードではありません。主力カードにドロップしてください。");
                return;
            }
            
            // Get BattleManager
            var bm = BattleManager.Instance;
            if (bm == null || bm.CurrentPlayer == null)
            {
                Debug.LogError("[FieldDropZone] BattleManager or CurrentPlayer is null!");
                return;
            }
            
            // Play card without target
            Debug.Log($"[FieldDropZone] {bm.CurrentPlayer.Name} が特殊カード [{droppedCard.Name}] を使用");
            bm.PlayCard(droppedCard, null);
            
            // Destroy the card object
            Destroy(droppedCard.gameObject, 0.1f);
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            // Only highlight if dragging a Special card
            if (eventData.pointerDrag != null)
            {
                Card card = eventData.pointerDrag.GetComponent<Card>();
                if (card != null && card.Type == CardType.Special)
                {
                    ShowHighlight(true);
                }
            }
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            ShowHighlight(false);
        }
        
        private void ShowHighlight(bool show)
        {
            if (highlightImage != null)
            {
                highlightImage.color = show ? highlightColor : originalColor;
            }
        }
    }
}
