using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StemClip
{
    [Tooltip("分轨名称，如 'Guitar', 'Strings'")]
    public string trackName;
    [Tooltip("与主旋律严格同长度、同BPM的分轨音频")]
    public AudioClip clip;
    [Range(0f, 1f)] public float maxVolume = 1f;
    public bool playOnStart = false;
    public float fadeDuration = 0.5f;
}

[CreateAssetMenu(fileName = "NewBGMTrack", menuName = "Audio/BGM Track Data")]
public class BGMTrackData : ScriptableObject
{
    [Tooltip("基础主旋律")]
    public AudioClip baseClip;

    [Range(0f, 1f)] public float baseVolume = 1f;

    [Tooltip("这首歌包含的动态分轨列表")]
    public List<StemClip> stems = new List<StemClip>();
}