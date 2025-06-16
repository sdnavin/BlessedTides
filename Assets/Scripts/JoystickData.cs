using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class JoystickData
{
    public string direction;
    public string message;
    public string userId;
    public string type;
    public double x;
    public double y;
    public int distance;
    public int slotId;
}
[System.Serializable]

public class UserData
{
    public string type;
    public string gameId;
    public int slotId;
    public string userId;
}
[System.Serializable]
public class DataIn
{
    public string status;
    public JoystickData data;
    public UserData user;
}
[System.Serializable]

public class GameState
{
    public string gameStatus;
    public string slotNumber;
}


