using UnityEngine;
using UnityEngine.UI;
using Game.UI;
using Game.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor; // For EditorUtility in ContextMenu
#endif

namespace Game.Gacha
{
    public class GachaDirector : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private GachaAnimationController animController;
        [SerializeField] private GameObject resultViewPrefab; // Changed to GameObject for easier assignment
        [SerializeField] private CanvasGroup uiCanvasGroup; // To fade out main UI

        [Header("Overlay")]
        [SerializeField] private Image blackOverlay; // For fade in/out

        [Header("BGM Settings")]
        [Tooltip("ガチャシーケンス中に再生するBGM")]
        [SerializeField] private AudioClip gachaBGM;
        
        [Tooltip("BGMのボリューム (0.0 - 1.0)")]
        [SerializeField, Range(0f, 1f)] private float bgmVolume = 0.7f;
        
        [Tooltip("フェードアウト時間（秒）")]
        [SerializeField] private float bgmFadeOutDuration = 0.5f;
        
        [Tooltip("シーンBGMをフェードアウトする時間（秒）")]
        [SerializeField] private float sceneBGMFadeDuration = 0.5f;

        private AudioSource _bgmAudioSource;
        private GachaResultView currentResultView; // Instance
        private bool isPlaying = false;

        private void Awake()
        {
            // BGM用AudioSourceの設定
            _bgmAudioSource = GetComponent<AudioSource>();
            if (_bgmAudioSource == null)
            {
                _bgmAudioSource = gameObject.AddComponent<AudioSource>();
            }
            _bgmAudioSource.playOnAwake = false;
            _bgmAudioSource.loop = true;
        }

        public async void PlayGachaSequence(List<CardDataBase> results, global::System.Action onComplete)
        {
            Debug.Log($"[GachaDirector] PlayGachaSequence Started at {Time.time}");
            if (isPlaying) return;
            isPlaying = true;

            // シーケンス開始時刻を記録
            float sequenceStartTime = Time.time;
            const float resultDisplayTime = 10.0f; // 結果表示までの時間（秒）

            // シーンBGMをフェードアウトで一時停止
            if (BGMManager.Instance != null)
            {
                BGMManager.Instance.PauseBGM(sceneBGMFadeDuration);
            }

            // ガチャBGM再生開始
            PlayBGM();

            int maxRarity = 0;
            foreach(var c in results) if(c.Rarity > maxRarity) maxRarity = c.Rarity;

            // 1. Fade Out Main UI & Fade In Black Overlay
            Debug.Log($"[GachaDirector] Step 1: Fade UI Out & Black In at {Time.time}");
            if (blackOverlay != null) 
            {
                Canvas canvas = GetComponentInParent<Canvas>();
                if (canvas != null && blackOverlay.transform.parent != canvas.transform)
                {
                     // Ensure overlay is under canvas if not already
                     blackOverlay.transform.SetParent(canvas.transform, false);
                }
                blackOverlay.transform.SetAsLastSibling(); 
            }
            await FadeRoutine(0, 1, 0.8f); // 0.5 -> 0.8
            
            if (uiCanvasGroup != null) uiCanvasGroup.alpha = 0; // Hide Main UI

            // 2. Play Animation (Cutscene)
            if (animController != null)
            {
                if (animController.HasAnimation(maxRarity))
                {
                    // Full Sequence: Fade Out -> Play -> Fade In
                    Debug.Log($"[GachaDirector] Step 2: Fade Black Out (Show Anim) at {Time.time}");
                    await FadeRoutine(1, 0, 0.8f); // 0.5 -> 0.8
                    
                    Debug.Log($"[GachaDirector] Play Animation (Rarity {maxRarity}) at {Time.time}");
                    await animController.PlayAnimation(maxRarity);
                    
                    Debug.Log($"[GachaDirector] Step 3: Fade Black In (End Anim) at {Time.time}");
                    await FadeRoutine(0, 1, 0.8f); // 0.5 -> 0.8
                }
                else
                {
                    // No Animation
                    Debug.Log($"[GachaDirector] No Animation for Rarity {maxRarity}. Keeping screen black.");
                    await Task.Delay(2250); // 1.5s -> 2.25s
                }
            }
            else
            {
                Debug.LogWarning("[GachaDirector] No AnimController, fallback wait.");
                await Task.Delay(1500); // 1.0s -> 1.5s
            }

            // 結果表示までの時間調整（10秒ぴったりにする）
            float elapsedTime = Time.time - sequenceStartTime;
            float remainingTime = resultDisplayTime - elapsedTime;
            if (remainingTime > 0)
            {
                Debug.Log($"[GachaDirector] Waiting {remainingTime:F2}s to reach 10s mark...");
                await Task.Delay((int)(remainingTime * 1000));
            }

            // 4. Show Result Screen
            if (resultViewPrefab != null)
            {
                // Instantiate Prefab
                Canvas canvas = GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    GameObject instance = null;
                    if (currentResultView == null)
                    {
                        instance = Instantiate(resultViewPrefab, canvas.transform);
                    }
                    else
                    {
                        Destroy(currentResultView.gameObject);
                        instance = Instantiate(resultViewPrefab, canvas.transform);
                    }
                    
                    currentResultView = instance.GetComponentInChildren<GachaResultView>(true);
                    if (currentResultView == null)
                    {
                         Debug.LogError("[GachaDirector] ResultPrefab does not have GachaResultView component in it or its children!");
                         // Safe failure fallback
                    }

                    // Reset RectTransform to cover screen
                    RectTransform rt = instance.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.anchorMin = Vector2.zero;
                        rt.anchorMax = Vector2.one;
                        rt.offsetMin = Vector2.zero;
                        rt.offsetMax = Vector2.zero;
                        rt.localScale = Vector3.one;
                    }
                    instance.transform.SetAsLastSibling();
                }

