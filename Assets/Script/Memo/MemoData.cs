using System;
using System.Collections.Generic;

[Serializable]
public class MemoItem
{
    public string id;              // 唯一标识
    public string title;           // 标题（为空时可自动截取正文前几个字）
    public string content;         // 正文内容
    public string updateTime;      // 最后编辑时间字符串

    public MemoItem()
    {
        id = Guid.NewGuid().ToString();
        title = "新备忘录";
        content = "";
        updateTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
    }
}

[Serializable]
public class MemoSaveData
{
    public List<MemoItem> memoList = new List<MemoItem>();
}