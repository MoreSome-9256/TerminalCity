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
    [Header("此资料属于的房间ID")] // 用于合成失败时增加对应房间的逆恒
    public int room;

    // ========================================================
    // ✨ 新增：多语言拦截属性（UI 显示时统一改用这些）
    // ========================================================

    // 物品特性的本地化，使用原本填写的中文 Name 作为 Key 
    public string LocalizedName => LocalizationHelper.GetText(Name, Name);

    // 事件/详情描述的本地化，内容通常很长，直接拿原中文当 Key
    public string LocalizedEvent => LocalizationHelper.GetText(Event, Event);

    // 关联角色名字的本地化（如果有的话）
    public string LocalizedCharacterName => string.IsNullOrEmpty(linkedCharacterName)
        ? ""
        : LocalizationHelper.GetText($"char_{linkedCharacterName}", linkedCharacterName);
}
