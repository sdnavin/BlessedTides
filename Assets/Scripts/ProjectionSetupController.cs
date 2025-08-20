using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectionSetupController : MonoBehaviour
{

    [SerializeField] QuadWarpController quadWarpController;

    public void SetupQuadCorners()
    {
        if (SettingsDataHandler.Instance.HasSettingsData)
        {
            Date configSettings = SettingsDataHandler.Instance.GetSettingsData();
            UpdateTableCorners(configSettings.table.cornerCoordinates);
        }
    }

    private void UpdateTableCorners(CornerCoordinates cornerCoordinates)
    {
        Vector3 bl = new Vector3(cornerCoordinates.bottomLeft.x, cornerCoordinates.bottomLeft.y, 0f);
        Vector3 br = new Vector3(cornerCoordinates.bottomRight.x, cornerCoordinates.bottomRight.y, 0f);
        Vector3 tr = new Vector3(cornerCoordinates.topRight.x, cornerCoordinates.topRight.y, 0f);
        Vector3 tl = new Vector3(cornerCoordinates.topLeft.x, cornerCoordinates.topLeft.y, 0f);

        quadWarpController.UpdateCorners(bl, br, tr, tl);
    }
}
