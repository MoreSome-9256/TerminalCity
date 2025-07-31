using System.Collections.Generic;
using UnityEngine;

public class ItemCollectTracker : MonoBehaviour
{
    //用于收集多个指定物品才显示对话的情况
    public DialogueManager dialogueManager;
    public List<string> requiredItems;

    private HashSet<string> collectedItems = new HashSet<string>();
    private bool dialogueTriggered = false;

    public void Collect(string itemID)
    {
        collectedItems.Add(itemID);
        CheckAndTriggerDialogue();
    }

    private void CheckAndTriggerDialogue()
    {
        Debug.Log("CheckAndTriggerDialogue");
        if (dialogueTriggered) return;

        bool allCollected = true;
        foreach (string id in requiredItems)
        {
            if (!collectedItems.Contains(id))
            {
                allCollected = false;
                break;
            }
        }

        if (allCollected)
        {
            dialogueManager.gameObject.SetActive(true);
        }
    }
}
