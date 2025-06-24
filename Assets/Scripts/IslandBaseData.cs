using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandBaseData : MonoBehaviour
{
    [SerializeField] BoatInventory boatInventory;

    public bool HasBoat(BoatInventory boatInventory)
    {
        if(this.boatInventory == boatInventory)
        {
            return true;
        }
        Debug.Log($"Boat {boatInventory.transform.name} does not belong to this Island");
        return false;
    }
}
