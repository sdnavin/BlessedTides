using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.Timeline;
using System;

public class GameSettingSetupController : MonoBehaviour
{
    [SerializeField] RectTransform[] qrCodeTransforms;
    public static Action OnGameSettingsSetupCompleted = delegate { };

    private void Start()
    {
        if (SettingsDataHandler.Instance.HasSettingsData)
        {
            TableSettings tableSettings = SettingsDataHandler.Instance.GetSettingsData();
            int index = 0;
            foreach (Plate plate in tableSettings.table.plates)
            {
                float posX = plate.x * 3;
                float posY = -plate.y; //MapZeroTo200ToMinusNToN(plate.y, 300, 200);//
                Vector3 anchoredPos = qrCodeTransforms[index].anchoredPosition;
                anchoredPos.x = posX;
                anchoredPos.y = posY;
                qrCodeTransforms[index].anchoredPosition = anchoredPos;
                index++;
            }
        }
        else
        {
            Debug.Log("No settings data found");
        }

        if(OnGameSettingsSetupCompleted != null)
        {
            OnGameSettingsSetupCompleted.Invoke();
        }
    }

    private float MapZeroTo200ToMinusNToN(float value, float n, float mapper)
    {
        return (value / mapper) * (2f * n) - n;
    }
}
