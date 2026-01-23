using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Second Level", menuName = "Inventory/New Second Level")]
public class Level2Data : Item
{
    public List<Level1Data> requiredLevel1;
    public string Event;
}
