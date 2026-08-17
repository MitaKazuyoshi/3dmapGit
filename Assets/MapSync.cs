using UnityEngine;

public class MapSync : MonoBehaviour
{
    [Header("大マップ用")]
    public RectTransform playerMarker; 
    public RectTransform mapRect;      

    [Header("ミニマップ用")]
    public RectTransform minimapMapImage; 
    public float minimapZoom = 2.0f;

    [Header("3Dのプレイヤー")]
    public Transform player3D;

    [Header("3D空間の境界")]
    public float minX = -100f; 
    public float maxX = 100f;  
    public float minZ = -100f; 
    public float maxZ = 100f;  

    void LateUpdate()
{
    if (player3D == null) return;

    // 1. 3D位置を 0.0 ～ 1.0 の割合に変換
    float xRatio = Mathf.InverseLerp(minX, maxX, player3D.position.x);
    float zRatio = Mathf.InverseLerp(minZ, maxZ, player3D.position.z);

    // --- A. 大マップ（Pivot X:0, Y:0.5 / Marker Anchor X:0, Y:0.5） ---
    if (playerMarker != null && mapRect != null) {
        // Xはアンカーが左(0)なので、そのまま掛ける
        float bigMapX = xRatio * mapRect.rect.width;
        // Yはアンカーが中央(0.5)なので、0.5を引いてから掛ける
        float bigMapY = (zRatio - 0.5f) * mapRect.rect.height;
        
        playerMarker.anchoredPosition = new Vector2(bigMapX, bigMapY);
    }

    // --- B. ミニマップ（中央追従型） ---
    if (minimapMapImage != null) {
        // ミニマップは地図自体がPivot(0.5, 0.5)であることを前提に計算
        float miniX = -(xRatio - 0.5f) * minimapMapImage.rect.width * minimapZoom;
        float miniY = -(zRatio - 0.5f) * minimapMapImage.rect.height * minimapZoom;

        minimapMapImage.anchoredPosition = new Vector2(miniX, miniY);
    }
}
}