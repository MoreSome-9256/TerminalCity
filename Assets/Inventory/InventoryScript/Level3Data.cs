using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Third Level", menuName = "Inventory/New Third Level")]
public class Level3Data : Item
{
    public int itemType; //记录某级中的具体类别
    public string characterId; // 对应的角色ID

    [Header("对应的演出Prefab")]
    public GameObject sequencePrefab;
}
