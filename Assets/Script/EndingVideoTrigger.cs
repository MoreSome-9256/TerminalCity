using UnityEngine;

public class EndingVideoTrigger : MonoBehaviour
{
    /// <summary>
    /// 直接挂在门或者触发器上，通过 UnityEvent (如 OnClick) 调用
    /// </summary>
    public void PlayVideo()
    {
        if (PrologueVideoEnding.Instance != null)
        {
            PrologueVideoEnding.Instance.PlayEndingVideo();
        }
        else
        {
            Debug.LogError("[EndingVideoTrigger] 场景中没有找到 PrologueVideoEnding 单例，请确认 VideoPanel 是否存在！");
        }
    }
}