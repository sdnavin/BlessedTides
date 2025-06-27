using UnityEngine;
using System;
using TMPro;
public class BaseInventory : MonoBehaviour
{
    public int maxBaseCapacity = 50;         // Maximum capacity of the base
    public int currentBaseLoad = 0;          // Current load in the base
    public GameObject[] baseInventoryObjects; // Visual objects in the base (10 objects, each with 5 children)

    public event Action OnBaseLoadMaxedOut = delegate { }; //Event triggered when the inventory has reached maxBaseCapacity
    public event Action<GameObject> OnBaseFullyLoaded; // Event triggered when a base object is fully loaded
    public TextMeshProUGUI invventoryUI;

    public GlowController[] glowControllers;
    private void Start()
    {
        // Disable all parent and child objects initially
        for (int i = 0; i < baseInventoryObjects.Length; i++)
        {
            baseInventoryObjects[i].SetActive(false);

            // Disable all children of the base inventory objects
            for (int j = 0; j < baseInventoryObjects[i].transform.childCount; j++)
            {
                baseInventoryObjects[i].transform.GetChild(j).gameObject.SetActive(false);
            }
        }
        updateUI();
    }
    public void updateUI()
    {
        invventoryUI.text = currentBaseLoad + "/" + maxBaseCapacity;
    }
    // Add resources to the base
    public bool AddToBase(int amount)
    {
        int spaceLeft = maxBaseCapacity - currentBaseLoad;
        int amountToAdd = Mathf.Min(spaceLeft, amount);

        if (amountToAdd > 0)
        {
            currentBaseLoad += amountToAdd;
            UpdateBaseInventoryDisplay(); // Update base inventory visuals
            if(currentBaseLoad == maxBaseCapacity)
            {
                //OnBaseLoadMaxedOut
                OnBaseLoadMaxedOut?.Invoke();
            }
            return true;
        }
        else
        {
            Debug.Log("Base is full! Cannot add more resources.");
            return false;
        }
    }

    // Update the visual display for base inventory
    void UpdateBaseInventoryDisplay()
    {
        int remainingLoad = currentBaseLoad; // Resources yet to distribute

        for (int i = 0; i < baseInventoryObjects.Length; i++)
        {
            bool wasFullyLoaded = IsFullyLoaded(baseInventoryObjects[i]);

            if (remainingLoad > 0)
            {
                baseInventoryObjects[i].SetActive(true); // Enable the parent object

                for (int j = 0; j < baseInventoryObjects[i].transform.childCount; j++)
                {
                    if (remainingLoad > 0)
                    {
                        baseInventoryObjects[i].transform.GetChild(j).gameObject.SetActive(true); // Enable child object
                        remainingLoad--;
                    }
                    else
                    {
                        baseInventoryObjects[i].transform.GetChild(j).gameObject.SetActive(false); // Disable child object
                    }
                }
            }
            else
            {
                baseInventoryObjects[i].SetActive(false); // Disable parent object and its children
                for (int j = 0; j < baseInventoryObjects[i].transform.childCount; j++)
                {
                    baseInventoryObjects[i].transform.GetChild(j).gameObject.SetActive(false);
                }
            }

            // Check if the object became fully loaded
            if (!wasFullyLoaded && IsFullyLoaded(baseInventoryObjects[i]))
            {
                OnBaseFullyLoaded?.Invoke(baseInventoryObjects[i]);
            }
        }
        updateUI();
    }

    // Check if all children of a base object are active
    bool IsFullyLoaded(GameObject baseObject)
    {
        foreach (Transform child in baseObject.transform)
        {
            if (!child.gameObject.activeSelf)
                return false;
        }
        return true;
    }
}
