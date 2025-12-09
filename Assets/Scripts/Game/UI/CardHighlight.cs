using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// カードのターゲットハイライト用コンポーネント
    /// ホバー時にアウトライン効果を表示
    /// </summary>
    public class CardHighlight : MonoBehaviour
    {
        [Header("Highlight Settings")]
        [SerializeField] private Color highlightColor = new Color(1f, 0.8f, 0f, 0.8f); // 金色、少し透明
        [SerializeField] private float outlineWidth = 12f;
        [SerializeField] private GameObject outlineObject;

        private Image outlineImage;
        private bool isHighlighted;
        private bool isInitialized;

        /// <summary>
        /// 初期化（遅延初期化）
        /// </summary>
        private void Initialize()
        {
            if (isInitialized) return;
            isInitialized = true;
            
            // 既存のOutlineがあれば使用
            if (outlineObject != null) return;

            CreateOutline();
        }

        private void CreateOutline()
        {
            // アウトライン用のオブジェクトを作成（カードの親に配置）
            outlineObject = new GameObject("HighlightOutline");
            
            // カードの親に配置し、カードの背後に表示
            Transform parent = transform.parent;
            if (parent != null)
            {
                outlineObject.transform.SetParent(parent, false);
                // このカードの直前に配置
                int myIndex = transform.GetSiblingIndex();
                outlineObject.transform.SetSiblingIndex(myIndex);
            }
            else
            {
                outlineObject.transform.SetParent(transform, false);
                outlineObject.transform.SetAsFirstSibling();
            }

            RectTransform outlineRect = outlineObject.AddComponent<RectTransform>();
            
            // カードと同じ位置・サイズ + アウトライン分の拡張
            RectTransform myRect = GetComponent<RectTransform>();
            if (myRect != null)
            {
                outlineRect.anchorMin = myRect.anchorMin;
                outlineRect.anchorMax = myRect.anchorMax;
                outlineRect.pivot = myRect.pivot;
                outlineRect.anchoredPosition = myRect.anchoredPosition;
                outlineRect.sizeDelta = myRect.sizeDelta + new Vector2(outlineWidth * 2, outlineWidth * 2);
            }

            outlineImage = outlineObject.AddComponent<Image>();
            outlineImage.color = highlightColor;
            outlineImage.raycastTarget = false;

            outlineObject.SetActive(false);
        }

        public void SetHighlight(bool active)
        {
            if (active && !isInitialized)
            {
                Initialize();
            }

            isHighlighted = active;
            
            if (outlineObject != null)
            {
                outlineObject.SetActive(active);
                
                // 位置を更新
                if (active)
                {
                    UpdateOutlinePosition();
                }
            }
        }

        private void UpdateOutlinePosition()
        {
            if (outlineObject == null) return;
            
            RectTransform myRect = GetComponent<RectTransform>();
            RectTransform outlineRect = outlineObject.GetComponent<RectTransform>();
            
            if (myRect != null && outlineRect != null)
            {
                outlineRect.position = myRect.position;
                outlineRect.sizeDelta = myRect.sizeDelta + new Vector2(outlineWidth * 2, outlineWidth * 2);
            }
        }

        public bool IsHighlighted => isHighlighted;

        private void OnDisable()
        {
            SetHighlight(false);
        }

        private void OnDestroy()
        {
            if (outlineObject != null)
            {
                Destroy(outlineObject);
            }
        }
    }
}

