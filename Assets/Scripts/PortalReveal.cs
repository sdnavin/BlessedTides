using UnityEngine;
using UnityEngine.Events;

public class PortalReveal : MonoBehaviour
{
    public UnityEvent onRevealOn;
    public UnityEvent onRevealOff;
    public Material portalMaterial; // Assign your material with the shader here
    public string revealRadiusProperty = "_RevealRadius"; // Name of the shader property
    public string circleCenterProperty = "_CircleCenter"; // Name of the shader property
    public float revealSpeed = 50f; // Speed at which the reveal radius increases

    private bool isRevealing = false;
    private bool isClosing = false;
    private float currentRadius = 0f;
    private float maxRadius = 1000f;

    private void Start()
    {
        // Initialize the shader property
        if (portalMaterial != null)
        {
            portalMaterial.SetFloat(revealRadiusProperty, currentRadius);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Boat")) // Check if the object entering is the boat
        {
            isRevealing = true;
        }
    }

    public void OnReveal()
    {
        isRevealing = true;
        Vector3 position = transform.position;
        // Pass the position to the shader as a Vector4
        portalMaterial.SetVector(circleCenterProperty, new Vector4(position.x, position.y, position.z, 0));
        Invoke("RevealIt", 1);
    }

    void RevealIt()
    {
        onRevealOn.Invoke();
    }
    void CloseIt()
    {
        isClosing = true;
    }
    private void Update()
    {
        if (isRevealing && portalMaterial != null)
        {
            // Gradually increase the radius
            currentRadius += revealSpeed * Time.deltaTime;
            currentRadius = Mathf.Clamp(currentRadius, 0f, maxRadius);
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.zero, Time.deltaTime * 2);
            // Update the material's shader property
            portalMaterial.SetFloat(revealRadiusProperty, currentRadius);

            // Stop revealing if max radius is reached
            if (currentRadius >= maxRadius)
            {
                isRevealing = false;
                Invoke("CloseIt", 10);
            }
        }
        if (isClosing)
        {
            // Gradually increase the radius
            currentRadius -= revealSpeed * Time.deltaTime;
            currentRadius = Mathf.Clamp(currentRadius, 0f, maxRadius);
            // Update the material's shader property
            portalMaterial.SetFloat(revealRadiusProperty, currentRadius);
            if (currentRadius <=0)
            {
                isClosing = false;
                onRevealOff.Invoke();
            }
        }
    }
}
