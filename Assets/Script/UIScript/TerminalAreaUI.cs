using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TerminalAreaUI : MonoBehaviour
{
    [Header("UI 组件引用")]
    [SerializeField] private Image placeIconImage;   // 区域小图标（如生活区的小房子等）
    [SerializeField] private TMP_Text areaNameText;  // 显示 "生活区"、"医疗区" 等
    [SerializeField] private TMP_Text rhoText;       // 显示 "RHO: 洛库斯/Locus"，自带底框

    [Header("各区域图标配置 (1-10 对应下标 0-9)")]
    [SerializeField] private Sprite[] placeIcons;    // 长度为 10 的图标数组
    [SerializeField] private Sprite unknownIcon;     // "？？？" 时的默认占位图标（可选）

    private int lastPlaceNumber = -999;

    private void Start()
    {
        RefreshAreaDisplay();
    }

    private void Update()
    {
        if (RoomManager.Instance == null) return;

        // 监测 placeNumber 变化（例如场景切换或传送）
        if (RoomManager.Instance.placeNumber != lastPlaceNumber)
        {
            RefreshAreaDisplay();
        }
    }

    /// <summary>
    /// 根据 RoomManager.placeNumber 刷新区域图标、名称和 RHO 显示
    /// </summary>
    public void RefreshAreaDisplay()
    {
        if (RoomManager.Instance == null) return;

        int placeNum = RoomManager.Instance.placeNumber;
        lastPlaceNumber = placeNum;

        GetAreaInfo(placeNum, out string areaName, out string rhoName, out Sprite icon);

        // 1. 设置区域图标
        if (placeIconImage != null)
        {
            placeIconImage.sprite = icon;
            placeIconImage.gameObject.SetActive(icon != null);
        }

        // 2. 设置区域名称
        if (areaNameText != null)
        {
            areaNameText.text = areaName;
        }

        // 3. 设置 RHO 显示逻辑（为空或？？？时直接隐藏自身）
        if (rhoText != null)
        {
            if (string.IsNullOrEmpty(rhoName))
            {
                rhoText.gameObject.SetActive(false);
            }
            else
            {
                rhoText.gameObject.SetActive(true);
                rhoText.text = $"RHO: {rhoName}";
            }
        }
    }

    /// <summary>
    /// 区域信息映射
    /// </summary>
    private void GetAreaInfo(int placeNumber, out string areaName, out string rhoName, out Sprite icon)
    {
        // 尝试从数组中获取对应图标（1-10 映射到下标 0-9）
        int iconIndex = placeNumber - 1;
        icon = (placeIcons != null && iconIndex >= 0 && iconIndex < placeIcons.Length)
            ? placeIcons[iconIndex]
            : unknownIcon;

        switch (placeNumber)
        {
            case 1:
                areaName = "生活区";
                rhoName = "洛库斯/Locus";
                break;
            case 2:
                areaName = "医疗区";
                rhoName = "埃洛拉/Aurora"; // 可根据你的设定替换
                break;
            case 3:
                areaName = "仓储区";
                rhoName = "丽贝卡/Rabbica"; // 可根据你的设定替换
                break;
            case 4:
                areaName = "研究区";
                rhoName = "缪尔/Moore";  // 可根据你的设定替换
                break;
            case 5:
                areaName = "信息区";
                rhoName = "伊莱斯/Elysian";    // 可根据你的设定替换
                break;
            case 6:
                areaName = "能源区";
                rhoName = "希奥多/Theodore";      // 可根据你的设定替换
                break;
            case 7:
                areaName = "重工区";
                rhoName = "开尔文/Kelvin";  // 可根据你的设定替换
                break;
            case 8:
                areaName = "办公区";
                rhoName = "霍维德/Hoveld";      // 可根据你的设定替换
                break;
            case 9:
                areaName = "休眠区";
                rhoName = "克索里/Xori";    // 可根据你的设定替换
                break;
            case 10:
                areaName = "中央控制区";
                rhoName = "艾瑞丝/Iris"; // 可根据你的设定替换
                break;
            default:
                areaName = "？？？";
                rhoName = null; // 其他数值不显示 RHO
                break;
        }
    }
}