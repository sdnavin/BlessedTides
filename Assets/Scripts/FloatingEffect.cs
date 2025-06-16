using UnityEngine;

public class FloatingEffect : MonoBehaviour
{
    // Adjustable parameters
    public float amplitude = 0.5f; // Height of the floating movement
    public float frequency = 1f;  // Speed of the floating movement

    // Initial position
    private Vector3 startPosition;

    void Start()
    {
        // Store the object's starting position
        startPosition = transform.localPosition;
    }

    void Update()
    {
        // Calculate the new position
        float newY = startPosition.y + Mathf.Sin(Time.time * frequency) * amplitude;
        transform.localPosition = new Vector3(startPosition.x, newY, startPosition.z);
    }
}
