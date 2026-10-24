using System;

[Serializable]
public class SaveData
{
    public float playerX;
    public float playerY;
    public float playerZ;
    public bool[] taskCompleted;
    public int currentTaskIndex;
    public int playerID;
    public int language;
    public string sceneName;
    public string saveTimestamp;
}
