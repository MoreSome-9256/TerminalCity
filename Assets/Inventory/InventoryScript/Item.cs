using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Item : ScriptableObject
{
    public int itemNum;
    public string itemName;
    public Sprite itemImage;

    [Header("多图（可为空）")]
    public List<Sprite> itemImages = new List<Sprite>();

    public int readTime=0;
    public bool isDecoded;
    public string itemInfo;
    public TextAsset textFile;
    [Header("词典资料")]
    public MaterialData materialData; // 每个物品独立的词典信息

    [Header("时间轴关联")]
    [Tooltip("如果拾取该物品需要解锁时间轴事件，请填入对应的事件ID (如 EV_001)")]
    public string timelineEventId;

    // ========================================================
    // 基础物品的多语言拦截属性（子类也会自动继承）
    // ========================================================

    // 物品基础名称的本地化
    public string LocalizedItemName => LocalizationHelper.GetText(itemName, itemName);

    // 物品简要介绍/描述信息的本地化
    public string LocalizedItemInfo => LocalizationHelper.GetText(itemInfo, itemInfo);
}
