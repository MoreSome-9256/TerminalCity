using UnityEngine;

public class MainBGMStarter : MonoBehaviour
{
    [Tooltip("主关卡的 BGM 触发器")]
    [SerializeField] private BGMTrigger mainBgmTrigger;

    private void OnEnable()
    {
        // 订阅开场结束的静态事件
        PiGlitchAndCollapsePlotPoint.OnOpeningFinished += HandleOpeningFinished;
    }

    private void OnDisable()
    {
        // 场景切换或销毁时务必注销，防止内存泄漏
        PiGlitchAndCollapsePlotPoint.OnOpeningFinished -= HandleOpeningFinished;
    }

    private void HandleOpeningFinished()
    {
        Debug.Log("接收到开场结束信号，准备播放正式游戏 BGM！");
        if (mainBgmTrigger != null)
        {
            mainBgmTrigger.PlayThisTrack();
        }
    }
}