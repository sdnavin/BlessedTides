using UnityEngine;

public class FishFlock : MonoBehaviour
{
    public float speed = 2f;                // Speed of the fish
    public float rotationSpeed = 5f;        // Speed of the fish's rotation
    public float radius = 5f;               // Radius within which the fish will move
    public float neighborDistance = 1f;     // Minimum distance from others (not used in this simplified version)
    public float avoidanceRadius = 3f;      // Radius to detect and avoid objects tagged as "Boat"

    private Vector3 targetPosition;         // Target position that the fish is moving towards

    void Start()
    {
        // Set initial random target position within the flock area
        targetPosition = GetRandomPositionWithinRadius();
    }

    void Update()
    {
        // Check for nearby objects tagged as "Boat"
        AvoidBoats();

        // Move the fish towards its target
        MoveFish();

        // If the fish is near its target, assign a new random target within the radius
        if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
        {
            targetPosition = GetRandomPositionWithinRadius();
        }

        // Ensure the fish remains at Y = 0
        transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
    }

    private void AvoidBoats()
    {
        // Find all objects tagged as "Boat" within the avoidance radius
        Collider[] boats = Physics.OverlapSphere(transform.position, avoidanceRadius, LayerMask.GetMask("Default"));

        foreach (var boat in boats)
        {
            if (boat.CompareTag("Boat"))
            {
                // Calculate a direction away from the boat
                Vector3 awayFromBoat = (transform.position - boat.transform.position).normalized;

                // Adjust the target position to move away from the boat
                targetPosition += awayFromBoat * avoidanceRadius;
                targetPosition = ClampToRadius(targetPosition);
                break; // Avoid the first boat detected
            }
        }
    }

    private void MoveFish()
    {
        // Move the fish towards the target position
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // Rotate the fish smoothly towards the target position
        Vector3 targetDirection = targetPosition - transform.position;
        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private Vector3 GetRandomPositionWithinRadius()
    {
        // Get a random position within a sphere of the given radius
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += transform.position; // Set the center to the fish's current position
        randomDirection.y = 0; // Keep the fish on a flat plane (Y=0)
        return randomDirection;
    }

    private Vector3 ClampToRadius(Vector3 position)
    {
        // Ensure the position stays within the defined radius
        Vector3 center = transform.position;
        Vector3 offset = position - center;
        if (offset.magnitude > radius)
        {
            offset = offset.normalized * radius;
        }
        return center + offset;
    }
}
