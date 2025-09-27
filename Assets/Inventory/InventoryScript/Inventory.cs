using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Inventory", menuName = "Inventory/New Inventory")]
public class Inventory : ScriptableObject
{
    public List<Level1Data> level1List = new List<Level1Data>();
    public List<Level2Data> level2List = new List<Level2Data>();
    public List<Level3Data> level3List = new List<Level3Data>();
    // 背包变更事件（任何物品增删都会触发）
    public event System.Action OnInventoryChanged;
    // 背包初始化时直接拿出已拾取的物品
    public List<Item> GetPickedItems()
    {
        List<Item> picked = new List<Item>();

        foreach (var item in level1List)
        {
            if (item.isPicked)
                picked.Add(item);
        }

        /*foreach (var item in level2List)
        {
            // Level2、3 里可能不需要 isPicked，直接加入
            picked.Add(item);
        }

        foreach (var item in level3List)
        {
            picked.Add(item);
        }*/

        return picked;
    }
    /// <summary>
    /// 添加物品
    /// </summary>
    public void AddItem(Item item)
    {
        if (item == null) return;

        if (item is Level1Data level1)
        {
            if (!level1List.Contains(level1))
                level1List.Add(level1);

            level1.isPicked = true;
        }
        else if (item is Level2Data level2)
        {
            if (!level2List.Contains(level2))
                level2List.Add(level2);
        }
        else if (item is Level3Data level3)
        {
            if (!level3List.Contains(level3))
                level3List.Add(level3);
        }

        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// 移除物品
    /// </summary>
    public void RemoveItem(Item item)
    {
        if (item == null) return;

        if (item is Level1Data level1)
        {
            if (level1List.Contains(level1))
                level1List.Remove(level1);

            //level1.isPicked = false;
        }
        else if (item is Level2Data level2)
        {
            if (level2List.Contains(level2))
                level2List.Remove(level2);
        }
        else if (item is Level3Data level3)
        {
            if (level3List.Contains(level3))
                level3List.Remove(level3);
        }

        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// 清空背包
    /// </summary>
    public void Clear()
    {
        level1List.Clear();
        level2List.Clear();
        level3List.Clear();

        OnInventoryChanged?.Invoke();
    }
}
