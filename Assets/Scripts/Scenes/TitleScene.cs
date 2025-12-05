using UnityEngine;
using UnityEngine.UI;
using Game.System;

namespace Game.Scenes
{
    /// <summary>
    /// タイトル画面
    /// </summary>
    public class TitleScene : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Button startButton;

        private void Start()
        {
            if (startButton != null)
            {
                startButton.onClick.AddListener(OnStartButtonClicked);
            }
        }

        private async void OnStartButtonClicked()
        {
            await SceneController.Instance.GoToLogin();
        }
    }
}
