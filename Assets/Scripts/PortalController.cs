using UnityEngine;
using System.Collections;

public class PortalController : MonoBehaviour
{
    [Header("World Settings")]
    [SerializeField] private GameObject world1;
    [SerializeField] private GameObject world2;
    [SerializeField] private float transitionDuration = 1.5f;
    [SerializeField] private Color portalColor = Color.blue;

    private Material transitionMaterial;
    private static readonly int RadiusProperty = Shader.PropertyToID("_Radius");
    private static readonly int CenterProperty = Shader.PropertyToID("_Center");
    private static readonly int ColorProperty = Shader.PropertyToID("_PortalColor");
    private bool isTransitioning = false;
    private bool isInWorld1 = true;

    private void Start()
    {
        // Create the transition material
        transitionMaterial = new Material(Shader.Find("Hidden/PortalReveal"));
        transitionMaterial.SetColor(ColorProperty, portalColor);

        // Initialize world states
        world1.SetActive(true);
        world2.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTransitioning)
        {
            StartCoroutine(SwapWorlds());
        }
    }

    private IEnumerator SwapWorlds()
    {
        isTransitioning = true;

        // Get the screen position of the portal for the effect center
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        screenPos.x /= Screen.width;
        screenPos.y /= Screen.height;
        transitionMaterial.SetVector(CenterProperty, screenPos);

        // First half of the transition - fade to portal color
        float elapsed = 0f;
        float halfDuration = transitionDuration / 2f;

        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / halfDuration;
            float radius = Mathf.Lerp(0f, 2f, normalizedTime);
            transitionMaterial.SetFloat(RadiusProperty, radius);

            OnRenderImage(null, null);
            yield return null;
        }

        // Swap the worlds at the peak of the transition
        if (isInWorld1)
        {
            world1.SetActive(false);
            world2.SetActive(true);
        }
        else
        {
            world1.SetActive(true);
            world2.SetActive(false);
        }
        isInWorld1 = !isInWorld1;

        // Second half of the transition - fade back from portal color
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = 1f - ((elapsed - halfDuration) / halfDuration);
            float radius = Mathf.Lerp(0f, 2f, normalizedTime);
            transitionMaterial.SetFloat(RadiusProperty, radius);

            OnRenderImage(null, null);
            yield return null;
        }

        isTransitioning = false;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, transitionMaterial);
    }
}

[System.Serializable]
public class WorldTransitionTrigger : MonoBehaviour
{
    [SerializeField] private float triggerRadius = 2f;
    [SerializeField] private ParticleSystem portalEffect;

    private void OnDrawGizmos()
    {
        // Visual indicator in the editor for the trigger area
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
}