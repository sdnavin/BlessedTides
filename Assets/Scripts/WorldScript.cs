using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WorldScript : MonoBehaviour
{

    [SerializeField]
    UnityEvent onPositioned;


    // Start is called before the first frame update
    public void BringItOn()
    {
        iTween.MoveTo(gameObject, iTween.Hash("y", -2, "time", 1, "islocal", true));
        Invoke("OnReached", 1);
    }

    void OnReached()
    {
        if (onPositioned != null)
        {
            onPositioned.Invoke();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
