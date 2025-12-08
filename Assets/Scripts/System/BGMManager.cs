using System.Collections;
using UnityEngine;

/// <summary>
/// ゲーム全体で使用可能なBGM管理システム。
/// シングルトンパターンでシーン間を跨いで動作します。
/// </summary>
public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance { get; private set; }

    [Header("Settings")]
    [Tooltip("デフォルトのBGMボリューム (0.0 - 1.0)")]
    [SerializeField, Range(0f, 1f)] private float defaultVolume = 0.7f;
    
    [Tooltip("デフォルトのフェード時間（秒）")]
    [SerializeField] private float defaultFadeDuration = 0.5f;

    [Header("Scene BGM (Optional)")]
    [Tooltip("このシーンで自動再生するBGM")]
    [SerializeField] private AudioClip sceneBGM;
    
    [Tooltip("シーン開始時に自動再生するか")]
    [SerializeField] private bool playOnStart = true;

    private AudioSource _audioSource;
    private Coroutine _fadeCoroutine;
    private float _targetVolume;
    private bool _isPaused = false;

    private void Awake()
    {
        // シングルトンパターン
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // AudioSourceの設定
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
            _audioSource.playOnAwake = false;
            _audioSource.loop = true;
            _targetVolume = defaultVolume;
        }
        else
        {
            // 既にインスタンスが存在する場合、このシーンのBGMを引き継ぐ
            if (sceneBGM != null && playOnStart)
            {
                Instance.PlayBGM(sceneBGM, defaultVolume, defaultFadeDuration);
            }
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // シーンBGMの自動再生
        if (sceneBGM != null && playOnStart)
        {
            PlayBGM(sceneBGM, defaultVolume, 0f); // 初回はフェードなし
        }
    }

    /// <summary>
    /// BGMを再生します。
    /// </summary>
    /// <param name="clip">再生するAudioClip</param>
    /// <param name="volume">ボリューム (0.0 - 1.0)</param>
    /// <param name="fadeDuration">フェードイン時間（秒）</param>
    public void PlayBGM(AudioClip clip, float volume = -1f, float fadeDuration = -1f)
    {
        if (clip == null) return;
        
        if (volume < 0) volume = defaultVolume;
        if (fadeDuration < 0) fadeDuration = defaultFadeDuration;
        
        _targetVolume = volume;
        _isPaused = false;
        
        // 別のBGMが再生中の場合はクロスフェード
        if (_audioSource.isPlaying && _audioSource.clip != clip)
        {
            StopFadeCoroutine();
            _fadeCoroutine = StartCoroutine(CrossFadeRoutine(clip, volume, fadeDuration));
        }
        else
        {
            _audioSource.clip = clip;
            if (fadeDuration > 0)
            {
                _audioSource.volume = 0;
                _audioSource.Play();
                StopFadeCoroutine();
                _fadeCoroutine = StartCoroutine(FadeRoutine(volume, fadeDuration));
            }
            else
            {
                _audioSource.volume = volume;
                _audioSource.Play();
            }
        }
        
        Debug.Log($"[BGMManager] Playing: {clip.name}");
    }

    /// <summary>
    /// BGMをフェードアウトして停止します。
    /// </summary>
    /// <param name="fadeDuration">フェードアウト時間（秒）</param>
    public void StopBGM(float fadeDuration = -1f)
    {
        if (fadeDuration < 0) fadeDuration = defaultFadeDuration;
        
        if (!_audioSource.isPlaying) return;
        
        StopFadeCoroutine();
        _fadeCoroutine = StartCoroutine(FadeOutAndStopRoutine(fadeDuration));
    }

    /// <summary>
    /// BGMを一時停止します（フェードアウト付き）。
    /// </summary>
    /// <param name="fadeDuration">フェードアウト時間（秒）</param>
    public void PauseBGM(float fadeDuration = -1f)
    {
        if (fadeDuration < 0) fadeDuration = defaultFadeDuration;
        
        if (!_audioSource.isPlaying || _isPaused) return;
        
        _isPaused = true;
        StopFadeCoroutine();
        _fadeCoroutine = StartCoroutine(FadeOutAndPauseRoutine(fadeDuration));
    }

    /// <summary>
    /// 一時停止したBGMを再開します（フェードイン付き）。
    /// </summary>
    /// <param name="fadeDuration">フェードイン時間（秒）</param>
    public void ResumeBGM(float fadeDuration = -1f)
    {
        if (fadeDuration < 0) fadeDuration = defaultFadeDuration;
        
        if (!_isPaused) return;
        
        _isPaused = false;
        _audioSource.UnPause();
        StopFadeCoroutine();
        _fadeCoroutine = StartCoroutine(FadeRoutine(_targetVolume, fadeDuration));
    }

    /// <summary>
    /// ボリュームを変更します（フェード付き）。
    /// </summary>
    public void SetVolume(float volume, float fadeDuration = -1f)
    {
        if (fadeDuration < 0) fadeDuration = defaultFadeDuration;
        
        _targetVolume = Mathf.Clamp01(volume);
        StopFadeCoroutine();
        _fadeCoroutine = StartCoroutine(FadeRoutine(_targetVolume, fadeDuration));
    }

    /// <summary>
    /// 現在BGMが再生中かどうか
    /// </summary>
    public bool IsPlaying => _audioSource != null && _audioSource.isPlaying && !_isPaused;

    private void StopFadeCoroutine()
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = null;
        }
    }

    private IEnumerator FadeRoutine(float targetVolume, float duration)
    {
        float startVolume = _audioSource.volume;
        float time = 0;
        
        while (time < duration)
        {
            time += Time.deltaTime;
            _audioSource.volume = Mathf.Lerp(startVolume, targetVolume, time / duration);
            yield return null;
        }
        
        _audioSource.volume = targetVolume;
        _fadeCoroutine = null;
    }

    private IEnumerator FadeOutAndStopRoutine(float duration)
    {
        yield return FadeRoutine(0, duration);
        _audioSource.Stop();
        Debug.Log("[BGMManager] BGM Stopped");
    }

    private IEnumerator FadeOutAndPauseRoutine(float duration)
    {
        yield return FadeRoutine(0, duration);
        _audioSource.Pause();
        Debug.Log("[BGMManager] BGM Paused");
    }

    private IEnumerator CrossFadeRoutine(AudioClip newClip, float targetVolume, float duration)
    {
        // フェードアウト
        yield return FadeRoutine(0, duration / 2);
        
        // クリップ切り替え
        _audioSource.clip = newClip;
        _audioSource.Play();
        
        // フェードイン
        yield return FadeRoutine(targetVolume, duration / 2);
    }
}
