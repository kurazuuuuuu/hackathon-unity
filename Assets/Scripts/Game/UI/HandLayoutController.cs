using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// 手札の動的レイアウトを管理するコンポーネント
    /// カード数に応じてスペーシングとスケールを自動調整
    /// </summary>
    public class HandLayoutController : MonoBehaviour
    {
        [Header("Layout Settings")]
        [SerializeField] private float maxHandWidth = 2000f; // 最大幅
        [SerializeField] private float cardWidth = 280f; // カードの基本幅
        [SerializeField] private float minSpacing = -150f; // 最小スペーシング（重なり）
        [SerializeField] private float maxSpacing = 50f; // 最大スペーシング（離れる）
        [SerializeField] private float minScale = 0.6f; // 最小スケール
        [SerializeField] private float maxScale = 1.0f; // 最大スケール
        [SerializeField] private int idealCardCount = 5; // 理想的なカード枚数

        private HorizontalLayoutGroup layoutGroup;
        private RectTransform rectTransform;
        private List<RectTransform> cards = new List<RectTransform>();

        private void Awake()
        {
            layoutGroup = GetComponent<HorizontalLayoutGroup>();
            rectTransform = GetComponent<RectTransform>();
        }

        private void OnTransformChildrenChanged()
        {
            // 子要素が変更されたら再計算
            UpdateLayout();
        }

        public void UpdateLayout()
        {
            if (layoutGroup == null) return;

            // 現在のカード数を取得
            cards.Clear();
            foreach (Transform child in transform)
            {
                var rt = child.GetComponent<RectTransform>();
                if (rt != null && child.gameObject.activeInHierarchy)
                {
                    cards.Add(rt);
                }
            }

            int cardCount = cards.Count;
            if (cardCount == 0) return;

            // スペーシングを計算
            // カードが多いほど重なりを増やす
            float t = Mathf.Clamp01((float)(cardCount - 1) / (float)(idealCardCount * 2 - 1));
            float spacing = Mathf.Lerp(maxSpacing, minSpacing, t);
            layoutGroup.spacing = spacing;

            // 全体幅を計算
            float totalWidth = cardCount * cardWidth + (cardCount - 1) * spacing;

            // 幅がオーバーする場合はスケールを調整
            if (totalWidth > maxHandWidth && cardCount > 1)
            {
                // より強い重なりに調整
                float targetWidth = maxHandWidth;
                float requiredSpacing = (targetWidth - cardCount * cardWidth) / (cardCount - 1);
                layoutGroup.spacing = Mathf.Max(requiredSpacing, minSpacing - 50f);
            }

            // カードのスケールも調整（オプション）
            float scale = cardCount <= idealCardCount ? maxScale : 
                          Mathf.Lerp(maxScale, minScale, (float)(cardCount - idealCardCount) / idealCardCount);
            
            foreach (var card in cards)
            {
                card.localScale = Vector3.one * scale;
            }
        }
    }
}
