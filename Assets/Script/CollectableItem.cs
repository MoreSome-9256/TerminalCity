using UnityEngine;

public class CollectableItem : MonoBehaviour
{
    public string itemID;
    public ItemCollectTracker tracker;

    public void PickUp()
    {
        Debug.Log("Picked up item: " + itemID);
        tracker.Collect(itemID);
        Destroy(gameObject);
    }
}
