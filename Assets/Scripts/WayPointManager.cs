using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointManager : MonoBehaviour
{
    [Header("Waypoint Settings")]
    public GameObject waypointPrefab;                  // Optional prefab for waypoints
    public Color waypointColor = Color.blue;           // Color for waypoint gizmos
    public float waypointSize = 0.5f;                  // Size of waypoint gizmos

    [Header("Dynamic Waypoint Generation")]
    public bool generateWaypointsAtRuntime = false;    // Whether to generate waypoints at runtime
    public int waypointCount = 5;                      // Number of waypoints to generate
    public float generationRadius = 10f;               // Radius within which to generate waypoints
    public Vector3 generationCenter = Vector3.zero;    // Center point for waypoint generation

    [Header("References")]
    public WaypointRoamer targetRoamer;                // Reference to the WaypointRoamer to update

    // List of all waypoints
    [SerializeField] private List<Transform> waypoints = new List<Transform>();

    private void Awake()
    {
        // If no waypoints are assigned yet, collect children as waypoints
        if (waypoints.Count == 0)
        {
            CollectChildWaypoints();
        }
    }

    private void Start()
    {
        // Generate waypoints at runtime if enabled
        if (generateWaypointsAtRuntime)
        {
            GenerateRandomWaypoints();
        }

        // Apply waypoints to the roamer if available
        if (targetRoamer != null)
        {
            ApplyWaypointsToRoamer();
        }
    }

    // Collect all child transforms as waypoints
    private void CollectChildWaypoints()
    {
        waypoints.Clear();

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.gameObject.activeSelf)
            {
                waypoints.Add(child);
            }
        }

        Debug.Log($"Collected {waypoints.Count} waypoints from children");
    }

    // Generate random waypoints within the specified radius
    private void GenerateRandomWaypoints()
    {
        for (int i = 0; i < waypointCount; i++)
        {
            // Generate random position within a circle
            Vector2 randomCircle = Random.insideUnitCircle * generationRadius;
            Vector3 randomPosition = new Vector3(randomCircle.x, 0, randomCircle.y) + generationCenter;

            // Create waypoint
            Transform newWaypoint;

            if (waypointPrefab != null)
            {
                GameObject waypointObj = Instantiate(waypointPrefab, randomPosition, Quaternion.identity, transform);
                waypointObj.name = $"Waypoint_{i}";
                newWaypoint = waypointObj.transform;
            }
            else
            {
                GameObject waypointObj = new GameObject($"Waypoint_{i}");
                waypointObj.transform.position = randomPosition;
                waypointObj.transform.SetParent(transform);
                newWaypoint = waypointObj.transform;
            }

            waypoints.Add(newWaypoint);
        }

        Debug.Log($"Generated {waypointCount} random waypoints");
    }

    // Apply the current waypoints to the target roamer
    public void ApplyWaypointsToRoamer()
    {
        if (targetRoamer != null)
        {
            targetRoamer.waypoints.Clear();
            targetRoamer.waypoints.AddRange(waypoints);
            Debug.Log($"Applied {waypoints.Count} waypoints to roamer");
        }
        else
        {
            Debug.LogWarning("No target roamer assigned!");
        }
    }

    // Add a new waypoint at the specified position
    public Transform AddWaypoint(Vector3 position)
    {
        Transform newWaypoint;

        if (waypointPrefab != null)
        {
            GameObject waypointObj = Instantiate(waypointPrefab, position, Quaternion.identity, transform);
            waypointObj.name = $"Waypoint_{waypoints.Count}";
            newWaypoint = waypointObj.transform;
        }
        else
        {
            GameObject waypointObj = new GameObject($"Waypoint_{waypoints.Count}");
            waypointObj.transform.position = position;
            waypointObj.transform.SetParent(transform);
            newWaypoint = waypointObj.transform;
        }

        waypoints.Add(newWaypoint);

        // Update the roamer if available
        if (targetRoamer != null)
        {
            targetRoamer.AddWaypoint(newWaypoint);
        }

        return newWaypoint;
    }

    // Remove a waypoint by index
    public void RemoveWaypoint(int index)
    {
        if (index >= 0 && index < waypoints.Count)
        {
            Transform waypointToRemove = waypoints[index];

            // Update the roamer if available
            if (targetRoamer != null)
            {
                targetRoamer.RemoveWaypoint(waypointToRemove);
            }

            waypoints.RemoveAt(index);
            Destroy(waypointToRemove.gameObject);
        }
    }

    // Remove a waypoint by reference
    public void RemoveWaypoint(Transform waypoint)
    {
        int index = waypoints.IndexOf(waypoint);
        if (index != -1)
        {
            RemoveWaypoint(index);
        }
    }

    // Optional: Draw gizmos in the editor to visualize waypoints
    private void OnDrawGizmos()
    {
        // Draw spheres at waypoint positions
        Gizmos.color = waypointColor;

        foreach (Transform waypoint in waypoints)
        {
            if (waypoint != null)
            {
                Gizmos.DrawSphere(waypoint.position, waypointSize);
            }
        }

        // Draw lines between waypoints
        if (waypoints.Count > 1)
        {
            Gizmos.color = new Color(waypointColor.r, waypointColor.g, waypointColor.b, 0.5f);

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

        // Visualize generation area if runtime generation is enabled
        if (generateWaypointsAtRuntime)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
            Gizmos.DrawSphere(generationCenter, generationRadius);
        }
    }

    // Add waypoints at runtime through a public method
    public void AddRandomWaypoints(int count)
    {
        for (int i = 0; i < count; i++)
        {
            // Generate random position within a circle
            Vector2 randomCircle = Random.insideUnitCircle * generationRadius;
            Vector3 randomPosition = new Vector3(randomCircle.x, 0, randomCircle.y) + generationCenter;

            // Add the waypoint
            AddWaypoint(randomPosition);
        }
    }
}