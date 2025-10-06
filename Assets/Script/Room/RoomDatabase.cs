using UnityEngine;

[System.Serializable]
public class RoomInfo
{
    public int roomID;
    public string roomName;
    public GameObject prefab;   // 直接拖 prefab 进来
    public bool unlocked = false;
    public bool isCorridor = false;
}

[CreateAssetMenu(fileName = "RoomDatabase", menuName = "Game/RoomDatabase")]
public class RoomDatabase : ScriptableObject
{
    public RoomInfo[] rooms;

    public RoomInfo GetRoomInfo(int roomID)
    {
        foreach (var room in rooms)
        {
            if (room.roomID == roomID)
                return room;
        }
        return null;
    }
}
