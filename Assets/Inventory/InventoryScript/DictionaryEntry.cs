[System.Serializable]
public class DictionaryEntry
{
    public string term;           // 正式词条
    public string definition;     // 解释
    public bool hasTerm;
    public bool hasDefinition;

    public string[] aliases;      // 别名 / 同义词（比如 "TC", "TERACITY"）

    public DictionaryEntry(string term, string definition, bool hasTerm = false, bool hasDefinition = false, string[] aliases = null)
    {
        this.term = term;
        this.definition = definition;
        this.hasTerm = hasTerm;
        this.hasDefinition = hasDefinition;
        this.aliases = aliases ?? new string[0];
    }
}
