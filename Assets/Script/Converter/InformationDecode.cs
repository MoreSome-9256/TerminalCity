using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class InformationDecode : MonoBehaviour
{
    [Header("组件绑定")]
    public Button infoDisplayButton;    // 信息显示按钮
    public TMP_Text infoDisplayText;    // 信息显示文本
    public float typingSpeed = 0.05f;   // 打字速度

    private Coroutine currentRoutine;

    void Start()
    {
        infoDisplayButton.onClick.AddListener(OnInfoButtonClicked);
    }

    private void OnInfoButtonClicked()
    {
        // 修改为通过实例访问数据
        if (Window.CurrentSelected == null) return;

        Level1Data data = Window.CurrentSelected.GetLevel1Data();  // 通过窗口实例获取数据
        if (data != null)
        {
            StartDisplayRoutine(data);
        }
        else
        {
            infoDisplayText.text = "未找到有效数据";
        }
    }

    private void StartDisplayRoutine(Level1Data data)
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }
        if (!data.isDecoded)
        {
            currentRoutine = StartCoroutine(TypewriterEffect(data));
            data.isDecoded = true;
        }
        else
        {
            DirectlyShow(data);
        }
    }

    private IEnumerator TypewriterEffect(Level1Data data)
    {
        string processedName = ProcessString(data.Name);
        string processedTime = ProcessString(data.Time);
        string processedEvent = ProcessString(data.Event);

        string fullText = $"<color=#ffbf6f><size=45>资料解码中……</size></color>\n<color=#ffecb7>>></color><color=#b7f0ff>人物：</color>{processedName}\n<color=#ffecb7>>></color><color=#b7f0ff>时间：</color>{processedTime}\n<color=#ffecb7>>></color><color=#b7f0ff>事件：</color>{processedEvent}";

        infoDisplayText.text = "";
        int visibleCharacters = 0; // 当前可见字符的索引
        bool insideTag = false; // 标记是否在HTML标签内

        while (visibleCharacters < fullText.Length)
        {
            char currentChar = fullText[visibleCharacters];

            // 检查是否进入或退出HTML标签
            if (currentChar == '<')
            {
                insideTag = true;
            }
            else if (currentChar == '>')
            {
                insideTag = false;
            }
            // 增加可见字符的索引
            visibleCharacters++;
            // 更新文本，确保HTML标签不会被逐字显示
            infoDisplayText.text = fullText.Substring(0, visibleCharacters);
            // 如果不在标签内，等待打字速度
            if (!insideTag)
            {
                yield return new WaitForSeconds(typingSpeed);
            }
        }
    }
    private void DirectlyShow(Level1Data data)
    {
        string processedName = ProcessString(data.Name);
        string processedTime = ProcessString(data.Time);
        string processedEvent = ProcessString(data.Event);

        string fullText = $"<color=#ffbf6f><size=45>资料解码中……</size></color>\n<color=#ffecb7>>></color><color=#b7f0ff>人物：</color>{processedName}\n<color=#ffecb7>>></color><color=#b7f0ff>时间：</color>{processedTime}\n<color=#ffecb7>>></color><color=#b7f0ff>事件：</color>{processedEvent}";

        infoDisplayText.text = fullText;
        Debug.Log("重复");
    }

    private string ProcessString(string input)
    {
        return string.IsNullOrEmpty(input) ? "无" : input;
    }
}