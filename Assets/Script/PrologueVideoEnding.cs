using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using System.IO;

[RequireComponent(typeof(VideoPlayer), typeof(RawImage))]
public class PrologueVideoEnding : MonoBehaviour
{
    public static PrologueVideoEnding Instance { get; private set; }

    [Header("视频配置")]
    [Tooltip("放在 StreamingAssets 文件夹下的视频完整带后缀名称")]
    [SerializeField] private string videoFileName = "video1.mp4";

    [Header("流程控制")]
    [Tooltip("视频播完后要加载的场景名字，留空则不跳转")]
    [SerializeField] private string nextSceneName = "Chapter1";
    [Tooltip("是否允许玩家按空格键跳过过场")]
    [SerializeField] private bool canSkip = true;

    [Header("播放结束回调事件")]
    public UnityEvent onVideoFinished;

    private VideoPlayer videoPlayer;
    private RawImage videoRawImage;
    private RenderTexture renderTexture;
    private bool isPlaying = false;

    private void Awake()
    {
        Instance = this;
        videoPlayer = GetComponent<VideoPlayer>();
        videoRawImage = GetComponent<RawImage>();

        // 绑定播放结束的事件监听
        videoPlayer.loopPointReached += OnVideoEndReached;

        // 核心修改：完成单例注册后，在第一帧画面渲染前立刻把自己隐藏掉
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (isPlaying && canSkip)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape))
            {
                FinishVideo();
            }
        }
    }

    /// <summary>
    /// 外部调用：开始播放视频
    /// </summary>
    public void PlayEndingVideo()
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling(); // 确保视频层级在最上方遮挡所有UI

        // ✨ 新增：视频起播时，淡出或瞬间停掉场景里所有的基础曲目与分轨
        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.FadeOutBGM(0.5f); // 半秒内快速淡出
            // 如果希望视频出来的瞬间立刻完全静音，可以改为：
            // BGMManager.Instance.StopBGM();
        }

        // 1. 动态创建高清 RenderTexture
        if (renderTexture == null)
        {
            renderTexture = new RenderTexture(Screen.width, Screen.height, 0);
        }

        // 2. 将 VideoPlayer 与 RawImage 桥接
        videoPlayer.targetTexture = renderTexture;
        videoRawImage.texture = renderTexture;

        // 3. 配置 URL 路径并播放
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = Path.Combine(Application.streamingAssetsPath, videoFileName);

        videoPlayer.Play();
        isPlaying = true;
    }

    private void OnVideoEndReached(VideoPlayer source)
    {
        FinishVideo();
    }

    private void FinishVideo()
    {
        if (!isPlaying) return;
        isPlaying = false;

        videoPlayer.Stop();

        // 释放内存与贴图资源
        if (renderTexture != null)
        {
            videoPlayer.targetTexture = null;
            videoRawImage.texture = null;
            renderTexture.Release();
            Destroy(renderTexture);
            renderTexture = null;
        }

        gameObject.SetActive(false);
        onVideoFinished?.Invoke();

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoEndReached;
        }
    }
}