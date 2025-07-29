using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Item : ScriptableObject
{
    public int itemNum;
    public string itemName;
    public Sprite itemImage;
    public int readTime=0;
    public bool isDecoded;
    public string itemInfo;
    public TextAsset textFile;
}
