using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnableEvent : MonoBehaviour
{
    [SerializeField]
    UnityEvent onEnable;

    [SerializeField]
    UnityEvent onStart;

    public float startDelay;

    private void OnEnable()
    {
        Invoke("RunIt", startDelay);
    }

    void RunIt()
    {
        onEnable.Invoke();
    }

    void OnStart()
    {
        onStart.Invoke();
    }


    // Start is called before the first frame update
    void Start()
    {
        OnStart();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateReveal(Material portalMaterial)
    {
        portalMaterial.SetFloat("_RevealRadius", 0);

    }
}
