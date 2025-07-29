using UnityEngine;
using System.Collections.Generic;

public class LineManager : MonoBehaviour
{
    public GameObject linePrefab;
    public RectTransform lineParent;

    private Dictionary<string, UILineController> lines = new Dictionary<string, UILineController>();
    public void DrawLineBetween(RectTransform a, RectTransform b)
    {
        string key = GetLineKey(a, b);

        if (lines.ContainsKey(key))
        {
            Debug.Log("这条线已经存在");
            return;
        }

        GameObject lineGO = Instantiate(linePrefab, lineParent);
        lineGO.transform.SetAsFirstSibling();
        UILineController lineController = lineGO.GetComponent<UILineController>();
        lineController.SetNodes(a, b, true);

        lines.Add(key, lineController);
    }

    private string GetLineKey(RectTransform a, RectTransform b)
    {
        return a.GetInstanceID() < b.GetInstanceID() ?
            $"{a.GetInstanceID()}_{b.GetInstanceID()}" :
            $"{b.GetInstanceID()}_{a.GetInstanceID()}";
    }
}
