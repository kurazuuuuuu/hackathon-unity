using UnityEngine;
using UnityEngine.InputSystem;

public class CursorEffectManager : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("クリック時に生成するプレハブ")]
    [SerializeField] private GameObject cursorEffectPrefab;
    [SerializeField] private GameObject cursorEffectCirclePrefab;

    [Tooltip("エフェクトが破棄されるまでの時間（秒）")]
    [SerializeField] private float effectDuration = 1.0f;

    [Tooltip("カメラからエフェクトを生成するZ距離（2D/UIオーバーレイ用）")]
    [SerializeField] private float spawnDistance = 10.0f;

    private Camera _mainCamera;

    private void Start()
    {
        _mainCamera = Camera.main;
        if (_mainCamera == null)
        {
            Debug.LogError("CursorEffectManager: MainCameraが見つかりません！");
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

    private void SpawnEffect()
    {
        if (cursorEffectCirclePrefab == null && cursorEffectPrefab == null)
        {
            Debug.LogWarning("CursorEffectManager: CursorEffectPrefabが割り当てられていません。");
            return;
        }

        if (_mainCamera == null) return;

        // マウスのスクリーン座標を取得
        Vector2 mousePos = Mouse.current.position.ReadValue();
        
        // ワールド座標に変換
        // カメラからの固定距離(Z)を設定して表示されるようにする
        Vector3 spawnPosition = _mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, spawnDistance));

        // エフェクトを生成
        GameObject effectInstance = Instantiate(cursorEffectPrefab, spawnPosition, Quaternion.identity);
        GameObject effectInstanceCircle = Instantiate(cursorEffectCirclePrefab, spawnPosition, Quaternion.identity);

        // 指定時間後に破棄
        Destroy(effectInstance, effectDuration);
        Destroy(effectInstanceCircle, effectDuration);
    }
}
