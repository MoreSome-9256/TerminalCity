[System.Serializable]
public class RoomState
{
    public int roomID;
    public int instability;
    public int instabilityThreshold;

    public RoomState(int id, int defaultInstability, int threshold)
    {
        roomID = id;
        instability = defaultInstability;
        instabilityThreshold = threshold;
    }

    public bool IsLocked => instability >= instabilityThreshold;
}
