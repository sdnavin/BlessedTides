using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using NativeWebSocket;
using System.IO;
using System.Collections.Generic;

[System.Serializable]
public class AdminCommandData
{
    public string messageType;
    public string gameId;
    public string scene;
    public long startTime;
    public long duration;
}

[System.Serializable]
public class Root
{
    public string Status { get; set; }

    public Date Date { get; set; }
}

[System.Serializable]
public class Date
{
    public string messageType;
    public string tableId;
    public string gameId;

    public bool editMode;

    public Table table;
    public ContentData contentData;

    public List<Plate> plates;//public PlateSettings plates;

    public bool last;

    public string action;
    public string timestamp;
    public string source;

    public long startTime;
    public long duration;
}

[System.Serializable]
public class ContentData
{
    public string type;       // e.g., "game"
    public string gameId;
    public string timestamp; // e.g., "2025-08-20T07:00:49.474Z"
}


[System.Serializable]
public class Table
{
    public string id;

    public string shape;

    public int width;

    public int height;

    public string status;

    public CornerCoordinates cornerCoordinates;
    public List<Plate> plates;
}

[System.Serializable]
public class PlateSettings
{
    public List<Plate> plates;
}

[System.Serializable]
public class Plate
{
    public string id;

    public int x;

    public int y;

    public int size;

    public int rotation;

    public string tableId;
}

[System.Serializable]
public class CornerCoordinates
{
    public Point topLeft;
    public Point topRight;
    public Point bottomRight;
    public Point bottomLeft;
}

[System.Serializable]
public class Point
{
    public float x;
    public float y;
}

//=================================================================================//

[System.Serializable]
public class WebSocketMessage
{
    public string status;
    public AdminCommandData data;
    public Date date;
}


[System.Serializable]
public class FishUpdateMessage
{
    public string messageType;
    public int totalFish;
    public int fishCount;
    public int slotId;
    public string userId;
    public string gameId;

    public FishUpdateMessage(string messageType, int totalFish, int fishCount, int slotId, string userId,string gameId)
    {
        this.messageType = messageType;
        this.totalFish = totalFish;
        this.fishCount = fishCount;
        this.slotId = slotId;
        this.userId = userId;
        this.gameId = gameId;
    }
}
[System.Serializable]
public class GameDetails
{
    public string logo;
    public string movie;
    public string gameId;
    public string gameStart;
    public string currentState;
    public string wssurl;
}

[System.Serializable]
public class GameMessage
{
    public string type;
    public string gameId;
    public int slotNumber;
    public int userId;

    public GameMessage(string messageType, string gameId, int slotNumber=-1,int userId=-1)
    {
        this.type = messageType;
        this.gameId = gameId;
        this.slotNumber = slotNumber;
        this.userId = userId;
    }
}

public class WebSocketClient : MonoBehaviour
{
    [SerializeField]
    public WorldScript[] worldScripts;
    [SerializeField]
    public UserData[] userData;
    private WebSocket websocket;
    public  string serverUrl = "ws://localhost:8080"; // Change this to your WebSocket server URL
    private bool isConnecting = false;
    private bool Connected = false;
    private float reconnectDelay = 5f;
    private bool shouldReconnect = true;

    public static WebSocketClient instance;
    bool gameStart;

    [SerializeField]
    private bool autoReconnect = true;

    public static DataIn[] dataIn;
    public DataIn dataInVis;
    public GameState gameState;

    private float lastUpdateTime; // Tracks the last time data was updated
    private float resetTimeThreshold = 0.5f; // 500ms threshold for resetting

    private bool isConnectedtoJoystick = false;
    [SerializeField]
    UnityEvent OnConnected;
    [SerializeField]
    string uniqueID;
    [SerializeField]
    int slotNumber;

    [SerializeField]
    public GameDetails gameDetails;

    [SerializeField]
    public WebSocketMessage webSocketMessage;

    [SerializeField]
    SessionController sessionController;
    private void Awake()
    {

        // Ensure only one instance exists
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject); // Optional: Keep across scenes

        string dataInString = File.ReadAllText(Application.dataPath + "/data.txt");
        print(dataInString);
        gameDetails = JsonUtility.FromJson<GameDetails>(dataInString);


        serverUrl = gameDetails.wssurl;



