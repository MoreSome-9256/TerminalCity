using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class DialogueEventList
{
    public int dialogueID;

    public UnityEvent onDialogueEnd;
}

public class DialogueEventDatabase : MonoBehaviour
{
    [Header("对话事件映射表")]
    public List<DialogueEventList> eventLists = new List<DialogueEventList>();

    /// <summary>
    /// 触发指定对话ID的事件
    /// </summary>
    public void TriggerEvents(int dialogueID)
    {
        DialogueEventList entry = eventLists.Find(e => e.dialogueID == dialogueID);
        if (entry != null && entry.onDialogueEnd != null)
        {
            Debug.Log($"[DialogueEventDatabase] 触发事件 (DialogueID={dialogueID})");
            entry.onDialogueEnd.Invoke();
        }
        else
        {
            Debug.Log($"[DialogueEventDatabase] 对话 {dialogueID} 没有关联事件");
        }
    }
}
