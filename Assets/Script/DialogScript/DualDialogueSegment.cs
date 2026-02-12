using UnityEngine;

[System.Serializable]
public class DualDialogueSegment
{
    public string speakerName;
    [TextArea(3, 10)]
    public string text;

    public Sprite leftSprite;
    public Sprite rightSprite;

    public bool highlightLeft;   // 当前说话人是左边？
}
