using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Level3Exchange : MonoBehaviour, IPointerClickHandler
{
    public Inventory playerInventory;
    public void OnPointerClick(PointerEventData eventData)
    {
        // 1. 收集当前三级合成界面中的所有资料
        List<int> inputItemIDs = new List<int>();

        foreach (var w in FindObjectsOfType<Window>())
            if (w.windowItem != null) inputItemIDs.Add(w.windowItem.itemNum);

        foreach (var w2 in FindObjectsOfType<Window2>())
            if (w2.windowItem != null) inputItemIDs.Add(w2.windowItem.itemNum);

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

        // 3. 调用对应的处理方法
        if (success)
            HandleSynthesisSuccess(resultID, similarity);
        else
            HandleSynthesisFail(similarity);
    }

    /// <summary>
    /// 处理合成成功的逻辑
    /// </summary>
    void HandleSynthesisSuccess(int resultID, float similarity)
    {
        Debug.Log($"【三级合成成功】结果ID: {resultID}, 相似度: {similarity:F1}%");

        // 1. 解锁对应三级资料
        Level3Data itemData = FindLevel3DataByID(resultID);
        playerInventory.level3List.Add(itemData);

        if (itemData != null)
        {
            var progress = PlayerDataManager.Instance.Data
                .GetOrCreateCharacter(itemData.characterId); // 假设每个三级资料有角色ID

            bool isFirstTime = !progress.HasCollectible(resultID);

            progress.AddCollectible(resultID);
            if (isFirstTime)
            {
                Level3SequencePlayer.Play(itemData);
            }

            // 如果需要，还可以播放解锁动画/演出
            Debug.Log($"已解锁资料: {itemData.itemName}");

            // 刷新角色资料界面
            if (Level3UIController.Instance.detailPanel != null &&
                Level3UIController.Instance.detailPanel.CurrentCharacterId == progress.characterId)
            {
                Level3UIController.Instance.detailPanel.Refresh(); // 重新刷新槽位显示
            }
        }
    }

    /// <summary>
    /// 处理合成失败的逻辑
    /// </summary>
    void HandleSynthesisFail(float similarity)
    {
        Debug.Log($"【三级合成失败】最高相似度: {similarity:F1}%");
        // 可在这里播放失败动画、音效或者提示文字
    }

    /// <summary>
    /// 根据 resultID 找到对应的 Level3Data
    /// </summary>
    Level3Data FindLevel3DataByID(int id)
    {
        var items = Resources.LoadAll<Level3Data>("Items/Level3");
        foreach (var item in items)
        {
            if (item.itemNum == id) return item;
        }
        Debug.LogWarning($"未找到 Level3Data: {id}");
        return null;
    }

}
