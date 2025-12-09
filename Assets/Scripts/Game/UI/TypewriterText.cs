using System.Collections;
using UnityEngine;
using TMPro;

namespace Game.UI
{
    /// <summary>
    /// タイプライターエフェクト付きテキスト表示コンポーネント
    /// 1文字ずつテキストを表示し、音声を再生する
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TypewriterText : MonoBehaviour
    {
        [Header("Typewriter Settings")]
        [SerializeField] private float characterDelay = 0.05f; // 1文字あたりの表示間隔（秒）
        [SerializeField] private bool playSound = true;
        
        [Header("Audio")]
        [SerializeField] private AudioClip typeSound; // 文字表示時の効果音
        [SerializeField] private AudioSource audioSource;
        [SerializeField] [Range(0f, 1f)] private float volume = 0.5f;
        [SerializeField] private int soundSkipCount = 2; // N文字ごとに音を鳴らす（パフォーマンス用）
        
        [Header("Behavior")]
        [SerializeField] private bool autoStart = false;
        [SerializeField] private bool skipOnClick = true; // クリックでスキップ可能か
        
        private TextMeshProUGUI textComponent;
        private Coroutine typewriterCoroutine;
        private string fullText;
        private bool isTyping = false;
        private global::System.Action onComplete;
        
        public bool IsTyping => isTyping;
        
        private void Awake()
        {
            textComponent = GetComponent<TextMeshProUGUI>();
            
            // AudioSourceが設定されていない場合は自動生成
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }
            
            audioSource.playOnAwake = false;
            audioSource.volume = volume;
        }
        
        /// <summary>
        /// テキストをタイプライター効果で表示
        /// </summary>
        public void ShowText(string text, global::System.Action onCompleteCallback = null)
        {
            if (textComponent == null) return;
            
            fullText = text;
            onComplete = onCompleteCallback;
            
            // 既存のコルーチンを停止
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
            }
            
            typewriterCoroutine = StartCoroutine(TypewriterEffect());
        }
        
        /// <summary>
        /// 即座に全テキストを表示
        /// </summary>
        public void ShowTextImmediately(string text)
        {
            if (textComponent == null) return;
            
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
            }
            
            fullText = text;
            textComponent.text = text;
            isTyping = false;
        }
        
        /// <summary>
        /// 現在のタイピングをスキップして全表示
        /// </summary>
        public void Skip()
        {
            if (!isTyping) return;
            
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
            }
            
            textComponent.text = fullText;
            isTyping = false;
            onComplete?.Invoke();
        }
        
        private IEnumerator TypewriterEffect()
        {
            isTyping = true;
            textComponent.text = "";
            
            int charCount = 0;
            foreach (char c in fullText)
            {
                textComponent.text += c;
                charCount++;
                
                // 音声再生（スキップカウントに応じて）
                if (playSound && typeSound != null && charCount % soundSkipCount == 0)
                {
                    // スペースや句読点では音を鳴らさない（オプション）
                    if (!char.IsWhiteSpace(c))
                    {
                        audioSource.PlayOneShot(typeSound, volume);
                    }
                }
                
                yield return new WaitForSeconds(characterDelay);
            }
            
            isTyping = false;
            onComplete?.Invoke();
        }
        
        private void Update()
        {
            // クリックでスキップ（新しいInput System対応）
            if (skipOnClick && isTyping)
            {
                bool clicked = false;
                
                // 新しいInput System
                #if ENABLE_INPUT_SYSTEM
                if (UnityEngine.InputSystem.Mouse.current != null && 
                    UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
                {
                    clicked = true;
                }
                #else
                // レガシーInput
                if (Input.GetMouseButtonDown(0))
                {
                    clicked = true;
                }
                #endif
                
                if (clicked)
                {
                    Skip();
                }
            }
        }
        
        /// <summary>
        /// Inspector用: 文字表示間隔を設定
        /// </summary>
        public void SetCharacterDelay(float delay)
        {
            characterDelay = delay;
        }
        
        /// <summary>
        /// Inspector用: 効果音を設定
        /// </summary>
        public void SetTypeSound(AudioClip clip)
        {
            typeSound = clip;
        }
    }
}
