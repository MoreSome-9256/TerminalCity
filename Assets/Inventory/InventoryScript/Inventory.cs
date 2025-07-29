using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Inventory", menuName = "Inventory/New Inventory")]
public class Inventory : ScriptableObject
{
    public List<Level1Data> level1List = new List<Level1Data>();
    public List<Level2Data> level2List = new List<Level2Data>();
    public List<Level3Data> level3List = new List<Level3Data>();
}
