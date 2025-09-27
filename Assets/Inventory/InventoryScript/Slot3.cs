using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEditor;

public class Slot3 : MonoBehaviour,IPointerClickHandler
{
    public Item slotItem;
    public Image slotImage;
    public TMP_Text slotName;
    public Image background;
    public Sprite selectedSprite;
    public Sprite deselectedSprite;
    public bool isSelected = false;
    public void OnPointerClick(PointerEventData eventData)
    {
        isSelected = !isSelected;
        UpdateSelectionVisual();

        // 通知 InventoryManager 管理选中列表
        InventoryManager5.instance.ToggleSelect(this);
    }

    public void UpdateSelectionVisual()
    {
        if (background == null) return;

        if (isSelected)
            background.sprite = selectedSprite;
        else
            background.sprite = deselectedSprite;
    }

    public void Deselect()
    {
        isSelected = false;
        UpdateSelectionVisual();
    }
}
