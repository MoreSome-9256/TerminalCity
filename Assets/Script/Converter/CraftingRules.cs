using System.Collections.Generic;

[System.Serializable] // 可序列化，便于调试查看
public class CraftingRule
{
    public List<int> requiredItems;
    public int resultItem;
}