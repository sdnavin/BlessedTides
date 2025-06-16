using UnityEngine;

public class PortalManager : MonoBehaviour
{
    public Transform portal;               // The portal transform
    public GameObject world1Objects;       // Objects for world 1
    public GameObject world2Objects;       // Objects for world 2
    public float moveSpeed = 5f;           // Speed of portal movement
    public float stopThreshold = 1f;       // Threshold distance to stop portal movement
    public float worldSwitchDelay = 1f;    // Delay before switching worlds after portal reaches the camera
    public float maxScale = 5f;            // Maximum scale of the portal when it reaches the camera
    public float scaleSpeed = 1f;          // Speed of scaling the portal
    private bool isBoatInside = false;     // To check if the boat is inside the portal
    private Camera mainCamera;             // The main camera reference

    void Start()
    {
        mainCamera = Camera.main;          // Get the main camera
    }

    // This method will be triggered by the PortalCollision script
    public void StartPortalGrowth()
    {
        // Start the portal transition (e.g., scaling and moving towards the camera)
        isBoatInside = true;  // Flag to indicate that the portal should grow
        Invoke("SwitchWorlds", worldSwitchDelay);
    }

    void Update()
    {
        // If the boat is inside the portal, handle the movement and scaling
        if (isBoatInside)
        {
            //MovePortalTowardsCamera();
        }
    }

    void MovePortalTowardsCamera()
    {
        // Move portal towards the camera
        Vector3 directionToCamera = (mainCamera.transform.position - portal.position).normalized;
        portal.position += directionToCamera * moveSpeed * Time.deltaTime;

        // Make the portal always face the camera
        portal.rotation = Quaternion.LookRotation(mainCamera.transform.position - portal.position);

        // Scale the portal as it moves toward the camera
        ScalePortal();

        // Check if the portal is close enough to the camera to switch worlds
        if (Vector3.Distance(portal.position, mainCamera.transform.position) <= stopThreshold)
        {
            // Switch the worlds after the portal reaches the camera
            Invoke("SwitchWorlds", worldSwitchDelay);
            portal.gameObject.SetActive(false);  // Hide the portal after transition
        }
    }

    void ScalePortal()
    {
        // Scale the portal gradually as it moves towards the camera
        float targetScale = Mathf.MoveTowards(portal.localScale.x, maxScale, scaleSpeed * Time.deltaTime);
        portal.localScale = new Vector3(targetScale, targetScale, targetScale);
    }

    void SwitchWorlds()
    {
        // Disable world 1 objects and enable world 2 objects
        world1Objects.SetActive(false);
        world2Objects.SetActive(true);
    }
}
