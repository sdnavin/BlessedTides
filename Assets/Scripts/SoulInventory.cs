using UnityEngine;

public class SoulInventory : MonoBehaviour
{
    public int maxBaseCapacity = 50;        // Maximum capacity of the base
    public int currentBaseLoad = 0;         // Current load in the base

    public GameObject[] baseInventoryObjects; // Visual objects in the base (10 objects)

    private void Start()
    {
        if (baseInventoryObjects != null)
        {
            for (int i = 0; i < baseInventoryObjects.Length; i++)
            {
                baseInventoryObjects[i].SetActive(false);
            }
        }
    }
    // Add resources to the base
    public bool AddToBase(int amount)
    {
        if (currentBaseLoad + amount <= maxBaseCapacity)
        {
            currentBaseLoad += amount;
            //Debug.Log($"Added {amount} resources to the base. Current load: {currentBaseLoad}/{maxBaseCapacity}");
            UpdateBaseInventoryDisplay(); // Update base inventory visuals
        }
        else
        {
            return false;
            Debug.Log("Base is full! Cannot add more resources.");
        }
        return true;
    }

    // Update the visual display for base inventory (enable/disable objects based on percentage)
    void UpdateBaseInventoryDisplay()
    {
        // Calculate the inventory percentage
        float inventoryPercentage = (float)currentBaseLoad / maxBaseCapacity;

        // Enable/disable base inventory objects based on percentage
        for (int i = 0; i < baseInventoryObjects.Length; i++)
        {
            if (i < inventoryPercentage * baseInventoryObjects.Length)
            {
                baseInventoryObjects[i].SetActive(true); // Enable object
            }
            else
            {
                baseInventoryObjects[i].SetActive(false); // Disable object
            }
        }
    }
}
