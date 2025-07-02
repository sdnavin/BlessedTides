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
    public VideoPlayer videoPlayer;

    public string videoURL;
    public string contentType;
    public bool isLooping;
        // Start is called before the first frame update
    void Start()
    {
        //Play Logo video by default
        PlayVideo("logo", WebSocketClient.instance.gameDetails.logo, true);
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
                if (allScenes[t].buildIndex == GameScene)
                {
                    SceneManager.UnloadScene(GameScene);
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

            if (allScenes[t].buildIndex == UIScene)
            {
                SceneManager.UnloadSceneAsync(UIScene);
            }
            if (allScenes[t].buildIndex == GameScene)
            {
                SceneManager.UnloadSceneAsync(GameScene);
            }
        }
    }
    public void HandleAdminCommand(AdminCommandData data)
    {
        switch (data.scene.ToLower())
        {
            case "logo":
                PlayVideo("logo",WebSocketClient.instance.gameDetails.logo,true);
                SetActiveCursor(true);
                break;
            case "video":
                PlayVideo("movie",WebSocketClient.instance.gameDetails.movie);
                SetActiveCursor(true);
                break;
            case "game":
                StartGame(data.gameId, data.startTime, data.duration);
                SetActiveCursor(false);
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

}
