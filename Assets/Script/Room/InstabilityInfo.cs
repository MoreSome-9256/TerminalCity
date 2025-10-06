using UnityEngine;

public class InstabilityInfo
{
    public string description;
    public Sprite profileSprite;

    public InstabilityInfo(string desc, Sprite sprite)
    {
        description = desc;
        profileSprite = sprite;
    }
}
public static class InstabilityTextProvider
{
    // 不同区间绑定不同文本和头像
    public static InstabilityInfo GetInfo(int instability, int threshold)
    {
        if (instability < 9700)
        {
            return new InstabilityInfo(
                "这里很安稳，可以放心进入。",
                Resources.Load<Sprite>("Picture/Character/Locus/Locus_smile") // 预制路径
            );
        }
        else if (instability >= 9700 && instability < 9800)
        {
            return new InstabilityInfo(
                "有些不稳定，小心为上。",
                Resources.Load<Sprite>("Picture/Character/Locus/Locus_normal")
            );
        }
        else if (instability >= 9800 && instability < 9900)
        {
            return new InstabilityInfo(
                "不稳定迹象明显，最好准备充分。",
                Resources.Load<Sprite>("Picture/Character/Locus/Locus_normal")
            );
        }
        else if (instability >= 9900 && instability < 10000)
        {
            return new InstabilityInfo(
                "危险！逆恒值接近极限，强烈不建议进入。",
                Resources.Load<Sprite>("Picture/Character/Locus/Locus_serious")
            );
        }
        else
        {
            return new InstabilityInfo(
                "该房间因逆恒值突破极限点，已无法进入。",
                Resources.Load<Sprite>("Picture/Character/Locus/Locus_serious")
            );
        }
    }
}