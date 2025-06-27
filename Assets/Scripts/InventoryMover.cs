using UnityEngine;

public class InventoryMover : MonoBehaviour
{
    public BaseInventory baseInventory;       // Reference to the BaseInventory script
    public Transform mainWarehousePosition;   // Position to deliver inventory objects
    public Transform startPosition;           // Position to return after delivery
    public Transform carryPoint;              // Local point where the player carries the inventory object
    public float moveSpeed = 5f;              // Speed of character movement

    private GameObject targetInventoryObject; // The inventory object to move
    private Vector3 destination;              // Current destination
    private bool isMovingToPickup = false;    // Flag to indicate moving to pickup state
    private bool isDelivering = false;        // Flag to indicate delivering state
    private bool isReturning = false;         // Flag to indicate returning state

    public Animator animator;                 // Reference to the Animator component
    [SerializeField] ParticleSystem celebrationFX;

    private void Start()
    {
        // Subscribe to the event from BaseInventory
        baseInventory.OnBaseFullyLoaded += HandleFullyLoadedBase;
    }

    private void OnEnable()
    {
        baseInventory.OnBaseLoadMaxedOut += Celebrate;
    }

    private void OnDisable()
    {
        baseInventory.OnBaseLoadMaxedOut -= Celebrate;
    }

    private void Update()
    {
        if (isMovingToPickup || isDelivering || isReturning)
        {
            MoveTowardsDestination();
        }
        else
        {
            // Set speed to 0 when not moving
            UpdateAnimator(0f);
        }
    }

    // Handle the fully loaded base inventory event
    private void HandleFullyLoadedBase(GameObject inventoryObject)
    {
        if (!isMovingToPickup && !isDelivering && !isReturning)
        {
            targetInventoryObject = inventoryObject;
            destination = targetInventoryObject.transform.position; // Move to the inventory object
            isMovingToPickup = true;
        }

        Celebrate();
    }

    [ContextMenu("Do Celebrate")]
    private void Celebrate()
    {
        animator.SetTrigger("Dance");
        celebrationFX.Play();
    }

    // Move towards the current destination
    private void MoveTowardsDestination()
    {
        Vector3 direction = (destination - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        if (direction.magnitude > 0.1f)
        {
            // Rotate the character towards the moving direction
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, moveSpeed * Time.deltaTime);
        }

        // Update animator for moving state
        UpdateAnimator(moveSpeed);

        // Check if reached destination
        if (Vector3.Distance(transform.position, destination) < 0.1f)
        {
            if (isMovingToPickup)
            {
                PickUpInventoryObject();
            }
            else if (isDelivering)
            {
                DeliverInventoryObject();
            }
            else if (isReturning)
            {
                FinishReturning();
            }
        }
    }

    // Pick up the inventory object
    private void PickUpInventoryObject()
    {
        //Debug.Log($"Picked up {targetInventoryObject.name}.");

        // Attach the inventory object to the player
        targetInventoryObject.transform.SetParent(carryPoint);
        targetInventoryObject.transform.localPosition = Vector3.zero;  // Place it exactly at the carryPoint
        targetInventoryObject.transform.localRotation = Quaternion.identity; // Reset rotation

        isMovingToPickup = false;

        // Set destination to the main warehouse
        destination = mainWarehousePosition.position;
        isDelivering = true;

        // Update animator for moving state
        UpdateAnimator(moveSpeed);
    }

    // Deliver the inventory object to the warehouse
    private void DeliverInventoryObject()
    {
        Debug.Log($"Delivered {targetInventoryObject.name} to the warehouse!");

        // Detach the inventory object and place it at the warehouse
        targetInventoryObject.transform.SetParent(null);
        targetInventoryObject.transform.position = mainWarehousePosition.position;

        targetInventoryObject = null; // Clear the target
        isDelivering = false;

        // Set destination to return to the starting position
        destination = startPosition.position;
        isReturning = true;

        // Update animator for returning state
        UpdateAnimator(moveSpeed);
    }

    // Handle the completion of returning to the start position
    private void FinishReturning()
    {
        Debug.Log("Returned to the starting position.");
        isReturning = false; // Stop returning
        UpdateAnimator(0f); // Update animator for idle state
    }

    // Update the animator with the given speed
    private void UpdateAnimator(float speed)
    {
        if (animator != null)
        {
            animator.SetFloat("speed", speed);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event to avoid memory leaks
        baseInventory.OnBaseFullyLoaded -= HandleFullyLoadedBase;
    }
}
