using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CursorEffectManager : MonoBehaviour
{
    public static CursorEffectManager Instance { get; private set; }

    [Header("Settings")]
    [Tooltip("クリック時に生成するプレハブ")]
    [SerializeField] private GameObject cursorEffectPrefab;
    [SerializeField] private GameObject cursorEffectCirclePrefab;

    [Tooltip("エフェクトが破棄（非アクティブ化）されるまでの時間（秒）")]
    [SerializeField] private float effectDuration = 1.0f;

    [Tooltip("カメラからエフェクトを生成するZ距離（2D/UIオーバーレイ用）")]
    [SerializeField] private float spawnDistance = 10.0f;

    [Tooltip("同時に表示できるエフェクトの最大数")]
    [SerializeField] private int maxPoolSize = 50;

    [Header("Sound Settings")]
    [Tooltip("クリック時に再生するサウンド")]
    [SerializeField] private AudioClip clickSound;
    
    [Tooltip("クリック音のボリューム (0.0 - 1.0)")]
    [SerializeField, Range(0f, 1f)] private float clickVolume = 0.5f;

    private Camera _mainCamera;
    private AudioSource _audioSource;
    
    // オブジェクトプール
    private Queue<GameObject> _effectPool = new Queue<GameObject>();
    private Queue<GameObject> _circlePool = new Queue<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // AudioSource の設定
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
            _audioSource.playOnAwake = false;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        UpdateCameraReference();
        CheckRecursivePrefab(cursorEffectPrefab);
        CheckRecursivePrefab(cursorEffectCirclePrefab);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateCameraReference();
    }

    private void UpdateCameraReference()
    {
        _mainCamera = Camera.main;
        if (_mainCamera == null)
        {
            // シーンロード直後はまだタグ付きカメラがない場合もあるため、Warningは控えめにするか、
            // 必要なシーンでのみLogを出すなどが良いが、ここではデバッグ用に残す
            // Debug.LogWarning("CursorEffectManager: MainCamera not found in this scene.");
        }
    }

    private void Update()
    {
        // 左クリックを検知
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            SpawnEffect();
        }
    }

    private void CheckRecursivePrefab(GameObject prefab)
    {
        if (prefab != null && prefab.GetComponent<CursorEffectManager>() != null)
        {
            Debug.LogError($"CursorEffectManager: 割り当てられたPrefab '{prefab.name}' に CursorEffectManager がアタッチされています！これは無限ループを引き起こします。Prefabからこのスクリプトを削除してください。");
        }
    }

    private void SpawnEffect()
    {
        if (cursorEffectCirclePrefab == null && cursorEffectPrefab == null)
        {
            Debug.LogWarning("CursorEffectManager: CursorEffectPrefabが割り当てられていません。");
            return;
        }

        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
            if (_mainCamera == null) return;
        }

        // マウスのスクリーン座標を取得
        Vector2 mousePos = Mouse.current.position.ReadValue();
        
        // ワールド座標に変換
        Vector3 spawnPosition = _mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, spawnDistance));

        // エフェクトを表示
        SpawnFromPool(cursorEffectPrefab, _effectPool, spawnPosition);
        SpawnFromPool(cursorEffectCirclePrefab, _circlePool, spawnPosition);
        
        // クリック音を再生
        PlayClickSound();
    }

    private void SpawnFromPool(GameObject prefab, Queue<GameObject> pool, Vector3 position)
    {
        if (prefab == null) return;

        GameObject obj = null;

        // プールから取得可能なオブジェクトを探す
        // 非アクティブなものが見つかるまで取り出す、または新規作成
        // ここでは単純なプール管理として、Queueの先頭を確認します
        
        if (pool.Count > 0 && pool.Count >= maxPoolSize)
        {
             // 最大数に達している場合、最も古いものを再利用する（先頭から取り出し）
             obj = pool.Dequeue();
        }
        else if (pool.Count > 0 && !pool.Peek().activeSelf)
        {
            // 非アクティブなものがあれば再利用
            obj = pool.Dequeue();
        }

        // まだオブジェクトがない場合は新規作成（上限未満なら）
        if (obj == null && (pool.Count + 1 + CountActive(pool) <= maxPoolSize * 2)) // 簡易的なチェック
        {
             obj = Instantiate(prefab);
             // Destroyされないようにする
             DontDestroyOnLoad(obj); // 必要であればシーン遷移対応。今回はGameObjectのライフサイクルに任せるため不要かも？
             // 生成したオブジェクトはルートに置くか整理する
             obj.transform.SetParent(transform); 
        }
        
        // オブジェクトがnullの場合（上限に達してないがプールも空などの場合）、Instantiate
        if (obj == null)
        {
             obj = Instantiate(prefab, transform);
        }

        // 初期化
        obj.transform.position = position;
        obj.transform.rotation = Quaternion.identity;
        obj.SetActive(true);
        
        // パーティクルシステムの再生をリセット
        ParticleSystem[] particles = obj.GetComponentsInChildren<ParticleSystem>();
        foreach (var p in particles)
        {
            p.Stop();
            p.Play();
        }

        // 使用後にキューの末尾に戻す（アクティブなまま戻し、後で使用時にリセット）
        pool.Enqueue(obj);

        // 一定時間後に非アクティブ化するコルーチン
        StartCoroutine(DeactivateAfterTime(obj, effectDuration));
    }

    private int CountActive(Queue<GameObject> pool)
    {
        // Debug用途、実際にはCountで管理
        return pool.Count;
    }

    private IEnumerator DeactivateAfterTime(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null)
        {
            obj.SetActive(false);
        }
    }

    private void PlayClickSound()
    {
        if (clickSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(clickSound, clickVolume);
        }
    }
}
