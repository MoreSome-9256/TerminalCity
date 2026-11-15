using UnityEngine;

public class DoorController : MonoBehaviour
{
    public bool isLocked = true;

    [Header("内部物体引用（不填则默认为自身）")]
    public GameObject thisObject;

    [Header("UI 引用（运行时由 UIManager 自动注入）")]
    public GameObject inventoryUI;
    public GameObject preview;
    public GameObject message;

    [Header("对话配置")]
    public DialogForPrefab dialogueWhenLocked;
    public DialogForPrefab dialogueWhenTrue;
    public DialogForPrefab dialogueWhenFalse;

    [Header("需要的物品编号")]
    public int requiredItemNum;

    private void Awake()
    {
        if (thisObject == null)
        {
            thisObject = gameObject;
        }

        if (UIManager.Instance != null)
        {
            if (inventoryUI == null)
                inventoryUI = UIManager.Instance.doorInventoryUI != null
                    ? UIManager.Instance.doorInventoryUI
                    : UIManager.Instance.firstLevel;

            if (preview == null)
                preview = UIManager.Instance.previewArea;

            if (message == null)
                message = UIManager.Instance.messagePanel;
        }
        else
        {
            Debug.LogError("[DoorController] UIManager 未找到，请确保场景中有 UIManager！");
        }
    }

    public void OnDoorClicked()
    {
        if (isLocked)
        {
            Debug.Log("门锁着 → 弹背包 UI + 播放对话");
            if (inventoryUI != null && SidePanelManager.Instance != null)
            {
                SidePanelManager.Instance.ShowPanel(inventoryUI);
            }

            InventoryManager4 inventoryManager = FindObjectOfType<InventoryManager4>(true);
            if (inventoryManager != null)
            {
                inventoryManager.SetActiveDoor(this);
                inventoryManager.doorController = this;
            }

            dialogueWhenLocked?.StartDialogue();
        }
        else
        {
            Debug.Log("门已解锁 → 进入房间");
        }
    }

    public void TryUseItem(int itemNum)
    {
        Debug.Log("尝试用物品编号: " + itemNum);

        if (itemNum == requiredItemNum)
        {
            Debug.Log("钥匙正确！开门！");
            UnlockDoor();
            if (inventoryUI != null) inventoryUI.SetActive(false);
            if (thisObject != null) thisObject.SetActive(false);
            dialogueWhenTrue?.StartDialogue();
        }
        else
        {
            Debug.Log("不是这把钥匙！");
            dialogueWhenFalse?.StartDialogue();
        }
    }

    public void UnlockDoor()
    {
        isLocked = false;
        Debug.Log("门已解锁！");
    }
}