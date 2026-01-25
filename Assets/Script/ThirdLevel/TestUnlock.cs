using UnityEngine;

// 仅测试使用！打包的时候记得关这个！
public class TestUnlock : MonoBehaviour
{
    public string characterId;

    public GameObject Level3;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U)) // 按 U 键解锁
        {
            Level3.SetActive(true);
            bool unlocked = PlayerDataManager.Instance.UnlockCharacter(characterId);
            Level3.SetActive(false);
            if (unlocked)
                Level3UIController.Instance.RefreshCharacterList();

            Debug.Log($"角色 {characterId} 解锁测试触发");
        }
    }
}
