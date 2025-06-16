using UnityEngine;

public class CameraUI : MonoBehaviour
{
    public Camera targetCamera; // Assign the camera in the Inspector

    void Start()
    {
        // Default to the main camera if no camera is assigned
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    void Update()
    {
        if (targetCamera != null)
        {
            //// Get the direction from the UI to the camera
            //Vector3 direction = targetCamera.transform.position - transform.position;

            //// Constrain the rotation: 
            //// Yaw (rotation around Y-axis) and fixed Pitch (X-axis always 90 degrees)
            //Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z), Vector3.up);

            // Adjust the rotation to ensure the X-axis stays at 90 degrees
            transform.rotation = Quaternion.Euler(90, 0, 0);
        }
    }
}
