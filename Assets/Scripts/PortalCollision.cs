using UnityEngine;

public class PortalCollision : MonoBehaviour
{
    public PortalManager portalManager; // Reference to the PortalManager script

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Boat"))  // Ensure the boat is the one colliding
        {
            // Notify the PortalManager to start the portal effect
            portalManager.StartPortalGrowth();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // You can handle actions when the boat exits the portal area if needed
    }
}
