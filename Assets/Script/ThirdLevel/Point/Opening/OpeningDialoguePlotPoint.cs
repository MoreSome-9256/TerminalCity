using System.Collections;
using UnityEngine;

public class OpeningDialoguePlotPoint : PlotPoint
{
    [SerializeField] private DialogueManager dialogueManager;

    public override IEnumerator Execute()
    {
        bool isFinished = false;

        // 监听 DialogueManager 播完
        dialogueManager.onDialogueEnd.AddListener(() => isFinished = true);

        // 挂起协程，静静等待玩家按 Next/空格把 14 句全部点完
        yield return new WaitUntil(() => isFinished);
    }
}