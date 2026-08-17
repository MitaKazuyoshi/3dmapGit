using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.AI;
using StarterAssets;

[System.Serializable]
public class BuildingData {
    public string buildingName;    // 建物名
    public Vector2 mapPosition;    // 2D地図上の位置（？を表示する場所）
    public Transform worldTarget; // 3D空間の目的地（建物の入り口など）
}

public class BuildingListManager : MonoBehaviour {
    public GameObject buttonPrefab; // ボタンのプレハブ
    public RectTransform targetMarker; // 地図上の「？」マーカー
    public List<BuildingData> buildingList; // 39個のデータを入れるリスト
    [Header("UI参照")]
    public GameObject mapSystem;

    [Header("ナビゲーションUI")]
    public TextMeshProUGUI navStatusText;

    [Header("ナビゲーション設定")] // インスペクターで見やすくするため
    public NavMeshAgent playerAgent; // 動かしたいキャラをドラッグする枠
    public GameObject navButton;    // 右側に出したいボタン本体
    private BuildingData selectedData; // 今どの建物が選ばれているかを記録

    [Header("感度設定")]
public float pcSensitivity = 1.0f;
public float mobileSensitivity = 0.2f; 

// この関数を StartNavigation() の中で呼び出す
void ApplyPlatformSensitivity() {
    // スクリプト名が StarterAssets 名前空間にあるので注意
    var controller = playerAgent.GetComponent<StarterAssets.ThirdPersonController>();
    
    if (controller != null) {
        // RotationSpeed は存在しないので、MobileSensitivity か MouseSensitivity を調整
        if (Application.isMobilePlatform) {
            // スマホでの感度をここで直接指定
            controller.MobileSensitivity = 0.5f; 
        } else {
            // パソコンでの感度をここで直接指定
            controller.MouseSensitivity = 1.0f;
        }
    }
}

    private float minX = -203.85f;
    private float maxX = 277.2796f;
    private float minZ = -315.85f;
    private float maxZ = 237.9392f;
    
    void Start() {
        if (navButton != null) navButton.SetActive(false);
        if (targetMarker != null) targetMarker.gameObject.SetActive(false);

        foreach (var data in buildingList) {
            // ボタンを自動生成
            GameObject btn = Instantiate(buttonPrefab, transform);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = data.buildingName;

            // ボタンを押した時の処理
            btn.GetComponent<Button>().onClick.AddListener(() => {
                ShowTargetOnMap(data);
            });
        }
    }

    void ShowTargetOnMap(BuildingData data) {
    if (data == null || data.worldTarget == null) return;
    
    selectedData = data;
    if (targetMarker != null) targetMarker.gameObject.SetActive(true);

    // 1. 地図の画像（2DMap）の RectTransform を確実に取得する
    RectTransform mapRect = null;
    if (mapSystem != null) {
        // 子要素から Image コンポーネントがついたものを探す
        Image img = mapSystem.GetComponentInChildren<Image>();
        if (img != null) {
            mapRect = img.rectTransform;
        }
    }

    if (mapRect == null) {
        Debug.LogError("地図のImageが見つかりません！MapSystemの中にImageがついたオブジェクトがありますか？");
        return;
    }

    // 2. 3D位置を 0.0 ～ 1.0 の割合に変換（minXなどはさっきの f 付きの数値）
    float xRatio = Mathf.InverseLerp(minX, maxX, data.worldTarget.position.x);
    float zRatio = Mathf.InverseLerp(minZ, maxZ, data.worldTarget.position.z);

    // 3. 座標計算（Pivot X:0, Y:0.5 用）
    // mapRect.rect.width を使うことで、Fit In Parent で変わった今のサイズを正確に取れる
    float uiX = xRatio * mapRect.rect.width;
    float uiY = (zRatio - 0.5f) * mapRect.rect.height;

    // 4. マーカーの位置を更新
    targetMarker.anchoredPosition = new Vector2(uiX, uiY);

    if (navButton != null) navButton.SetActive(true);
}

    public void OpenMap() {
        // 1. まず「説明を読むボタン」を全部隠す
        // "BuildingUI" というタグが付いたオブジェクトをすべて探して非表示にする
    GameObject[] uis = GameObject.FindGameObjectsWithTag("BuildingUI");
    foreach (GameObject ui in uis)
    {
        ui.SetActive(false);
    }

        // 2. 地図を表示する
        if (mapSystem != null) {
            mapSystem.SetActive(true);
        } else {
            // もしドラッグし忘れていたら、自動で探す（保険）
            mapSystem = transform.root.Find("MapSystem").gameObject;
            if (mapSystem != null) mapSystem.SetActive(true);
        }
    }
    public void StartNavigation() {
        ApplyPlatformSensitivity();
        //Debug.Log("ナビゲーション開始ボタンが押されました");
        HideNavStatusByTag();
        GameObject[] uis = GameObject.FindGameObjectsWithTag("BuildingUI");
        foreach (GameObject ui in uis){
        ui.SetActive(false);
        }
        if (selectedData != null && playerAgent != null) {
            if (navStatusText != null) {
                navStatusText.text = selectedData.buildingName + " へ自動ナビゲーション中";
                navStatusText.gameObject.SetActive(true);
            }
            // プレイヤーに貼ったブリッジスクリプトを探す
            NavControlBridge bridge = playerAgent.GetComponent<NavControlBridge>();
            
            if (bridge != null) {
                // ブリッジを通じてナビを開始
                bridge.StartNav(selectedData.worldTarget.position);
                
                // マップを閉じる
                GameObject mapSystem = transform.root.Find("MapSystem").gameObject;
                if (mapSystem != null) {
                    mapSystem.SetActive(false);
                }
        }
    }
}

    private void OnEnable() {
    // 直接実行せず、コルーチンを呼び出す
    StartCoroutine(RefreshLayoutSafe());

    if (targetMarker != null) targetMarker.gameObject.SetActive(false);
    if (navButton != null) navButton.SetActive(false);
}

// 安全にレイアウトを更新するための関数
System.Collections.IEnumerator RefreshLayoutSafe() {
    // 1フレームだけ待つ（これで無限ループを防げます）
    yield return null;

    Canvas.ForceUpdateCanvases();
    var rect = GetComponent<RectTransform>();
    if (rect != null) {
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
    }
}

    public void ClearNavStatus() {
        if (navStatusText != null) {
            navStatusText.text = "";
            navStatusText.gameObject.SetActive(false);
        }
    }

    public static void HideNavStatusByTag() {
    GameObject[] statusUIs = GameObject.FindGameObjectsWithTag("NaviUI");
    foreach (GameObject ui in statusUIs) {
        if (ui != null) ui.SetActive(false);
    }
}

}
