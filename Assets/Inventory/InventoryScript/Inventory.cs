using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Inventory", menuName = "Inventory/New Inventory")]
public class Inventory : ScriptableObject
{
    public List<Level1Data> level1List = new List<Level1Data>();
    public List<Level2Data> level2List = new List<Level2Data>();
    public List<Level3Data> level3List = new List<Level3Data>();

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
}
