using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance { get; private set; }

    [Header("主播放器")]
    private AudioSource baseSource;

    [Header("动态分轨池")]
    private List<AudioSource> stemSourcePool = new List<AudioSource>();
    private Dictionary<string, ActiveStem> activeStems = new Dictionary<string, ActiveStem>();

    private Coroutine baseFadeCoroutine;

    private class ActiveStem
    {
        public StemClip config;
        public AudioSource source;
        public float currentTargetVolume;
        public Coroutine fadeCoroutine;
    }

    public BGMTrackData CurrentTrackData { get; private set; }
    private Coroutine songSwitchCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // 若需要跨场景保留可取消注释
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        baseSource = gameObject.AddComponent<AudioSource>();
        baseSource.loop = true;
    }

    /// <summary>
    /// 核心接口：平滑切换到新的 BGM 资产
    /// </summary>
    public void SwitchBGM(BGMTrackData newTrack, float crossfadeDuration = 1.0f)
    {
        if (newTrack == null)
        {
            FadeOutBGM(crossfadeDuration);
            return;
        }

        if (CurrentTrackData == newTrack) return; // 已经在播当前曲目则忽略

        if (songSwitchCoroutine != null) StopCoroutine(songSwitchCoroutine);
        songSwitchCoroutine = StartCoroutine(CrossfadeSongRoutine(newTrack, crossfadeDuration));
    }

    private IEnumerator CrossfadeSongRoutine(BGMTrackData newTrack, float duration)
    {
        // 1. 旧曲目整体淡出
        if (baseSource.isPlaying)
        {
            float elapsed = 0f;
            float startVol = baseSource.volume;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float factor = 1f - (elapsed / duration);
                baseSource.volume = startVol * factor;

                foreach (var stem in activeStems.Values)
                {
                    stem.source.volume = stem.currentTargetVolume * factor;
                }
                yield return null;
            }
        }

        StopBGM(); // 停止所有旧播放

        // ✨ 新增：如果传进来的新曲目是 null（说明只是纯粹的淡出停止），直接结束协程！
        if (newTrack == null)
        {
            yield break;
        }

        // 2. 装载新数据
        CurrentTrackData = newTrack;
        baseSource.clip = newTrack.baseClip;
        baseSource.volume = newTrack.baseVolume;

        // 动态适配分轨池数量
        EnsureStemSourceCapacity(newTrack.stems.Count);
        activeStems.Clear();

        double dspStartTime = AudioSettings.dspTime + 0.05;

        for (int i = 0; i < newTrack.stems.Count; i++)
        {
            var config = newTrack.stems[i];
            AudioSource src = stemSourcePool[i];
            src.clip = config.clip;
            src.loop = true;
            src.volume = config.playOnStart ? config.maxVolume : 0f;

            var active = new ActiveStem
            {
                config = config,
                source = src,
                currentTargetVolume = src.volume
            };

            if (!string.IsNullOrEmpty(config.trackName))
            {
                activeStems[config.trackName] = active;
            }

            if (src.clip != null)
            {
                src.PlayScheduled(dspStartTime);
            }
        }

        if (baseSource.clip != null)
        {
            baseSource.PlayScheduled(dspStartTime);
        }
    }

    private void EnsureStemSourceCapacity(int requiredCount)
    {
        while (stemSourcePool.Count < requiredCount)
        {
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            newSource.playOnAwake = false;
            stemSourcePool.Add(newSource);
        }
    }

    #region 基础播放控制 (暂停/恢复/停止)

    public void PauseBGM()
    {
        if (baseSource != null) baseSource.Pause();
        foreach (var src in stemSourcePool)
        {
            if (src != null) src.Pause();
        }
    }

    public void ResumeBGM()
    {
        if (baseSource != null) baseSource.UnPause();
        foreach (var src in stemSourcePool)
        {
            if (src != null) src.UnPause();
        }
    }

    public void StopBGM()
    {
        if (baseSource != null) baseSource.Stop();
        foreach (var src in stemSourcePool)
        {
            if (src != null) src.Stop();
        }
    }

    #endregion

    #region 分轨控制接口 (Event List 调用)

    public void EnableTrack(string trackName) => SetTrackActive(trackName, true);
    public void DisableTrack(string trackName) => SetTrackActive(trackName, false);

    public void SetTrackActive(string trackName, bool enable)
    {
        if (!activeStems.TryGetValue(trackName, out ActiveStem stem))
        {
            Debug.LogWarning($"[BGMManager] 当前播放的曲目未包含分轨: {trackName}");
            return;
        }

        float target = enable ? stem.config.maxVolume : 0f;
        if (Mathf.Approximately(stem.currentTargetVolume, target)) return;

        stem.currentTargetVolume = target;
        if (stem.fadeCoroutine != null) StopCoroutine(stem.fadeCoroutine);
        stem.fadeCoroutine = StartCoroutine(FadeStemRoutine(stem, target, stem.config.fadeDuration));
    }

    private IEnumerator FadeStemRoutine(ActiveStem stem, float targetVolume, float duration)
    {
        float start = stem.source.volume;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            stem.source.volume = Mathf.Lerp(start, targetVolume, elapsed / duration);
            yield return null;
        }
        stem.source.volume = targetVolume;
    }

    public void FadeOutBGM(float duration)
    {
        if (songSwitchCoroutine != null) StopCoroutine(songSwitchCoroutine);
        songSwitchCoroutine = StartCoroutine(CrossfadeSongRoutine(null, duration));
    }

    #endregion
    /// <summary>
    /// 将指定分轨平滑渐变到具体的音量值 (0f ~ 1f)
    /// </summary>
    public void SetTrackVolume(string trackName, float targetVolume)
    {
        if (!activeStems.TryGetValue(trackName, out ActiveStem stem))
        {
            Debug.LogWarning($"[BGMManager] 当前播放的曲目未包含分轨: {trackName}");
            return;
        }

        // 确保不超过该分轨设置的最大限制
        targetVolume = Mathf.Clamp(targetVolume, 0f, stem.config.maxVolume);

        if (Mathf.Approximately(stem.currentTargetVolume, targetVolume)) return;

        stem.currentTargetVolume = targetVolume;
        if (stem.fadeCoroutine != null) StopCoroutine(stem.fadeCoroutine);
        stem.fadeCoroutine = StartCoroutine(FadeStemRoutine(stem, targetVolume, stem.config.fadeDuration));
    }

    /// <summary>
    /// 将主轨（BaseClip）平滑渐变到指定音量
    /// </summary>
    public void SetBaseTrackVolume(float targetVolume, float fadeDuration = 0.5f)
    {
        if (baseSource == null) return;

        if (baseFadeCoroutine != null) StopCoroutine(baseFadeCoroutine);
        baseFadeCoroutine = StartCoroutine(FadeBaseRoutine(targetVolume, fadeDuration));
    }

    /// <summary>
    /// 恢复主轨到当前配置的默认音量
    /// </summary>
    public void RestoreBaseTrackVolume(float fadeDuration = 0.5f)
    {
        if (CurrentTrackData != null)
        {
            SetBaseTrackVolume(CurrentTrackData.baseVolume, fadeDuration);
        }
    }

    private IEnumerator FadeBaseRoutine(float targetVol, float duration)
    {
        float startVol = baseSource.volume;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            baseSource.volume = Mathf.Lerp(startVol, targetVol, elapsed / duration);
            yield return null;
        }
        baseSource.volume = targetVol;
    }
}