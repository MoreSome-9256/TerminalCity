using UnityEngine;
using System.Collections;

public class BGMManager : MonoBehaviour
{
    private AudioSource bgmSource;

    [SerializeField] private AudioClip initialClip;

    private Coroutine fadeOutCoroutine = null;

    private void Awake()
    {
        bgmSource = GetComponent<AudioSource>();
        if (initialClip != null)
        {
            bgmSource.clip = initialClip;
            //bgmSource.Play();
        }
    }

    public void PlayBGM()
    {
        bgmSource.Stop();           // Í£Ö¹¾ÉÒôÀÖ
        bgmSource.clip = initialClip;      // ÇÐ»»µ½ÐÂÒôÀÖ
        bgmSource.Play();
    }

    public void StopBGM() => bgmSource.Stop();
    public void PauseBGM() => bgmSource.Pause();
    public void ResumeBGM() => bgmSource.UnPause();
    public void FadeOutBGM(float duration)
    {
        if (fadeOutCoroutine != null)
        {
            StopCoroutine(fadeOutCoroutine);
        }
        fadeOutCoroutine = StartCoroutine(FadeOutCoroutine(duration));
    }

    private IEnumerator FadeOutCoroutine(float duration)
    {
        float startVolume = bgmSource.volume;

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, time / duration);
            yield return null;
        }

        bgmSource.Stop();
        bgmSource.volume = startVolume; // »Ö¸´ÒôÁ¿£¬·½±ãÏÂ´Î²¥·Å
    }
}
