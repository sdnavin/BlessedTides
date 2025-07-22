using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class LogoSetupController : MonoBehaviour
{
    [SerializeField] RectTransform[] logoTransforms;

    private void OnEnable()
    {
        if(SettingsDataHandler.Instance.HasSettingsData)
        {
            TableSettings tableSettings = SettingsDataHandler.Instance.GetSettingsData();
            int index = 0;
            foreach(Plate plate in tableSettings.table.plates)
            {
                float posX = LinearScale(plate.x, 960);
                float posY = -LinearScale(plate.y, 550);
                Vector3 anchoredPos = logoTransforms[index].anchoredPosition;
                anchoredPos.x = posX;
                anchoredPos.y = posY;
                logoTransforms[index].anchoredPosition = anchoredPos;
                index++;
            }
        }
        else
        {
            Debug.Log("No settings data found");
        }
    }

    //private float MapZeroTo200ToMinusNToN(float value, float n, float mapper)
    //{
    //    return (value / mapper) * (2f * n) - n;
    //}
    private float LinearScale(float value, float n)
    {
        return (value / 200f) * n;
    }
}
