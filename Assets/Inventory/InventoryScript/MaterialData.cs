using System.Collections.Generic;

[System.Serializable]
public class MaterialData
{
    public List<DictionaryEntry> entries = new List<DictionaryEntry>();
    public List<string> aliases = new List<string>(); // 这份资料可能涉及的别名

    // 构造函数（可选）
    public MaterialData(List<DictionaryEntry> entries = null)
    {
        this.entries = entries ?? new List<DictionaryEntry>();
    }
}
