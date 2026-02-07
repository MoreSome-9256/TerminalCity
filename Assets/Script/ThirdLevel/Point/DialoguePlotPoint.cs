using System.Collections;
using UnityEngine;

/// <summary>
/// PlotPoint 桥接器，只负责触发 prefab 内的 DialogueManager
/// </summary>
public class DialoguePlotPoint : PlotPoint
{
    [Header("直接拖拽 prefab 内的 DialogueManager")]
    public DialogueManager dialogueManager;

    [Header("Optional CG Root")]
    public GameObject cgRoot;   // 可选
    public bool showCGOnStart = true;

    public override IEnumerator Execute()
    {
        // 进入 CG 界面（如果有）
        if (showCGOnStart && cgRoot != null)
        {
            cgRoot.SetActive(true);
        }

        if (dialogueManager == null)
        {
            Debug.LogError("DialoguePlotPoint: DialogueManager 未绑定");
            yield break;
        }

        bool finished = false;
        dialogueManager.onDialogueEnd.RemoveAllListeners();
        dialogueManager.onDialogueEnd.AddListener(() => finished = true);

        dialogueManager.gameObject.SetActive(true);
        dialogueManager.StartDialogue();

        // 等待对话结束
        yield return new WaitUntil(() => finished);
    }
}
