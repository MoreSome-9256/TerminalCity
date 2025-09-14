using UnityEngine;
using UnityEngine.Events;

public class InventoryDialogProxy : MonoBehaviour
{
    public DialogTriggerAfterInventory dialogTrigger;
    public SideScreenMove bagController;
    public UnityEvent dialogToTrigger;

    public void TriggerAfterBagClose()
    {
        if (dialogTrigger != null && bagController != null && dialogToTrigger != null)
        {
            dialogTrigger.RequestTriggerAfterInventoryClosed(bagController, dialogToTrigger);
        }
    }
}
