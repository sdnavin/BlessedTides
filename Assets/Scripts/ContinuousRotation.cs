using UnityEngine;

public class ContinuousRotation : MonoBehaviour
{
    // Rotation speed (degrees per second)
    public float rotationSpeed = 50f;

    // Rotation axis
    public Vector3 rotationAxis = Vector3.up; // Default is Vector3.up (y-axis)

    void Update()
    {
        // Apply rotation
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
    }
}
