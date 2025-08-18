using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class SettingsController : MonoBehaviour
{
    [SerializeField] GameObject platePrefab;
    [SerializeField] Transform setupRoot;

    public void OpenSettingsScene()
    {

    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void UpdateTable(Table table)
    {
        //ClearRoot();
        //List<Plate> plates = tableSettings.table.plates;
        //for(int i = 0; i < plates.Count; i++)
        //{
        //    GameObject plateObject = Instantiate(platePrefab, setupRoot);
        //    Vector3 pos = plateObject.transform.position;
        //    pos.x = MapZeroTo200ToMinusNToN(plates[i].x, 20, 200);
        //    pos.z = -MapZeroTo200ToMinusNToN(plates[i].y, 8, 200);

        //    Vector3 rotation = plateObject.transform.rotation.eulerAngles;
        //    rotation.y = plates[i].rotation;

        //    plateObject.transform.SetPositionAndRotation(pos, Quaternion.Euler(rotation));
        //    plateObject.transform.localScale = Vector3.one * MapZeroTo200ToMinusNToN(plates[i].size, 1, 30);
        //    if(plateObject.TryGetComponent<PlateData>(out  PlateData data))
        //    {
        //        data.IdNumber = i;
        //    }
        //}
        //Debug.Log("Table update completed!");
    }

    public void UpdatePlates(List<Plate> plateSettings)
    {
        ClearRoot();
        List<Plate> plates = plateSettings;
        for (int i = 0; i < plates.Count; i++)
        {
            GameObject plateObject = Instantiate(platePrefab, setupRoot);
            Vector3 pos = plateObject.transform.position;
            pos.x = MapZeroTo200ToMinusNToN(plates[i].x, 20, 200);
            pos.z = -MapZeroTo200ToMinusNToN(plates[i].y, 8, 200);

            Vector3 rotation = plateObject.transform.rotation.eulerAngles;
            rotation.y = plates[i].rotation;

            plateObject.transform.SetPositionAndRotation(pos, Quaternion.Euler(rotation));
            plateObject.transform.localScale = Vector3.one * MapZeroTo200ToMinusNToN(plates[i].size, 1, 30);
            if (plateObject.TryGetComponent<PlateData>(out PlateData data))
            {
                data.IdNumber = i;
            }
        }
        Debug.Log("Plate update completed!");
    }

    private float MapZeroTo200ToMinusNToN(float value, float n, float mapper)
    {
        return (value / mapper) * (2f * n) - n;
    }

    public void ClearRoot()
    {
        if(setupRoot.childCount > 0)
        {
            foreach(Transform t in setupRoot)
            {
                if(t.TryGetComponent<PlateData>(out PlateData plate))
                {
                    Destroy(t.gameObject);
                }
            }
        }
    }
}
