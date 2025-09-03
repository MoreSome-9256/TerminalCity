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
}
