using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlateData : MonoBehaviour
{
    [SerializeField] Transform ID;
    TextMeshPro idNumberText;

    private int idNumber;
    public int IdNumber { get { return idNumber; } set { idNumber = value; idNumberText.text = idNumber.ToString(); } }

    // Start is called before the first frame update
    void Awake()
    {
        idNumberText = ID.GetComponent<TextMeshPro>(); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
