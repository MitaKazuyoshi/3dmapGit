using UnityEngine;

public class DeviceSelector : MonoBehaviour
{
    void Awake()
    {
        #if UNITY_EDITOR
        // Unityエディタ上（シミュレーター含む）では、テストのために常に表示
        gameObject.SetActive(true);
        #else
        // ビルドして公開した後は、スマホの時だけ表示
        gameObject.SetActive(Application.isMobilePlatform);
        #endif
    }
}