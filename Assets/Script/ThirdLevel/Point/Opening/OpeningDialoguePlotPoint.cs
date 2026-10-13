using System.Collections;
using UnityEngine;

public class OpeningDialoguePlotPoint : PlotPoint
{
    [Tooltip("挂有 DialogueManager 的 GameObject，默认在 Hierarchy 中保持 Inactive")]
    [SerializeField] private GameObject dialogueManagerObj;
    [SerializeField] private CenterGraphicVisualController visualController;
    [SerializeField] private BGMManager openingBGM;

    public override IEnumerator Execute()
    {
        // 0. 画圆前或画圆开始时起播 BGM（画圆伴随音乐，代入感极强）
        if (openingBGM != null)
        {
            openingBGM.PlayBGM(); // 调用你写好的播放方法
        }
        // 确保初始处于未激活状态
        if (dialogueManagerObj != null)
        {
            dialogueManagerObj.SetActive(false);
        }

        // 1. 等待图形顺时针勾勒出来
        if (visualController != null)
        {
            yield return StartCoroutine(visualController.PlayDrawAnimation());
        }

        // 稍留 0.2 秒呼吸感，此时图形已开始优雅慢速自转
        yield return new WaitForSeconds(0.2f);

        // 2. 画圆完毕，激活对话物体启动第一段对白
        if (dialogueManagerObj != null)
        {
            dialogueManagerObj.SetActive(true);

            // 获取组件并监听对白结束
            var dialogueManager = dialogueManagerObj.GetComponent<DialogueManager>();
            if (dialogueManager != null)
            {
                bool isFinished = false;
                dialogueManager.onDialogueEnd.AddListener(() => isFinished = true);

                // 挂起协程，等待 14 句全部点完
                yield return new WaitUntil(() => isFinished);
            }
        }
    }
}