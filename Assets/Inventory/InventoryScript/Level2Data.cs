using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Second Level", menuName = "Inventory/New Second Level")]
public class Level2Data : Item
{
    public List<Level1Data> requiredLevel1;
    public string Event;
    public int type;

    // ========================================================
    // ✨ 新增：二级资料的多语言拦截属性
    // ========================================================

    // 二级资料关联事件/详情描述的本地化
    public string LocalizedEvent => LocalizationHelper.GetText(Event, Event);
}
