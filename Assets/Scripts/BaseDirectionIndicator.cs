using UnityEngine;
using UnityEngine.UI;

public class BaseDirectionIndicator : MonoBehaviour
{
    public Transform boat; // Reference to the boat
    public Transform arrow; // Reference to the boat
    public Transform baseLocation; // Reference to the base location
    public float arrowDistance = 2f; // Distance of the arrow from the boat

    void Update()
    {
        // Set the position of the arrow at a fixed distance in front of the boat
        Vector3 arrowPosition = boat.position + boat.forward * arrowDistance;
        arrow.transform.position = arrowPosition;

        // Make the arrow point towards the base
        Vector3 directionToBase = (baseLocation.position - arrow.transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToBase);
        arrow.transform.rotation = targetRotation;
    }
}

