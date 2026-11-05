#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class ResetInventoryTools
{
    [MenuItem("Tools/测试工具/一键重置背包与所有一二级资料状态", false, 100)]
    public static void ResetAllInventoryAndPickedStates()
    {
        int resetAssetCount = 0;

        // 1. 检索项目中所有的 Level1Data 资源文件并重置
        string[] level1Guids = AssetDatabase.FindAssets("t:Level1Data");
        foreach (string guid in level1Guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Level1Data data = AssetDatabase.LoadAssetAtPath<Level1Data>(path);
            if (data != null)
            {
                Undo.RecordObject(data, "Reset Level1 State");
                data.isPicked = false;

                // 继承自 Item 的通用属性重置
                data.isDecoded = false;
                data.readTime = 0;

                EditorUtility.SetDirty(data);
                resetAssetCount++;
            }
        }

        // 2. 检索项目中所有的 Level2Data 资源文件并重置
        string[] level2Guids = AssetDatabase.FindAssets("t:Level2Data");
        foreach (string guid in level2Guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Level2Data data = AssetDatabase.LoadAssetAtPath<Level2Data>(path);
            if (data != null)
            {
                Undo.RecordObject(data, "Reset Level2 State");

                // 继承自 Item 的通用属性重置
                data.isDecoded = false;
                data.readTime = 0;

                EditorUtility.SetDirty(data);
                resetAssetCount++;
            }
        }

        // 保存对资源文件的修改
        AssetDatabase.SaveAssets();

        // 3. 如果场景中存在 Inventory 资产/组件，清空其内部的 1级 和 2级 列表
        Inventory[] inventories = Resources.FindObjectsOfTypeAll<Inventory>();
        foreach (var inv in inventories)
        {
            Undo.RecordObject(inv, "Clear Inventory Lists");

            if (inv.level1List != null)
            {
                inv.level1List.Clear();
            }
            if (inv.level2List != null)
            {
                inv.level2List.Clear();
            }

            EditorUtility.SetDirty(inv);
        }

        // 4. 游戏处于运行模式时，刷新背包 UI
        if (Application.isPlaying && InventoryManager.instance != null)
        {
            InventoryManager.instance.RefreshInventoryUI();
        }

        // 5. 同步清除人物关系图持久化存档
        PlayerPrefs.DeleteKey("TC_RelGraph_UnlockedChars");
        PlayerPrefs.DeleteKey("TC_RelGraph_ConnectedPairs");
        PlayerPrefs.Save();

        Debug.Log($"<color=#7CFF79>[ResetTools] 成功重置 {resetAssetCount} 个一/二级资料的状态（含 isDecoded 和 readTime），并已清空背包数据与关系图存档！</color>");
        EditorUtility.DisplayDialog("重置完成", $"已成功将 {resetAssetCount} 个资料重置，背包（一二级）及关系图存档已清空！", "确定");
    }
}
#endif