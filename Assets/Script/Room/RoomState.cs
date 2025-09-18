[System.Serializable]
public class RoomState
{
    public int roomID;
    public int instability;
    public int instabilityThreshold;
    public bool hasEntered;

    public RoomState(int id, int defaultInstability, int threshold)
    {
        roomID = id;
        instability = defaultInstability;
        instabilityThreshold = threshold;
        this.hasEntered = false;
    }

    public bool IsLocked => instability >= instabilityThreshold;
}
