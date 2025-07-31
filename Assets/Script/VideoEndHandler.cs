using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VideoEndHandler : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string nextSceneName = "MainMenu"; // 播放后跳转的场景名

    void Start()
    {
        // 视频播放结束时触发事件
        videoPlayer.loopPointReached += OnVideoEnd;

        // 从 StreamingAssets 读取视频路径
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, "ending.mp4");
        videoPlayer.url = path;
        videoPlayer.Play();
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        Debug.Log("视频播放完毕，跳转场景");
        SceneManager.LoadScene(nextSceneName);
    }
}
