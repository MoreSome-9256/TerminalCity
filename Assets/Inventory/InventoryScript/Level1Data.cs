using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New First Level", menuName = "Inventory/New First Level")]
public class Level1Data : Item
{
    public int itemType; //记录某级中的具体类别
    public string Name; // 物品的特性
    public string Time;
    public string Event;
    public bool showDialogueOnPickup = true;
    public bool isPicked = false;
    public bool isImportant = false;

    [Header("如果此资料关联角色，则填角色名字，否则留空")]
    public string linkedCharacterName;
    [Header("此资料属于的房间ID")]
    public int room;
}
