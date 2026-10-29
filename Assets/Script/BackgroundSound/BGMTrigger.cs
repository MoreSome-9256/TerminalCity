using UnityEngine;

public class BGMTrigger : MonoBehaviour
{
    [Header("绑定的曲目数据 (ScriptableObject)")]
    [SerializeField] private BGMTrackData trackToPlay;

    [Header("切歌平滑淡入淡出时长(秒)")]
    [SerializeField] private float defaultFadeDuration = 1.0f;

    /// <summary>
    /// 播放配置的曲目（Event List 常用）
    /// </summary>
    public void PlayThisTrack()
    {
        if (BGMManager.Instance != null && trackToPlay != null)
        {
            BGMManager.Instance.SwitchBGM(trackToPlay, defaultFadeDuration);
        }
    }

    /// <summary>
    /// 临时自定义淡入时长的播放
    /// </summary>
    public void PlayThisTrackWithFade(float fadeDuration)
    {
        if (BGMManager.Instance != null && trackToPlay != null)
        {
            BGMManager.Instance.SwitchBGM(trackToPlay, fadeDuration);
        }
    }

    /// <summary>
    /// 停止所有播放并淡出
    /// </summary>
    public void FadeOutCurrentBGM()
    {
        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.FadeOutBGM(defaultFadeDuration);
        }
    }

    // --- 代理基础控制 ---
    public void PauseBGM() => BGMManager.Instance?.PauseBGM();
    public void ResumeBGM() => BGMManager.Instance?.ResumeBGM();
    public void StopBGM() => BGMManager.Instance?.StopBGM();

    // --- 代理分轨控制 ---
    public void EnableTrack(string trackName) => BGMManager.Instance?.EnableTrack(trackName);
    public void DisableTrack(string trackName) => BGMManager.Instance?.DisableTrack(trackName);
}