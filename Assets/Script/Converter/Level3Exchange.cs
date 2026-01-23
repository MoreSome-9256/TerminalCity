using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class Level3Exchange : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        // 1. 收集当前三级合成界面中的所有资料
        var windows = FindObjectsOfType<Window>();
        var windows2 = FindObjectsOfType<Window2>();

        List<int> inputItemIDs = new List<int>();

        foreach (var w in windows)
        {
            if (w.windowItem != null)
                inputItemIDs.Add(w.windowItem.itemNum);
        }

        foreach (var w2 in windows2)
        {
            if (w2.windowItem != null)
                inputItemIDs.Add(w2.windowItem.itemNum);
        }

        if (inputItemIDs.Count == 0)
        {
            Debug.Log("【三级合成】没有放入任何资料");
            return;
        }

        Debug.Log($"【三级合成】参与合成的资料ID：{string.Join(", ", inputItemIDs)}");

        // 2. 调用三级合成系统
        Level3SynthesisSystem system = FindObjectOfType<Level3SynthesisSystem>();
        if (system == null)
        {
            Debug.LogError("未找到 Level3SynthesisSystem");
            return;
        }

        bool success = system.TrySynthesize(
            inputItemIDs,
            out int resultID,
            out float similarity
        );

        // 3. 仅做 Debug 输出
        if (success)
        {
            Debug.Log($"【三级合成成功】结果ID: {resultID}, 相似度: {similarity:F1}%");
        }
        else
        {
            Debug.Log($"【三级合成失败】最高相似度: {similarity:F1}%");
        }

    }
}
