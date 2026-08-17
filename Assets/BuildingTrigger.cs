using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BuildingTrigger : MonoBehaviour
{
    // 現在プレイヤーが範囲内にいる「はてな」を管理する名簿
    private static List<BuildingTrigger> activeTriggers = new List<BuildingTrigger>();

    [Header("表示する内容")]
    public string buildingName;
    [TextArea] public string description;

    [Header("参照するUI要素")]
    public GameObject readButton;
    public TextMeshProUGUI nameLabel;
    public Button actualButtonComponent;
    public GameObject infoPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI contentText;

    [Header("初期設定")]
    public bool showOnStart = false; // インスペクターでこれにチェックを入れた建物が最初に出る

    private bool isInside = false;

    void Start()
    {
        // 1. リストに自分を登録（これがないと後で消せなくなるので必須）
        if (!activeTriggers.Contains(this)) activeTriggers.Add(this);

        // 2. チェックが入っていたら即座に表示
        if (showOnStart)
        {
            ShowInitialPanel();
        }
    }

    private void ShowInitialPanel()
    {
        ShowInfoPanel();
        
        // ブラウザで操作できるようにマウスを解放
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // --- 全体のUIを制御する命令 ---
    public static void HideAllUI() {
        foreach (var trigger in activeTriggers) {
            if (trigger.readButton != null) trigger.readButton.SetActive(false);
            if (trigger.infoPanel != null) trigger.infoPanel.SetActive(false);
            if (trigger.nameLabel != null) trigger.nameLabel.gameObject.SetActive(false);
        }
    }
    public static void ShowAllUI() {
        foreach (var t in activeTriggers) t.SetVisibility(true);
    }

    private void SetVisibility(bool visible) {
        bool shouldShow = visible && !infoPanel.activeSelf;
        // パネルが開いている時はボタンは出さないなど、状況に合わせて制御
        if (readButton != null) readButton.SetActive(shouldShow);
        
        if (nameLabel != null) {
            nameLabel.text = buildingName; // ここで名前を更新
            nameLabel.gameObject.SetActive(shouldShow); 
        }

        if (visible) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        } else {
            // 自動ナビ中は邪魔なのでパネルも消す
            if (infoPanel != null) infoPanel.SetActive(false);
        }
    }

    // --- トリガー判定 ---
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            isInside = true;
            // 名簿に自分を追加する
            if (!activeTriggers.Contains(this)) activeTriggers.Add(this);

            SetVisibility(true);

            if (actualButtonComponent != null) {
                actualButtonComponent.onClick.RemoveListener(OnReadButtonClicked);
                actualButtonComponent.onClick.AddListener(OnReadButtonClicked);
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            isInside = false;
            // 名簿から自分を消す
            if (activeTriggers.Contains(this)) activeTriggers.Remove(this);
            
            SetVisibility(false);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void OnReadButtonClicked() {
        if (isInside) ShowInfoPanel();
    }

    private void ShowInfoPanel() {
        if (titleText != null) titleText.text = buildingName;
        if (contentText != null) contentText.text = description;
        if (infoPanel != null) infoPanel.SetActive(true);
        if (readButton != null) readButton.SetActive(false);
        if (nameLabel != null) nameLabel.gameObject.SetActive(false);
    }
}