using UnityEngine;

public class DoorController : MonoBehaviour
{
    public bool isLocked = true;

    public GameObject thisObject;
    public GameObject inventoryUI;
    public GameObject preview;
    public GameObject message;
    public SimpleDialogue dialogueWhenLocked;
    public SimpleDialogue dialogueWhenTrue;
    public SimpleDialogue dialogueWhenFalse;

    [Header("需要的物品编号")]
    public int requiredItemNum;

    public void OnDoorClicked()
    {
        if (isLocked)
        {
            Debug.Log("门锁着 → 弹背包 UI + 播放对话");
            SidePanelManager.Instance.ShowPanel(inventoryUI);

            InventoryManager4 inventoryManager = FindObjectOfType<InventoryManager4>();
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
            inventoryUI.SetActive(false);
            thisObject.SetActive(false);
            dialogueWhenTrue?.StartDialogue();
        }
        else
        {
            Debug.Log("不是这把钥匙！");
            // 可播放“不是这个东西”的对话
            dialogueWhenFalse?.StartDialogue();
        }
    }
    public void UnlockDoor()
    {
        isLocked = false;
        Debug.Log("门已解锁！");
    }
}
