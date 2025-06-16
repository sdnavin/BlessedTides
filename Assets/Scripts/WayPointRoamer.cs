using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointRoamer : MonoBehaviour
{
    [Header("Waypoint Settings")]
    public List<Transform> waypoints = new List<Transform>();  // List of waypoints to visit
    public float moveSpeed = 5f;                              // Speed of character movement
    public float stayDuration = 3f;                           // Time to stay at each waypoint
    public bool randomizeWaypoints = false;                   // Option to visit waypoints in random order

    [Header("References")]
    public Animator animator;                                 // Reference to the Animator component

    // Animation parameter names
    private const string SPEED_PARAM = "speed";
    private const string IDLE_PARAM = "isIdle";

    // Private variables
    private int currentWaypointIndex = 0;                     // Index of the current waypoint
    private Vector3 currentDestination;                       // Current destination position
    private bool isMoving = false;                            // Flag to indicate if character is moving
    private bool isWaiting = false;                           // Flag to indicate if character is waiting at a waypoint
    private List<int> randomWaypointIndices = new List<int>(); // List for randomized waypoint order

    private void Start()
    {
        if (waypoints.Count == 0)
        {
            Debug.LogWarning("No waypoints assigned to WaypointRoamer!");
            return;
        }

        // Initialize randomized waypoint indices if needed
        if (randomizeWaypoints)
        {
            InitializeRandomWaypoints();
        }

        // Start the roaming routine
        StartRoaming();
    }

    private void Update()
    {
        if (isMoving)
        {
            MoveTowardsDestination();
        }
        else if (!isWaiting && waypoints.Count > 0)
        {
            // If not moving or waiting, start moving to the next waypoint
            MoveToNextWaypoint();
        }
    }

    // Initialize the list of randomized waypoint indices
    private void InitializeRandomWaypoints()
    {
        randomWaypointIndices.Clear();
        for (int i = 0; i < waypoints.Count; i++)
        {
            randomWaypointIndices.Add(i);
        }
        ShuffleList(randomWaypointIndices);
    }

    // Shuffle a list using Fisher-Yates algorithm
    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        for (int i = 0; i < n; i++)
        {
            int r = i + Random.Range(0, n - i);
            T temp = list[r];
            list[r] = list[i];
            list[i] = temp;
        }
    }

    // Start the roaming behavior
    public void StartRoaming()
    {
        if (waypoints.Count == 0) return;

        // Set the first waypoint as the destination
        SetNextWaypoint();
    }

    // Move towards the current destination
    private void MoveTowardsDestination()
    {
        // Calculate direction and normalize it
        Vector3 direction = (currentDestination - transform.position).normalized;

        // Move the character
        transform.position += direction * moveSpeed * Time.deltaTime;

        // Rotate towards the movement direction
        if (direction.magnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
        }

        // Update animator for moving state
        UpdateAnimator(moveSpeed, false);

        // Check if reached destination
        if (Vector3.Distance(transform.position, currentDestination) < 0.1f)
        {
            // Stop moving and start waiting
            isMoving = false;
            StartCoroutine(WaitAtWaypoint());
        }
    }

    // Wait at the current waypoint for the specified duration
    private IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;

        // Update animator for idle state
        UpdateAnimator(0f, true);

        //Debug.Log($"Waiting at waypoint {currentWaypointIndex} for {stayDuration} seconds");

        // Wait for the specified duration
        yield return new WaitForSeconds(stayDuration);

        isWaiting = false;

        // Move to the next waypoint
        MoveToNextWaypoint();
    }

    // Set the next waypoint as the destination
    private void MoveToNextWaypoint()
    {
        SetNextWaypoint();
        isMoving = true;
    }

    // Determine and set the next waypoint
    private void SetNextWaypoint()
    {
        if (randomizeWaypoints)
        {
            // Get next waypoint from randomized list
            currentWaypointIndex = randomWaypointIndices[0];

            // Move the used index to the end of the list
            randomWaypointIndices.RemoveAt(0);
            randomWaypointIndices.Add(currentWaypointIndex);

            // If we used all waypoints, re-shuffle the list
            if (randomWaypointIndices.Count == 1)
            {
                ShuffleList(randomWaypointIndices);
            }
        }
        else
        {
            // Sequential waypoint selection
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
        }

        // Set the destination to the selected waypoint
        if (waypoints[currentWaypointIndex] != null)
        {
            currentDestination = waypoints[currentWaypointIndex].position;
            //Debug.Log($"Moving to waypoint {currentWaypointIndex}");
        }
        else
        {
            Debug.LogError($"Waypoint at index {currentWaypointIndex} is null!");
        }
    }

    // Update the animator with the given speed and idle state
    private void UpdateAnimator(float speed, bool isIdle)
    {
        if (animator != null)
        {
            animator.SetFloat(SPEED_PARAM, speed);
            animator.SetBool(IDLE_PARAM, isIdle);
        }
    }

    // Add a new waypoint at runtime
    public void AddWaypoint(Transform newWaypoint)
    {
        if (newWaypoint != null)
        {
            waypoints.Add(newWaypoint);

            // If randomizing waypoints, add the new index to the random list
            if (randomizeWaypoints)
            {
                randomWaypointIndices.Add(waypoints.Count - 1);
                ShuffleList(randomWaypointIndices);
            }

            Debug.Log($"Added new waypoint. Total waypoints: {waypoints.Count}");
        }
    }

    // Remove a waypoint at runtime
    public void RemoveWaypoint(Transform waypointToRemove)
    {
        int index = waypoints.IndexOf(waypointToRemove);
        if (index != -1)
        {
            waypoints.RemoveAt(index);

            // Update the random indices list if needed
            if (randomizeWaypoints)
            {
                randomWaypointIndices.Clear();
                for (int i = 0; i < waypoints.Count; i++)
                {
                    randomWaypointIndices.Add(i);
                }
                ShuffleList(randomWaypointIndices);
            }

            // If the current waypoint was removed, pick a new one
            if (index == currentWaypointIndex)
            {
                currentWaypointIndex = currentWaypointIndex % waypoints.Count;
                if (isMoving)
                {
                    SetNextWaypoint();
                }
            }
            else if (index < currentWaypointIndex)
            {
                // Adjust the current index if a waypoint before it was removed
                currentWaypointIndex--;
            }

            Debug.Log($"Removed waypoint. Total waypoints: {waypoints.Count}");
        }
    }

    // Helper method to create a waypoint at a given position
    public static Transform CreateWaypoint(Vector3 position, Transform parent = null)
    {
        GameObject waypoint = new GameObject("Waypoint");
        waypoint.transform.position = position;

        if (parent != null)
        {
            waypoint.transform.SetParent(parent);
        }

        return waypoint.transform;
    }

    // Optional: Draw gizmos in the editor to visualize waypoints
    private void OnDrawGizmos()
    {
        if (waypoints.Count == 0) return;

        // Draw spheres at waypoint positions
        Gizmos.color = Color.blue;
        foreach (Transform waypoint in waypoints)
        {
            if (waypoint != null)
            {
                Gizmos.DrawSphere(waypoint.position, 0.3f);
            }
        }

        // Draw lines between waypoints
        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] != null)
            {
                Vector3 currentPos = waypoints[i].position;
                Vector3 nextPos = waypoints[(i + 1) % waypoints.Count].position;
                Gizmos.DrawLine(currentPos, nextPos);
            }
        }
    }
}