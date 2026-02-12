using UnityEngine;

[System.Serializable]
public class CGFrame
{
    public Sprite cgSprite;

    [Tooltip("该 CG 显示时长（秒），<=0 表示不自动切换")]
    public float duration = 1f;

    [Tooltip("是否等待玩家输入再进入下一帧")]
    public bool waitForInput = false;

    public KeyCode[] validKeys;
}
