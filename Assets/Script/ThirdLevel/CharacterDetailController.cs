using UnityEngine;

public class CharacterDetailController : MonoBehaviour
{
    public static CharacterDetailController Instance { get; private set; }

    [Header("槽位父对象")]
    public Transform slotParent;

    [Header("槽位模板")]
    public SlotView slotPrefab;

    // 当前显示的角色配置
    private CharacterConfig currentConfig;
    public string CurrentCharacterId { get; private set; }

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false); // 默认隐藏
    }

    /// <summary>
    /// 显示某角色详情
    /// </summary>
    public void Show(CharacterConfig config)
    {
        if (config == null) return;

        currentConfig = config;
        CurrentCharacterId = config.characterId;

        Refresh();

        gameObject.SetActive(true);
    }

    /// <summary>
    /// 刷新当前角色槽位显示
    /// </summary>
    public void Refresh()
    {
        if (currentConfig == null) return;

        // 清空旧槽位
        foreach (Transform t in slotParent)
            Destroy(t.gameObject);

        // 读取玩家进度
        var progress = PlayerDataManager.Instance.Data
            .GetOrCreateCharacter(currentConfig.characterId);

        // 创建槽位
        foreach (var item in currentConfig.collectibles)
        {
            bool obtained = progress.HasCollectible(item.itemNum);
            var slot = Instantiate(slotPrefab, slotParent);
            slot.Refresh(item, obtained);
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void OnSlotClicked(Level3Data data)
    {
        // 打开三级资料阅读界面
    }

    public void OnBackClicked()
    {
        Level3UIController.Instance.ShowCharacterList();
    }
}
