using UnityEngine;

public class WarpManager : MonoBehaviour
{
    public CharacterController playerController;

    // 消したいUIの参照
    public GameObject readButton;
    public GameObject infoPanel;
    
    public GameObject mobileUI;

    public void WarpToPoint(Transform targetPoint)
    {
        if (playerController != null && targetPoint != null)
        {
            // 1. キャラクターを動かす
            playerController.enabled = false;
            playerController.transform.position = targetPoint.position;
            playerController.transform.rotation = targetPoint.rotation;
            playerController.enabled = true;

            // 2. UIを強制的に非表示にする
            if (readButton != null) readButton.SetActive(false);
            if (infoPanel != null) infoPanel.SetActive(false);

            GameObject[] uis = GameObject.FindGameObjectsWithTag("BuildingUI");
        foreach (GameObject ui in uis)
        {
            ui.SetActive(false);
        }
        
            // 3. カーソルをロック状態に戻す
            // モバイルUIが出ていない（PC操作中）時のみロックする設定
            bool isMobileMode = mobileUI != null && mobileUI.activeInHierarchy;
            if (!isMobileMode)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}