                bool resultsClosed = false;
                
                // Reveal Result View (Fade from black)
                Debug.Log($"[GachaDirector] Step 4: Fade Black Out (Show Results) at {Time.time}");
                await FadeRoutine(1, 0, 0.8f); // 0.5 -> 0.8
                
                if (currentResultView != null)
                {
                    currentResultView.ShowResults(results, () => {
                        resultsClosed = true;
                    });
                }
                else
                {
                    resultsClosed = true; // Fallback
                }

                // Wait until Result View is closed
                while (!resultsClosed)
                {
                    await Task.Delay(100);
                }
                
                // --- NEW EXIT SEQUENCE ---
                // Instead of fading screen to black, we restore main UI and fade out the result view directly
                Debug.Log($"[GachaDirector] Step 5: Fade Out Results (Reveal UI) at {Time.time}");

                // 1. Show Main UI behind
                if (uiCanvasGroup != null) uiCanvasGroup.alpha = 1;

                // 2. Fade Out Result View
                if (currentResultView != null)
                {
                    CanvasGroup cg = currentResultView.GetComponent<CanvasGroup>();
                    if (cg == null) cg = currentResultView.gameObject.AddComponent<CanvasGroup>();
                    
                    float fadeDuration = 0.8f;
                    float time = 0;
                    while (time < fadeDuration)
                    {
                        time += Time.deltaTime;
                        cg.alpha = Mathf.Lerp(1, 0, time / fadeDuration);
                        await Task.Yield();
                    }
                    cg.alpha = 0;

                    Destroy(currentResultView.gameObject);
                    currentResultView = null;
                }
            }
            else
            {
                // Fallback exit if no result view
                if (uiCanvasGroup != null) uiCanvasGroup.alpha = 1; 
                await FadeRoutine(1, 0, 0.8f);
            }

            // Ensure Black Overlay is gone (should be 0 alpha from Step 4, but safe to deactivate)
            if (blackOverlay != null) 
            {
                Color c = blackOverlay.color;
                c.a = 0;
                blackOverlay.color = c;
                blackOverlay.gameObject.SetActive(false);
            }

            // ガチャBGMフェードアウト
            await FadeBGMOut();

            // シーンBGMをフェードインで再開
            if (BGMManager.Instance != null)
            {
                BGMManager.Instance.ResumeBGM(sceneBGMFadeDuration);
            }

            Debug.Log($"[GachaDirector] Sequence Complete at {Time.time}");
            isPlaying = false;
            onComplete?.Invoke();
        }

        private async Task FadeRoutine(float startAlpha, float endAlpha, float duration)
        {
            if (blackOverlay == null) 
            {
                Debug.LogError("[GachaDirector] FadeRoutine: BlackOverlay is NULL!");
                return;
            }
            
            float time = 0;
            blackOverlay.gameObject.SetActive(true);
            Color c = blackOverlay.color;
            
            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;
                c.a = Mathf.Lerp(startAlpha, endAlpha, t);
                blackOverlay.color = c;
                await Task.Yield();
            }
            
            c.a = endAlpha;
            blackOverlay.color = c;
            
            if (endAlpha <= 0.01f) blackOverlay.gameObject.SetActive(false);
        }

        private void PlayBGM()
        {
            if (gachaBGM != null && _bgmAudioSource != null)
            {
                _bgmAudioSource.clip = gachaBGM;
                _bgmAudioSource.volume = bgmVolume;
                _bgmAudioSource.Play();
                Debug.Log("[GachaDirector] BGM Started");
            }
        }

        private async Task FadeBGMOut()
        {
            if (_bgmAudioSource == null || !_bgmAudioSource.isPlaying) return;

            float startVolume = _bgmAudioSource.volume;
            float time = 0;

            while (time < bgmFadeOutDuration)
            {
                time += Time.deltaTime;
                _bgmAudioSource.volume = Mathf.Lerp(startVolume, 0, time / bgmFadeOutDuration);
                await Task.Yield();
            }

            _bgmAudioSource.Stop();
            _bgmAudioSource.volume = bgmVolume; // Reset for next time
            Debug.Log("[GachaDirector] BGM Faded Out");
        }
    }
}
