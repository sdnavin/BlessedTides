using TMPro;
using UnityEngine;

public class BoatInventory : MonoBehaviour
{
    public int maxBoatCapacity = 50;         // Maximum capacity of the boat
    public int currentBoatLoad = 0;          // Current load on the boat

    public int maxSoulCapacity = 50;         // Maximum capacity of the boat
    public int currentSoulLoad = 0;          // Current load on the boat


    public float itemsToUnloadPerSecond = 10f; // Number of items unloaded per second

    public GameObject[] boatInventoryObjects; // Visual objects in the boat (10 objects)
    public GameObject[] soulInventoryObjects; // Visual objects in the boat (10 objects)
    public BaseInventory baseInventory;      // Reference to the base's inventory
    public SoulInventory soulInventory;      // Reference to the base's inventory
    public BoatController controller;
    private bool isUnloading = false;        // Is the boat currently unloading?
    private float unloadTimer = 0f;          // Timer for the unloading process
    public TextMeshProUGUI uGUI;

    public bool isFull=false;
    public GameObject FullObj;
    private void Start()
    {
        controller = GetComponent<BoatController>();
        // Initially disable all boat inventory objects
        for (int i = 0; i < boatInventoryObjects.Length; i++)
        {
            boatInventoryObjects[i].SetActive(false);
        }

        for (int i = 0; i < soulInventoryObjects.Length; i++)
        {
            soulInventoryObjects[i].SetActive(false);
        }
        UpdateBoatInventoryDisplay();
    }
    [SerializeField]
    GameObject arrowObj;
    public void SetInventoryStatus(bool status)
    {
        FullObj.SetActive(status);
        isFull = status;
        arrowObj.SetActive(status);
        for(int t = 0; t < baseInventory.glowControllers.Length; t++)
        {
            if(status)
            baseInventory.glowControllers[t].StartGlowEffect();
            else
                baseInventory.glowControllers[t].StopGlowEffect();

        }
    }

    public void ResetBoat()
    {
        currentBoatLoad = 0;
        currentSoulLoad = 0;
        UpdateBoatInventoryDisplay();
    }
    // Add resources to the boat
    public bool AddToBoat(int amount)
    {
        if (currentBoatLoad + amount <= maxBoatCapacity)
        {
            currentBoatLoad += amount;
            Debug.Log($"Added {amount} resources to the boat. Current load: {currentBoatLoad}/{maxBoatCapacity}");
            controller.updateFishCatch(currentBoatLoad);
            UpdateBoatInventoryDisplay(); // Update boat inventory visuals
            if (currentBoatLoad >= maxBoatCapacity)
            {
                SetInventoryStatus(true);
            }
            else
            {
                SetInventoryStatus(false);
            }
            return true; // Successfully added
        }
        else
        {
            SetInventoryStatus(true);
            Debug.Log("Boat is full! Cannot add more resources.");
            return false; // Failed to add
        }
    }

    public bool AddToSoul (int amount)
    {
        if (currentSoulLoad + amount <= maxSoulCapacity)
        {
            currentSoulLoad += amount;
            Debug.Log($"Added {amount} resources to the boat. Current load: {currentSoulLoad}/{maxSoulCapacity}");
            UpdateBoatInventoryDisplay(); // Update boat inventory visuals
            return true; // Successfully added
        }
        else
        {
            Debug.Log("Boat is full! Cannot add more resources.");
            return false; // Failed to add
        }
    }

    // Trigger-based unloading when near the base
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Base") && other.TryGetComponent<IslandBaseData>(out IslandBaseData islandBase)) // Ensure the colliding object is the base
        {
            if(islandBase.HasBoat(this))
            {
                Debug.Log("Boat reached the base. Starting unloading...");
                StartUnloading();
            }
        }

        if (other.CompareTag("Souls")) // Ensure the colliding object is the base
        {
            bool collected=AddToSoul(1);
            if(collected)
            other.gameObject.SetActive(false);
            UpdateSoulInventoryDisplay();
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Base") && other.TryGetComponent<IslandBaseData>(out IslandBaseData islandBase)) // Ensure the colliding object is the base
        {
            if (islandBase.HasBoat(this))
            {
                //Debug.Log("Boat left the base. Stopping unloading...");
                StopUnloading();
            }
        }
    }

    // Start unloading process
    private void StartUnloading()
    {
        if (!isUnloading && currentBoatLoad > 0 && baseInventory != null)
        {
            isUnloading = true;
            unloadTimer = 0f;
        }
        if (currentSoulLoad > 0)
        {
            bool isuloaded = soulInventory.AddToBase(currentSoulLoad);
            if (isuloaded)
            {
                currentSoulLoad = 0;
                UpdateSoulInventoryDisplay();
            }
            else
            {
                Debug.Log("Base Full");
            }
        }
    }

    // Stop unloading process
    private void StopUnloading()
    {
        isUnloading = false;
    }

    private void Update()
    {
        if (isUnloading && currentBoatLoad > 0 && baseInventory != null)
        {
            UnloadInventory();
        }
    }

    // Unload inventory gradually
    private void UnloadInventory()
    {
        // Unload items over time
        float unloadAmount = itemsToUnloadPerSecond ;

        if (unloadAmount > currentBoatLoad)
        {
            unloadAmount = currentBoatLoad; // Ensure we don't unload more than available
        }
        bool addedToBase = baseInventory.AddToBase((int)unloadAmount);
        if (addedToBase)
        {
            controller.updateFishCount(baseInventory.currentBaseLoad, (int)unloadAmount);

            currentBoatLoad -= (int)unloadAmount; // Remove from boat inventory
            UpdateBoatInventoryDisplay();       // Update boat inventory visuals

            Debug.Log($"Unloading... Remaining boat load: {currentBoatLoad}/{maxBoatCapacity}");

            if (currentBoatLoad == 0)
            {
                isUnloading = false; // Stop unloading when the boat is empty
                Debug.Log("Unloading complete.");
                SetInventoryStatus(false);
            }
        }
    }

    // Update the visual display for boat inventory (enable/disable objects based on percentage)
    void UpdateBoatInventoryDisplay()
    {
        float inventoryPercentage = (float)currentBoatLoad / maxBoatCapacity;

        for (int i = 0; i < boatInventoryObjects.Length; i++)
        {
            if (i < inventoryPercentage * boatInventoryObjects.Length)
            {
                boatInventoryObjects[i].SetActive(true); // Enable object
            }
            else
            {
                boatInventoryObjects[i].SetActive(false); // Disable object
            }
        }
        uGUI.text = currentBoatLoad + "/" + maxBoatCapacity;
    }

    void UpdateSoulInventoryDisplay()
    {

        for (int i = 0; i < soulInventoryObjects.Length; i++)
        {
            if (i < currentSoulLoad)
            {
                soulInventoryObjects[i].SetActive(true); // Enable object
            }
            else
            {
                soulInventoryObjects[i].SetActive(false); // Disable object
            }
        }
    }
}