          dataIn = new DataIn[4];
        userData = new UserData[4];
        //uniqueID = SystemInfo.deviceUniqueIdentifier;
        Debug.Log("Device Unique ID: " + uniqueID);

        InitializeWebSocket();
        StartCoroutine(ConnectToServer());
    }

    private void InitializeWebSocket()
    {
        websocket = new WebSocket(serverUrl);

        websocket.OnOpen += () =>
        {
            Debug.Log("[WebSocket] Connected to server!");

            isConnecting = false;
            Connected = true;
        };

        websocket.OnError += (e) =>
        {
            Debug.LogError("[WebSocket] Error: " + e);
            isConnecting = false;
            Connected = false;
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("[WebSocket] Connection closed!");
            isConnecting = false;
            Connected = false;
            if (autoReconnect && shouldReconnect)
            {
                StartCoroutine(ReconnectWithDelay());
            }
        };

        websocket.OnMessage += (bytes) =>
        {
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            print("OnMessage :"+message);
            ProcessReceivedData(message);
        };
    }

    private IEnumerator ConnectToServer()
    {
        while (true)
        {
            if (websocket.State == WebSocketState.Closed && !isConnecting)
            {
                isConnecting = true;
                Debug.Log("[WebSocket] Attempting to connect...");

                    yield return new WaitForSeconds(0.1f); // Small delay before connecting
                    websocket.Connect();
            }
            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator ReconnectWithDelay()
    {
        Debug.Log($"[WebSocket] Attempting to reconnect in {reconnectDelay} seconds...");
        yield return new WaitForSeconds(reconnectDelay);

        if (websocket.State == WebSocketState.Closed)
        {
            InitializeWebSocket();
        }
    }
    int activeWorlds = 0;
    private void ProcessReceivedData(string data)
    {
        if (slotNumber == 0)
        {
            try
            {
                gameState = JsonUtility.FromJson<GameState>(data);
                if (gameState.slotNumber!=null&&gameState.gameStatus.Length > 0)
                {
                    slotNumber = int.Parse(gameState.slotNumber);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[WebSocket] Error processing data: " + e.Message);
            }
        }
        try
        {
            Debug.Log("[WebSocket] Received: " + data);
            try
            {
                webSocketMessage = JsonUtility.FromJson<WebSocketMessage>(data);
                Debug.Log("[WebSocket] Received Json: " + JsonUtility.ToJson(webSocketMessage));

                if (webSocketMessage != null)//&&webSocketMessage.status != "userJoined"
                {
                    switch(webSocketMessage.status)
                    {
                        case "update_edit_mode":
                            sessionController.HandleSettingsCommand(webSocketMessage.date);
                            break;
                            
                        case "table_settings_update"://plate_settings_update
                            
                            sessionController.HandleSettingsUpdation(webSocketMessage.date.table);
                            break;
                        case "plate_settings_update":
                            sessionController.HandleSettingsUpdation(webSocketMessage.date.plates);
                            break;

                        case "userJoined":
                            break;

                        case "content_update":
                            sessionController.HandleAdminCommand(webSocketMessage.date);
                            break;

                        default:
                            //do admin action 
                            //sessionController.HandleAdminCommand(webSocketMessage.date);
                            return;
                    }
                    
                  //  return;
                }
            }
            catch(Exception e)
            {
                Debug.Log("not websockmessage");
            }

            dataInVis = JsonUtility.FromJson<DataIn>(data);
           
            if (dataInVis.status == "userJoined")
            {
                WorldScript[] AllworldScripts = FindObjectsOfType<WorldScript>();
                for(int t=0;t< AllworldScripts.Length; t++)
                {
                    worldScripts[int.Parse(AllworldScripts[t].name)-1] = AllworldScripts[t];
                }
                OnConnected.Invoke();
                isConnectedtoJoystick = true;
                print(dataInVis.user.slotId);
                activeWorlds = (dataInVis.user.slotId)-1;
                userData[activeWorlds] = dataInVis.user;
                print(activeWorlds);
                UIHandler.instance.closeUI(activeWorlds);

                worldScripts[activeWorlds].BringItOn();
            }
            //if (dataInVis.received.type!=null&&dataInVis.received.type.Length>0)
            //{
            //    OnConnected.Invoke();
            //    isConnectedtoJoystick = true;
            //}
            if (dataInVis.data != null&& dataInVis.data.slotId>0)
            {
                userData[dataInVis.data.slotId - 1].userId= dataInVis.data.userId;
                dataIn[dataInVis.data.slotId - 1] = dataInVis;
            }
            lastUpdateTime = Time.time;
            // Add your data processing logic here
            // Example: Parse JSON data
            // var parsedData = JsonUtility.FromJson<YourDataType>(data);
        }
        catch (Exception e)
        {
            Debug.LogError("[WebSocket] Error processing data: " + e.Message);
        }
    }



    public async void SendMessage(string message)
    {
        if (websocket.State == WebSocketState.Open)
        {
            try
            {
                await websocket.SendText(message);
                Debug.Log("[WebSocket] Sent: " + message);
            }
            catch (Exception e)
            {
                Debug.LogError("[WebSocket] Error sending message: " + e.Message);
            }
        }
        else
        {
            Debug.LogWarning("[WebSocket] Cannot send message - connection is not open");
        }
    }

    private void Update()
    {
        if (websocket != null)
        {
            websocket.DispatchMessageQueue();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnConnected.Invoke();
        }
        if (!gameStart && Connected)
        {
            gameStart = true;
            SendGameStartMessage();
        }
        //if (Time.time - lastUpdateTime > resetTimeThreshold)
        //{
        //    dataInVis = null;
        //    dataIn = dataInVis;
        //}
    }

    private async void OnDisable()
    {
        shouldReconnect = false; // Prevent auto reconnection when disabled

        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.Close();
        }
    }

    private void OnApplicationQuit()
    {
        shouldReconnect = false;
        StopAllCoroutines();
    }

    // Public methods for external control
    public bool IsConnected()
    {
        return websocket != null && websocket.State == WebSocketState.Open;
    }

    public void Disconnect()
    {
        shouldReconnect = false;
        StopAllCoroutines();
        if (websocket != null)
        {
            websocket.Close();
        }
    }


    public void SetAutoReconnect(bool value)
    {
        autoReconnect = value;
    }
    public void SendGameStartMessage()
    {
        GameMessage message = new GameMessage("gameStart", uniqueID, slotNumber);
        string jsonMessage = JsonUtility.ToJson(message);
        SendMessage(jsonMessage);
    }
    public void SendGameEndMessage()
    {
        GameMessage message = new GameMessage("gameEnd", uniqueID);
        string jsonMessage = JsonUtility.ToJson(message);
        SendMessage(jsonMessage);
    }

    public void UserJoinedMessage()
    {
        GameMessage message = new GameMessage("userJoin", uniqueID, slotNumber, -1);
        string jsonMessage = JsonUtility.ToJson(message);
        SendMessage(jsonMessage);
    }

    public void UserLeftMessage()
    {
        GameMessage message = new GameMessage("userLeft", uniqueID, slotNumber, -1);
        string jsonMessage = JsonUtility.ToJson(message);
        SendMessage(jsonMessage);
    }
    public void SendFishUpdateMessage(int totalFish, int fishCount, int slotId)
    {
        // Create a new class to represent the fish update message structure

        // Create the message with the specified parameters //catchFish
        FishUpdateMessage message = new FishUpdateMessage("updateFish", totalFish, fishCount, slotId, userData[slotId-1].userId, uniqueID);

    // Convert the message to JSON
    string jsonMessage = JsonUtility.ToJson(message);

    // Send the message using your existing SendMessage method
    SendMessage(jsonMessage);
}
    public void SendFishCatchUpdateMessage(int totalFish, int fishCount, int slotId)
    {
        // Create a new class to represent the fish update message structure

        // Create the message with the specified parameters //catchFish
        FishUpdateMessage message = new FishUpdateMessage("catchFish", totalFish, fishCount, slotId, userData[slotId - 1].userId, uniqueID);

        // Convert the message to JSON
        string jsonMessage = JsonUtility.ToJson(message);

        // Send the message using your existing SendMessage method
        SendMessage(jsonMessage);
    }
    public void SetServerUrl(string newUrl)
    {
        if (serverUrl != newUrl)
        {
            serverUrl = newUrl;
            if (websocket != null && websocket.State == WebSocketState.Open)
            {
                Disconnect();
                InitializeWebSocket();
                StartCoroutine(ConnectToServer());
            }
        }
    }
}