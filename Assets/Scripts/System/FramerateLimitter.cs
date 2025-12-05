using UnityEngine;

public class FramerateLimitter : MonoBehaviour
{
    private void Awake()
    {
        // 60FPSに制限
        Application.targetFrameRate = 60;
    }
}
