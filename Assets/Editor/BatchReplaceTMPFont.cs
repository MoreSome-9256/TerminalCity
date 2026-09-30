using UnityEngine;
using UnityEditor;
using TMPro;

public class BatchReplaceTMPFont : EditorWindow
{
    private TMP_FontAsset targetFont;
    private bool includeInactive = true;

    [MenuItem("Tools/批量替换 TMP 字体")]
    public static void ShowWindow()
    {
        GetWindow<BatchReplaceTMPFont>("TMP 字体批量替换");
    }

    private void OnGUI()
    {
        GUILayout.Label("全局/当前场景 TMP 字体替换工具", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        targetFont = (TMP_FontAsset)EditorGUILayout.ObjectField("目标新字体 (Font Asset)", targetFont, typeof(TMP_FontAsset), false);
        includeInactive = EditorGUILayout.Toggle("包含隐藏(Inactive)物体", includeInactive);

        EditorGUILayout.Space();

        if (GUILayout.Button("替换当前场景中所有 TMP", GUILayout.Height(35)))
        {
            if (targetFont == null)
            {
                EditorUtility.DisplayDialog("提示", "请先拖入目标新字体！", "确定");
                return;
            }
            ReplaceInCurrentScene();
        }
    }

    private void ReplaceInCurrentScene()
    {
        // 查找场景中所有的 TextMeshProUGUI 组件
        TextMeshProUGUI[] textComponents = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
        int count = 0;

        foreach (var text in textComponents)
        {
            // 过滤掉非场景里的预制体资源本体
            if (EditorUtility.IsPersistent(text.transform.root.gameObject))
                continue;

            if (!includeInactive && !text.gameObject.activeInHierarchy)
                continue;

            // 记录撤销操作并更新字体
            Undo.RecordObject(text, "Batch Replace TMP Font");
            text.font = targetFont;
            EditorUtility.SetDirty(text);
            count++;
        }

        // 同时支持 3D 空间的 TextMeshPro 组件（如果有使用）
        TextMeshPro[] worldTexts = Resources.FindObjectsOfTypeAll<TextMeshPro>();
        foreach (var text in worldTexts)
        {
            if (EditorUtility.IsPersistent(text.transform.root.gameObject))
                continue;

            if (!includeInactive && !text.gameObject.activeInHierarchy)
                continue;

            Undo.RecordObject(text, "Batch Replace TMP Font");
            text.font = targetFont;
            EditorUtility.SetDirty(text);
            count++;
        }

        // 标记场景已修改，提醒保存
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        EditorUtility.DisplayDialog("完成", $"已成功替换 {count} 个 TMP 文本组件的字体！\n请记得 Ctrl+S 保存场景。", "确定");
    }
}