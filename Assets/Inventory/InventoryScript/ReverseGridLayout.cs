using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class ReverseGridLayout : MonoBehaviour
{
    private GridLayoutGroup gridLayoutGroup;

    private void Awake()
    {
        gridLayoutGroup = GetComponent<GridLayoutGroup>();
    }

    // 添加新子物体并移动到首位
    public void AddItemToTop(GameObject item)
    {
        // 添加新子物体
        item.transform.SetParent(transform, false);

        // 将新子物体移动到首位
        item.transform.SetAsFirstSibling();

        // 强制刷新布局
        LayoutRebuilder.ForceRebuildLayoutImmediate(gridLayoutGroup.GetComponent<RectTransform>());
    }
}