using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveWithObj : MonoBehaviour
{
    public GameObject OffsetObj;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       transform.position= OffsetObj.transform.position;
    }
}
