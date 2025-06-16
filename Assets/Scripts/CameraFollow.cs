using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform boat;              // Reference to the boat's transform
    public float followSpeed = 5f;      // Speed at which the camera follows the boat
    public float followDistance = 10f;  // Distance the camera maintains from the boat
    public float height = 10f;          // Height of the camera above the boat

    public float zoomSpeed = 5f;        // Speed of zooming in and out
    public float zoomInFOV = 50f;      // Field of view when the boat is stopped (zoomed in)
    public float zoomOutFOV = 60f;     // Field of view when the boat is moving (zoomed out)

    public float cameraAngle = 45f;    // Dynamic camera viewing angle (serialized field)

    private Camera cam;                // Camera component
    private Vector3 offset;            // Offset for the camera position
    private Vector3 previousPosition;  // To store the boat's position from the previous frame

    void Start()
    {
        cam = Camera.main;              // Get the main camera
        offset = new Vector3(0, height, -followDistance);  // Initialize the offset
        cam.fieldOfView = zoomInFOV;    // Start with the zoomed-in FOV
        previousPosition = boat.position; // Initialize previous position
    }

    public void MovetoBoat()
    {
        followSpeed = 2;
        Invoke("addSpeed", 2.5f);
    }

    void addSpeed()
    {
        followSpeed = 1000;
    }



    void Update()
    {
        // Calculate the target position based on the boat's position and the fixed offset
        Vector3 targetPosition = boat.position + offset;

        // Smoothly move the camera towards the target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

        // The camera will always look at the boat, and we will apply the dynamic camera angle
        transform.rotation = Quaternion.Euler(cameraAngle, 0f, 0f);

        // Get the boat's movement speed based on position change
        float boatSpeed = CalculateBoatSpeed();

        // Adjust zoom based on boat's speed (zoom out when moving, zoom in when stopped)
        AdjustZoom(boatSpeed);

        // Update previous position for the next frame
        previousPosition = boat.position;
    }

    // Calculate speed based on position change over time
    float CalculateBoatSpeed()
    {
        // The speed is the distance the boat moved from the previous frame
        float distanceMoved = Vector3.Distance(previousPosition, boat.position);
        return distanceMoved / Time.deltaTime; // Speed is distance divided by deltaTime
    }

    // Adjust the camera's FOV based on the boat's speed
    void AdjustZoom(float speed)
    {
        // Determine the target FOV based on boat's speed
        float targetFOV = (speed > 0.5f) ? zoomOutFOV : zoomInFOV;

        // Smoothly interpolate the FOV to the target value
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, zoomSpeed * Time.deltaTime);
    }
}
