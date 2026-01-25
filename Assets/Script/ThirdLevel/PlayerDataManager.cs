using System.Collections.Generic;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    public PlayerData Data { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitNewGame();   // 现在先当“新游戏”
    }

    void InitNewGame()
    {
        Data = new PlayerData();
        Data.InitEmpty();
    }
    public bool UnlockCharacter(string characterId)
    {
        var cp = Data.GetOrCreateCharacter(characterId);
        if (cp.unlocked) return false;

        // Debug.Log("解锁角色");
        cp.unlocked = true;
        return true;
    }

}
