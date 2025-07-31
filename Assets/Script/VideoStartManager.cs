using UnityEngine.Video;
using UnityEngine;
using System.IO;

public class VideoStartManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public AudioSource audioSource;
    public ObjectFadeController fadeController;

    void Start()
    {
        string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, "video1.mp4");
        videoPath = videoPath.Replace("\\", "/");  // Windows 路径修正
        videoPlayer.url = "file:///" + videoPath;

        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.EnableAudioTrack(0, true);
        videoPlayer.SetTargetAudioSource(0, audioSource);

        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.Prepare();  // 异步加载，准备后触发事件
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        vp.Play();  // 视频已加载完成，开始播放
        fadeController.Fade(false);  // 现在才淡出黑幕
    }
}
