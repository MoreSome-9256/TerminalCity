using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;
    private AudioSource bgmSource;

    private void Awake()
    {
        // 保证只有一个 BGMManager（单例模式）
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 音乐在切换场景时不销毁
            bgmSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayBGM()
    {
        if (!bgmSource.isPlaying)
            bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource.isPlaying)
            bgmSource.Stop();
    }

    public void PauseBGM()
    {
        if (bgmSource.isPlaying)
            bgmSource.Pause();
    }

    public void ResumeBGM()
    {
        if (!bgmSource.isPlaying)
            bgmSource.UnPause();
    }
}
