using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Level3UIController : MonoBehaviour
{
    public static Level3UIController Instance { get; private set; }

    [Header("子界面")]
    public GameObject characterListPanel;
    public CharacterDetailController detailPanel;

    [Header("角色卡")]
    public List<GameObject> characterCards;     // 11 个
    public List<CharacterConfig> allCharacters; // 11 个

    private void Awake()
    {
        Instance = this;
    }

    public void Open()
    {
        gameObject.SetActive(true);
        ShowCharacterList();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void ShowCharacterList()
    {
        characterListPanel.SetActive(true);
        detailPanel.Hide();
        RefreshCharacterList(); // ⭐ 关键
    }

    public void ShowCharacterDetail(CharacterConfig config)
    {
        var progress = PlayerDataManager.Instance.Data
            .GetOrCreateCharacter(config.characterId);

        if (!progress.unlocked)
            return;

        characterListPanel.SetActive(false);
        detailPanel.Show(config);
    }

    /// <summary>
    /// 刷新角色列表 UI
    /// </summary>
    public void RefreshCharacterList()
    {
        for (int i = 0; i < allCharacters.Count; i++)
        {
            var config = allCharacters[i];
            var card = characterCards[i];
            var progress = PlayerDataManager.Instance.Data
                .GetOrCreateCharacter(config.characterId);

            var button = card.GetComponent<Button>();
            var portrait = card.GetComponent<Image>();

            var nameCard = card.transform.Find("NameCard");
            var nameText = nameCard.Find("Name").GetComponent<TMPro.TMP_Text>();
            var countText = nameCard.Find("Number").GetComponent<TMPro.TMP_Text>();
            var percentText = nameCard.Find("Percent").GetComponent<TMPro.TMP_Text>();

            if (!progress.unlocked)
            {
                portrait.sprite = config.silhouetteSprite;
                nameText.text = "???";
                countText.text = "??/??";
                percentText.text = "??%";
                button.interactable = false;
            }
            else
            {
                portrait.sprite = config.portraitSprite;
                nameText.text = config.displayName;

                int obtained = progress.obtainedItemNums.Count;
                int total = config.collectibles.Count;

                countText.text = $"{obtained}/{total}";
                float percent = total == 0 ? 0 : (float)obtained / total * 100f;
                percentText.text = $"{percent:0}%";

                button.interactable = true;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => ShowCharacterDetail(config));

        }
    }
}
