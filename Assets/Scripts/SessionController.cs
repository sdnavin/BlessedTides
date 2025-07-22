using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class SessionController : MonoBehaviour
{
    public int GameScene;
    public int UIScene;
    public int SettingsScene;
    public VideoPlayer videoPlayer;

    public string videoURL;
    public string contentType;
    public bool isLooping;
    public GameState currentState;

    private SettingsController settingsController;

    public enum GameState
    {
        StandBy,
        InGame,
        Settings,
        Video
    }

    private void Awake()
    {
       SettingsDataHandler.Instance.Init();
    }
    // Start is called before the first frame update
    void Start()
    {
        //Play Logo video by default
        PlayVideo("logo", WebSocketClient.instance.gameDetails.logo, true);
        currentState = GameState.StandBy;
    }

    // Update is called once per frame
    void PlayVideo(string content, string VideoURL,bool isLoop=false)
    {
        contentType = content;

        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        Scene[] allScenes = SceneManager.GetAllScenes();
        print("T :"+allScenes.Length);
        if (allScenes.Length > 1)
        {
            for (int t = 0; t < allScenes.Length; t++)
            {
                print(allScenes[t].name);
                if (allScenes[t].buildIndex == GameScene || allScenes[t].buildIndex == SettingsScene)
                {
                    SceneManager.UnloadScene(allScenes[t].buildIndex);
                    videoURL = VideoURL;
                    isLooping = isLoop;
                    SceneManager.LoadSceneAsync(UIScene, LoadSceneMode.Additive);
                    return;
                }
            }
        }
        


        if (SceneManager.GetAllScenes().Length > 1)
        {
            videoURL = VideoURL;
            isLooping = isLoop;
            onUISceneLoad();
        }
        else
        {
            SceneManager.LoadSceneAsync(UIScene, LoadSceneMode.Additive);
            videoURL = VideoURL;
            isLooping = isLoop;
        }
    }
    void onUISceneLoad()
    {

        videoPlayer = FindFirstObjectByType<VideoPlayer>();
        if (contentType == "logo")
        {
            RawImage rawImage = videoPlayer.GetComponent<RawImage>();
            rawImage.enabled = false;
            videoPlayer.transform.GetChild(0).gameObject.SetActive(true);
        }
        else
        {
            videoPlayer.transform.GetChild(0).gameObject.SetActive(false);
            RawImage rawImage = videoPlayer.GetComponent<RawImage>();
            rawImage.enabled = true;
        }

        videoPlayer.url = videoURL;
        videoPlayer.isLooping = isLooping;
        videoPlayer.Play();
    }
    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if (arg0.buildIndex == UIScene)
        {
            onUISceneLoad();
        }
    }

    void StartGame(string gameId,float startTime, float duration)
    {
        Scene[] allScenes = SceneManager.GetAllScenes();
        print("T :" + allScenes.Length);
        if (allScenes.Length > 1)
        {
            SceneManager.UnloadSceneAsync(UIScene);
        }
        SceneManager.LoadSceneAsync(GameScene, LoadSceneMode.Additive);
        StartCoroutine(RestartSceneAfterDelay(duration / 1000f)); // convert ms to seconds
    }


    IEnumerator RestartSceneAfterDelay(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);

        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log("Restarting scene: " + currentScene);
        Scene[] allScenes = SceneManager.GetAllScenes();
        print("T :" + allScenes.Length);
        for (int t = 0; t < allScenes.Length; t++)
        {
            AsyncOperation asyncOperation = null;
            if (allScenes[t].buildIndex == UIScene)
            {
                SceneManager.UnloadSceneAsync(UIScene);
            }
            if (allScenes[t].buildIndex == GameScene)
            {
                asyncOperation = SceneManager.UnloadSceneAsync(GameScene);
                if(asyncOperation != null) 
                {
                    asyncOperation.completed += PlayLogoScene;
                }
            }
        }
    }

    private void PlayLogoScene(AsyncOperation asyncOperation)
    {
       PlayVideo("logo", WebSocketClient.instance.gameDetails.logo, true);
    }

    public void HandleAdminCommand(AdminCommandData data)
    {
        switch (data.scene.ToLower())
        {
            case "logo":
                PlayVideo("logo",WebSocketClient.instance.gameDetails.logo,true);
                SetActiveCursor(true);
                currentState = GameState.StandBy;
                break;
            case "video":
                PlayVideo("movie",WebSocketClient.instance.gameDetails.movie);
                SetActiveCursor(true);
                currentState = GameState.Video;
                break;
            case "game":
                StartGame(data.gameId, data.startTime, data.duration);
                SetActiveCursor(false);
                currentState = GameState.InGame;
                break;
            default:
                Debug.LogWarning("Unknown scene type: " + data.scene);
                SetActiveCursor(true);
                break;
        }
    }

    private static void SetActiveCursor(bool value)
    {
        Cursor.visible = value;
    }

    private void ChangeGameState(GameState gameState)
    {
        if (currentState == gameState) return;

        currentState = gameState;
        OnGameStateChanged();
    }

    private void OnGameStateChanged()
    {
        switch(currentState) 
        {
            case GameState.StandBy:
                PlayLogoScene(null);
                SetActiveCursor(true);
                break;

                case GameState.Video:
                break;

                case GameState.InGame:
                break;

            case GameState.Settings:
                OpenSettingsScene();
                SetActiveCursor(true);
                break;
        }
    }

    internal void HandleSettingsCommand(Date date)
    {
        if(currentState == GameState.InGame)
        {
            Debug.Log("Game mode is on: " + currentState);
            return;
        }

        if (date.editMode == true)
        {
            ChangeGameState(GameState.Settings);
        }
        else
        {
            ChangeGameState(GameState.StandBy);
            Debug.Log("Edit mode disabled!");
        }
    }

    private void OpenSettingsScene()
    {
        Scene[] allScenes = SceneManager.GetAllScenes();
        print("T :" + allScenes.Length);
        if (allScenes.Length > 1)
        {
            SceneManager.UnloadSceneAsync(UIScene);
        }
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(SettingsScene, LoadSceneMode.Additive);
        asyncOperation.completed += FindSettingsController;
    }

    private void FindSettingsController(AsyncOperation obj)
    {
        //Bad coding practice ...refactor later
       settingsController = GameObject.FindObjectOfType<SettingsController>();
    }

    public void HandleSettingsUpdation(TableSettings tableSettings)
    {
       if(settingsController != null) 
       {
            SettingsDataHandler.Instance.SaveSettingsData(tableSettings);
            settingsController.UpdateTable(tableSettings);
       }
    }
}
