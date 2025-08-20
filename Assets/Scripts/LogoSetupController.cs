using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogoSetupController : MonoBehaviour
{
    [SerializeField] RectTransform[] logoTransforms;

    private void OnEnable()
    {
        if(SettingsDataHandler.Instance.HasSettingsData)
        {
            Date configSettings = SettingsDataHandler.Instance.GetSettingsData();
            int index = 0;
            foreach(Plate plate in configSettings.table.plates)
            {
                //Set position of Logo parent
                float posX = LinearScale(plate.x, 960);
                float posY = -LinearScale(plate.y, 550);
                Vector3 anchoredPos = logoTransforms[index].anchoredPosition;
                anchoredPos.x = posX;
                anchoredPos.y = posY;
                logoTransforms[index].anchoredPosition = anchoredPos;

                //Set Rotation of Logo
                Vector3 rotation = logoTransforms[index].GetChild(0).transform.rotation.eulerAngles;
                rotation.y = 360 - (180-plate.rotation);
                logoTransforms[index].GetChild(0).transform.rotation = Quaternion.Euler(rotation);

                //Set scale of logo
                logoTransforms[index].GetChild(0).transform.localScale = Vector3.one * LinearScale(plate.size, 1, 30);

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
    private float LinearScale(float value, float n, float mapper = 200)
    {
        return (value / mapper) * n;
    }
}
