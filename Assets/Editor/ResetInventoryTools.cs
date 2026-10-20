#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class ResetInventoryTools
{
    [MenuItem("Tools/测试工具/一键重置背包与所有一级资料状态", false, 100)]
    public static void ResetAllInventoryAndPickedStates()
    {
        int resetAssetCount = 0;

        // 1. 检索项目中所有的 Level1Data 资源文件并重置 isPicked
        string[] guids = AssetDatabase.FindAssets("t:Level1Data");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Level1Data data = AssetDatabase.LoadAssetAtPath<Level1Data>(path);
            if (data != null)
            {
                Undo.RecordObject(data, "Reset Picked State");
                data.isPicked = false;
                EditorUtility.SetDirty(data);
                resetAssetCount++;
            }
        }

        // 保存对资源文件的修改
        AssetDatabase.SaveAssets();

        // 2. 如果场景中存在 Inventory 资产/组件，清空其内部列表
        Inventory[] inventories = Resources.FindObjectsOfTypeAll<Inventory>();
        foreach (var inv in inventories)
        {
            if (inv.level1List != null)
            {
                Undo.RecordObject(inv, "Clear Inventory List");
                inv.level1List.Clear();
                EditorUtility.SetDirty(inv);
            }
        }

        // 3. 游戏处于运行模式时，刷新背包 UI 与清空残留列表
        if (Application.isPlaying && InventoryManager.instance != null)
        {
            InventoryManager.instance.RefreshInventoryUI();
        }

        // 4. 同步清除人物关系图持久化存档
        PlayerPrefs.DeleteKey("TC_RelGraph_UnlockedChars");
        PlayerPrefs.DeleteKey("TC_RelGraph_ConnectedPairs");
        PlayerPrefs.Save();

        Debug.Log($"<color=#7CFF79>[ResetTools] 成功重置 {resetAssetCount} 个一级资料的 isPicked 状态，并已清空背包数据与关系图存档！</color>");
        EditorUtility.DisplayDialog("重置完成", $"已成功将 {resetAssetCount} 个 Level1Data 设为未拾取状态，背包及关系图存档已清空！", "确定");
    }
}
#endif