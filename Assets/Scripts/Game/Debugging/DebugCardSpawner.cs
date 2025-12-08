using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Debugging
{
    /// <summary>
    /// デバッグ用：クリックでランダムなカードをスポーンする
    /// </summary>
    public class DebugCardSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CardManager cardManager;
        [SerializeField] private Canvas targetCanvas;

        private void Update()
        {
            // 新しいInput Systemでマウス左クリックを検出
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                SpawnRandomCard();
            }
        }

        private void SpawnRandomCard()
        {
            if (cardManager == null)
            {
                cardManager = FindAnyObjectByType<CardManager>();
                if (cardManager == null)
                {
                    Debug.LogError("CardManager が見つかりません");
                    return;
                }
            }

            // ランダムなカードを取得してスポーン
            CardDataBase randomCard = cardManager.GetRandomCardData();
            if (randomCard != null)
            {
                Card card = cardManager.SpawnCard(randomCard);
                
                if (card != null)
                {
                    // マウス位置に配置
                    RectTransform rectTransform = card.GetComponent<RectTransform>();
                    if (rectTransform != null && targetCanvas != null)
                    {
                        Vector2 localPoint;
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(
                            targetCanvas.GetComponent<RectTransform>(),
                            Mouse.current.position.ReadValue(),
                            targetCanvas.worldCamera,
                            out localPoint
                        );
                        rectTransform.anchoredPosition = localPoint;
                    }

                    Debug.Log($"カードをスポーン: {card.Name} | Parent: {card.transform.parent?.name} | Position: {card.transform.position}");
                }
                else
                {
                    Debug.LogError("カードのスポーンに失敗しました");
                }
            }
        }
    }
}
