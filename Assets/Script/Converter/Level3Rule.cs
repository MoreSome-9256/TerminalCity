using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class Level3Rule
{
    public List<int> items;        // 理想组合中的物品ID
    public List<float> weights;    // 对应权重
    public int resultItem;         // 产出三级资料ID

    // 该规则的总权重（用于后续算百分比）
    public float TotalWeight => weights.Sum();
}
