using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace Game
{
    /// <summary>
    /// 敵の主力カード用コンポーネント
    /// 攻撃対象選択時のみクリック可能
    /// </summary>
    public class EnemyPrimaryCard : PrimaryCard, IPointerClickHandler
    {
        private bool isTargetable = false;
        
        /// <summary>
        /// ターゲット選択モードを設定
        /// </summary>
        public void SetTargetable(bool targetable)
        {
            isTargetable = targetable;
            
            // 視覚的フィードバック（オプション）
            if (targetable)
            {
                // ターゲット可能時は少し明るく
                var canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1.0f;
                }
            }
        }
        
        /// <summary>
        /// クリック時の処理（ターゲット選択時のみ）
        /// </summary>
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (!isTargetable)
            {
                Debug.Log($"[EnemyPrimaryCard] {Name} はターゲット選択モードではありません");
                return;
            }
            
            if (IsDead)
            {
                Debug.Log($"[EnemyPrimaryCard] {Name} は既に撃破されています");
                return;
            }
            
            Debug.Log($"[EnemyPrimaryCard] {Name} がターゲットとして選択されました");
            
            // Use TargetSelectionMode to handle target selection
            var targetMode = FindAnyObjectByType<Game.UI.TargetSelectionMode>();
            if (targetMode != null && targetMode.IsSelecting)
            {
                targetMode.SelectTarget(this);
            }
        }
    }
}
