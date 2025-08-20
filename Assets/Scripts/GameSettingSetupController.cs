using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.Timeline;
using System;

public class GameSettingSetupController : MonoBehaviour
{
    [SerializeField] IslandData[] islandTransformData;
    public static Action OnGameSettingsSetupCompleted = delegate { };

    private void Awake()
    {
        //foreach(IslandData data in islandTransformData)
        //{
        //    data.Initialize();
        //}
    }

    private void Start()
    {
        foreach (IslandData data in islandTransformData)
        {
            data.Initialize();
        }

        if (SettingsDataHandler.Instance.HasSettingsData)
        {
            Date configSettings = SettingsDataHandler.Instance.GetSettingsData();
            int index = 0;
            foreach (Plate plate in configSettings.table.plates)
            {
                float posX = plate.x * 3;
                float posY = -plate.y; //MapZeroTo200ToMinusNToN(plate.y, 300, 200);//
                Vector3 anchoredPos = islandTransformData[index].qrCode.anchoredPosition;
                anchoredPos.x = posX;
                anchoredPos.y = posY;
                islandTransformData[index].qrCode.anchoredPosition = anchoredPos;

                //islandTransformData[index].island.position = islandTransformData[index].qrCode.position + islandTransformData[index].InitPosOffset;

                //Set Rotation of Logo
                Vector3 rotation = islandTransformData[index].island.rotation.eulerAngles;
                rotation.y = 180 - plate.rotation;
                islandTransformData[index].island.rotation = Quaternion.Euler(rotation);

                //Set scale of logo
                islandTransformData[index].island.localScale = Vector3.one * LinearScale(plate.size, 0.7f, 30);

                index++;
            }
        }
        else
        {
            Debug.Log("No settings data found");
        }

        if (OnGameSettingsSetupCompleted != null)
        {
            OnGameSettingsSetupCompleted.Invoke();
        }
    }

    private float LinearScale(float value, float n, float mapper = 200)
    {
        return (value / mapper) * n;
    }
}

[System.Serializable]
public struct IslandData
{
    public RectTransform qrCode;
    public Transform island;
    public Vector3 InitPosOffset { get; private set; }

    public void Initialize()
    {
        InitPosOffset = island.position - qrCode.position;
    }
};
