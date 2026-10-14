using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class MemoData
{
    public string id;
    [TextArea(3, 10)]
    public string content; // 备忘录整页文本内容

    public MemoData()
    {
        id = Guid.NewGuid().ToString();
        content = "";
    }
}

[Serializable]
public class MemoSaveContainer
{
    public List<MemoData> list = new List<MemoData>();
}

public class MemoManager : MonoBehaviour
{
    [Header("页面引用")]
    [Tooltip("备忘录列表容器（Scroll View 所在界面）")]
    [SerializeField] private GameObject listPanel;
    [Tooltip("点击条目后弹出的全屏/整页编辑面板")]
    [SerializeField] private GameObject editorPanel;

    [Header("列表视图")]
    [Tooltip("对应 Scroll View/Viewport/Content")]
    [SerializeField] private Transform contentContainer;
    [Tooltip("你的 MemoItem Prefab")]
    [SerializeField] private GameObject memoItemPrefab;
    [Tooltip("右上角或底部的 新增 按钮 (AddButton)")]
    [SerializeField] private Button addButton;

    [Header("编辑页面组件")]
    [Tooltip("整页多行文本输入框 (TMP_InputField)")]
    [SerializeField] private TMP_InputField editorInputField;
    [Tooltip("编辑界面的返回按钮（点击自动保存并回列表）")]
    [SerializeField] private Button backButton;

    // 数据源
    private List<MemoData> memoList = new List<MemoData>();
    private MemoData activeEditingMemo = null;

    private const string SAVE_KEY = "TerminalCity_Memos";

    private void Awake()
    {
        if (addButton != null)
            addButton.onClick.AddListener(CreateNewMemo);

        if (backButton != null)
            backButton.onClick.AddListener(SaveAndReturnToList);

        LoadData();
    }

    private void OnEnable()
    {
        // 每次切到备忘录标签，确保回到列表并刷新
        ShowListView();
    }

    #region 列表控制

    public void ShowListView()
    {
        if (editorPanel != null) editorPanel.SetActive(false);
        if (listPanel != null) listPanel.SetActive(true);

        RefreshListUI();
    }

    private void RefreshListUI()
    {
        // 清理当前生成的旧条目
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        // 重新按数据生成列表项
        foreach (var memo in memoList)
        {
            GameObject itemObj = Instantiate(memoItemPrefab, contentContainer);
            MemoItemUI itemUI = itemObj.GetComponent<MemoItemUI>();
            if (itemUI != null)
            {
                // 传入 OpenEditor 和 DeleteMemo
                itemUI.Setup(memo, OpenEditor, DeleteMemo);
            }
        }
    }

    /// <summary>
    /// 删除指定备忘录
    /// </summary>
    public void DeleteMemo(MemoData memo)
    {
        if (memo == null) return;

        if (memoList.Contains(memo))
        {
            memoList.Remove(memo);
            SaveData();         // 保存改动到本地存档
            RefreshListUI();    // 重新刷新列表视图
        }
    }

    /// <summary>
    /// 点击【新增】按钮
    /// </summary>
    private void CreateNewMemo()
    {
        // 1. 新建数据并插入到最前面
        MemoData newMemo = new MemoData();
        memoList.Insert(0, newMemo);

        // 2. 直接进入整页编辑页
        OpenEditor(newMemo);
    }

    #endregion

    #region 整页编辑与自动保存

    /// <summary>
    /// 点击 MemoItem 展开内部整页编辑
    /// </summary>
    public void OpenEditor(MemoData memo)
    {
        activeEditingMemo = memo;

        if (listPanel != null) listPanel.SetActive(false);
        if (editorPanel != null) editorPanel.SetActive(true);

        if (editorInputField != null)
        {
            editorInputField.text = memo.content;
            // 不要写 ActivateInputField()！
            // 这样点进来就是纯净的阅读状态，绝不会有全选蓝底，也不会有误触覆盖文本的风险
        }
    }

    /// <summary>
    /// 点击返回键：自动提取输入框文本保存并切回列表
    /// </summary>
    public void SaveAndReturnToList()
    {
        if (activeEditingMemo != null && editorInputField != null)
        {
            activeEditingMemo.content = editorInputField.text;

            // 可选：如果一条备忘录完全没输入任何字，退出时可以直接移除该空白项
            if (string.IsNullOrWhiteSpace(activeEditingMemo.content))
            {
                memoList.Remove(activeEditingMemo);
            }
        }

        activeEditingMemo = null;
        SaveData();
        ShowListView();
    }

    #endregion

    #region 数据持久化 (存档)

    private void SaveData()
    {
        MemoSaveContainer container = new MemoSaveContainer { list = this.memoList };
        string json = JsonUtility.ToJson(container);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    private void LoadData()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            string json = PlayerPrefs.GetString(SAVE_KEY);
            MemoSaveContainer container = JsonUtility.FromJson<MemoSaveContainer>(json);
            if (container != null && container.list != null)
            {
                memoList = container.list;
            }
        }
    }

    #endregion
}