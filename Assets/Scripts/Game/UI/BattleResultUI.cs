using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.System;
using Game.Network;
using Game.Data;

namespace Game.UI
{
    public class BattleResultUI : MonoBehaviour
    {
        private GameObject panelObj;
        private TextMeshProUGUI resultText;
        private Button homeButton;

        private void Awake()
        {
            CreateUI();
            Hide();
        }

        public async void Show(bool isWin)
        {
            if (panelObj != null) panelObj.SetActive(true);
            
            if (resultText != null)
            {
                resultText.text = isWin ? "VICTORY" : "DEFEAT";
                resultText.color = isWin ? new Color(1f, 0.8f, 0.2f) : new Color(0.8f, 0.2f, 0.2f); // Gold or Red
            }

            // バトル結果をサーバーに保存
            await SaveBattleResult(isWin);
        }

        private async global::System.Threading.Tasks.Task SaveBattleResult(bool isWin)
        {
            if (ApiClient.Instance == null || ApiClient.Instance.UserData == null)
            {
                Debug.Log("[BattleResultUI] UserData is null, skipping save.");
                return;
            }

            try
            {
                SaveOverlayManager.Instance?.Show("結果を保存中...");
                
                // 勝利報酬などがあれば追加（例: チケット獲得）
                if (isWin)
                {
                    ApiClient.Instance.UserData.GachaTickets += 1;
                    Debug.Log("[BattleResultUI] 勝利報酬: チケット+1");
                }

                var result = await ApiClient.Instance.SaveUserData(ApiClient.Instance.UserData);
                
                if (result.Success)
                {
                    Debug.Log("[BattleResultUI] バトル結果保存成功");
                }
                else
                {
                    Debug.LogWarning($"[BattleResultUI] バトル結果保存失敗: {result.Error}");
                }
            }
            catch (global::System.Exception e)
            {
                Debug.LogError($"[BattleResultUI] 保存エラー: {e.Message}");
            }
            finally
            {
                SaveOverlayManager.Instance?.Hide();
            }
        }

        public void Hide()
        {
            if (panelObj != null) panelObj.SetActive(false);
        }

        private void CreateUI()
        {
            // Create Panel
            panelObj = new GameObject("ResultPanel");
            panelObj.transform.SetParent(transform, false);
            
            RectTransform rect = panelObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            Image bg = panelObj.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.9f);
            bg.raycastTarget = true; // ブロックレイキャスト

            // 最前面に表示
            panelObj.transform.SetAsLastSibling();

            // Create Result Text
            GameObject textObj = new GameObject("ResultText");
            textObj.transform.SetParent(panelObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0.5f);
            textRect.anchorMax = new Vector2(1, 0.5f);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.sizeDelta = new Vector2(0, 200);
            textRect.anchoredPosition = new Vector2(0, 100);

            resultText = textObj.AddComponent<TextMeshProUGUI>();
            resultText.alignment = TextAlignmentOptions.Center;
            resultText.fontSize = 96;
            resultText.fontStyle = FontStyles.Bold;
            resultText.enableAutoSizing = true;

            // Create Home Button
            GameObject buttonObj = new GameObject("HomeButton");
            buttonObj.transform.SetParent(panelObj.transform, false);

            RectTransform btnRect = buttonObj.AddComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(300, 100);
            btnRect.anchoredPosition = new Vector2(0, -100);

            Image btnImg = buttonObj.AddComponent<Image>();
            btnImg.color = Color.white;

            homeButton = buttonObj.AddComponent<Button>();
            homeButton.onClick.AddListener(OnHomeButtonClicked);

            // Button Text
            GameObject btnTextObj = new GameObject("Text");
            btnTextObj.transform.SetParent(buttonObj.transform, false);
            
            RectTransform btnTextRect = btnTextObj.AddComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
            btnText.text = "Back to Home";
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.fontSize = 36;
            btnText.color = Color.black;
        }

        private async void OnHomeButtonClicked()
        {
            if (SceneController.Instance != null)
            {
                await SceneController.Instance.GoToHome();
            }
            else
            {
                // Fallback
                UnityEngine.SceneManagement.SceneManager.LoadScene("HomeScene");
            }
        }
    }
}
