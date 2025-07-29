using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Slot : MonoBehaviour, IPointerClickHandler
{
    public Item slotItem;
    public Image slotImage;
    public TMP_Text slotName;
    public TMP_Text slotTrait;
    public TMP_Text slotSynopsis;
    public bool canOpen = true;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (canOpen)
        {
            // 获取背包管理器并调用其OnItemClicked方法
            InventoryManager inventoryManager = FindObjectOfType<InventoryManager>();
            InventoryManager3 inventoryManager3 = FindObjectOfType<InventoryManager3>();
            if (inventoryManager != null)
            {
                inventoryManager.OnItemClicked(gameObject);
            }
            if (inventoryManager3 != null)
            {
                inventoryManager3.OnItemClicked(gameObject);
            }
        }
        else
        {
            //在工具背包中的实现方法
            InventoryManager4 inventoryManager = FindObjectOfType<InventoryManager4>();
            inventoryManager.OnToolItemClicked(slotItem.itemNum);
        }
    }
}
