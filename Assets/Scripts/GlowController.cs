using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Renderer))]
public class GlowController : MonoBehaviour
{
    public Material glowMaterial;                   // Reference to the glow material
    [ColorUsage(true, true)]
    public Color glowColor = Color.cyan;            // Color of the glow
    [Range(0, 10)]
    public float glowIntensity = 2.0f;              // Intensity of the glow
    [Range(0.1f, 5.0f)]
    public float glowSpeed = 1.0f;                  // Speed of the glow pulse
    [Range(0, 1)]
    public float glowMinimum = 0.0f;                // Minimum glow value
    [Range(0, 1)]
    public float glowMaximum = 1.0f;                // Maximum glow value

    public enum BlendMode { Additive = 0, Overlay = 1, Screen = 2 }
    public BlendMode blendMode = BlendMode.Overlay;  // Blend mode for the glow

    [Header("Duration Settings")]
    public float glowDuration = 2.0f;               // How long the glow lasts (0 = infinite)
    public float fadeDuration = 0.5f;               // How long it takes to fade in/out
    public bool looping = false;                    // Whether the glow effect should loop
    public float loopInterval = 5.0f;               // Time between loops

    public enum TriggerType
    {
        Manual,                  // Triggered via script only
        OnStart,                 // Triggered when the object starts
        OnEnable,                // Triggered when the object is enabled
        OnTriggerEnter,          // Triggered when another object enters the trigger
        OnCollision,             // Triggered on collision
        TimedInterval,           // Triggered at regular intervals
        ProximityToTarget        // Triggered when close to a target
    }

    public TriggerType triggerType = TriggerType.Manual;

    [Header("Trigger Parameters")]
    [Tooltip("Tags that can trigger this glow (leave empty for any)")]
    public string[] triggerTags;                    // Tags that can trigger the glow
    [Tooltip("Time between automatic triggers")]
    public float triggerInterval = 5.0f;            // Time between automatic triggers
    [Tooltip("Target to check proximity against")]
    public Transform proximityTarget;               // Target for proximity check
    [Tooltip("Distance at which glow activates")]
    public float proximityDistance = 5.0f;          // Distance for proximity trigger
    [Tooltip("Check proximity every X seconds")]
    public float proximityCheckInterval = 0.2f;     // How often to check proximity

    [Header("Events")]
    public UnityEvent OnGlowStart;                  // Event fired when glow starts
    public UnityEvent OnGlowEnd;                    // Event fired when glow ends

    // Private variables
    private Renderer objectRenderer;
    private Material instancedGlowMaterial;
    private Material[] originalMaterials;
    private Material[] materialsWithGlow;
    private Coroutine activeGlowRoutine;
    private Coroutine checkProximityRoutine;
    private bool isGlowing = false;

    private void Awake()
    {
        // Get the renderer
        objectRenderer = GetComponent<Renderer>();

        // Store original materials
        originalMaterials = objectRenderer.materials;

        // Setup materials array with glow
        materialsWithGlow = new Material[originalMaterials.Length + 1];
        System.Array.Copy(originalMaterials, materialsWithGlow, originalMaterials.Length);

        // Check if we have a glow material assigned
        if (glowMaterial == null)
        {
            Shader glowShader = Shader.Find("Custom/SimpleGlowShader");
            if (glowShader != null)
            {
                glowMaterial = new Material(glowShader);
            }
            else
            {
                Debug.LogError("GlowController: SimpleGlowShader not found. Make sure the shader is in your project.");
                enabled = false;
                return;
            }
        }

        // Create a new instance of the material to avoid modifying the original
        instancedGlowMaterial = new Material(glowMaterial);
        materialsWithGlow[originalMaterials.Length] = instancedGlowMaterial;

        // Apply initial glow settings
        UpdateGlowSettings();
    }

    private void OnEnable()
    {
        if (triggerType == TriggerType.OnEnable)
        {
            StartGlowEffect();
        }

        // Start proximity checking if needed
        if (triggerType == TriggerType.ProximityToTarget && proximityTarget != null)
        {
            checkProximityRoutine = StartCoroutine(CheckProximityRoutine());
        }

        // Start timed interval if needed
        if (triggerType == TriggerType.TimedInterval)
        {
            StartCoroutine(TimedTriggerRoutine());
        }
    }

    private void Start()
    {
        if (triggerType == TriggerType.OnStart)
        {
            StartGlowEffect();
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        RemoveGlowMaterial();
    }

    private void OnDestroy()
    {
        // Clean up the instanced material
        if (instancedGlowMaterial != null)
        {
            Destroy(instancedGlowMaterial);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggerType == TriggerType.OnTriggerEnter)
        {
            if (ShouldTrigger(other.tag))
            {
                StartGlowEffect();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (triggerType == TriggerType.OnCollision)
        {
            if (ShouldTrigger(collision.gameObject.tag))
            {
                StartGlowEffect();
            }
        }
    }

    // Check if the tag should trigger the glow
    private bool ShouldTrigger(string tag)
    {
        // If no tags specified, trigger on any
        if (triggerTags.Length == 0)
            return true;

        // Check if the tag is in our list
        foreach (string triggerTag in triggerTags)
        {
            if (tag == triggerTag)
                return true;
        }

        return false;
    }

    // Update the glow material with current settings
    private void UpdateGlowSettings()
    {
        if (instancedGlowMaterial != null)
        {
            instancedGlowMaterial.SetColor("_GlowColor", glowColor);
            instancedGlowMaterial.SetFloat("_GlowIntensity", glowIntensity);
            instancedGlowMaterial.SetFloat("_GlowSpeed", glowSpeed);
            instancedGlowMaterial.SetFloat("_GlowMinimum", glowMinimum);
            instancedGlowMaterial.SetFloat("_GlowMaximum", glowMaximum);
            instancedGlowMaterial.SetFloat("_BlendMode", (float)blendMode);
        }
    }

    // Add the glow material to the renderer
    private void ApplyGlowMaterial()
    {
        objectRenderer.materials = materialsWithGlow;
    }

    // Remove the glow material from the renderer
    private void RemoveGlowMaterial()
    {
        if (objectRenderer != null)
        {
            objectRenderer.materials = originalMaterials;
        }
        isGlowing = false;
    }

    // Public method to start the glow effect
    public void StartGlowEffect()
    {
        // Stop any active glow routine
        if (activeGlowRoutine != null)
        {
            StopCoroutine(activeGlowRoutine);
        }

        // Start a new glow routine
        activeGlowRoutine = StartCoroutine(GlowRoutine());
    }

    // Public method to stop the glow effect
    public void StopGlowEffect(bool fadeOut = true)
    {
        if (activeGlowRoutine != null)
        {
            StopCoroutine(activeGlowRoutine);
            activeGlowRoutine = null;
        }

        if (fadeOut)
        {
            StartCoroutine(FadeOutRoutine());
        }
        else
        {
            RemoveGlowMaterial();
            OnGlowEnd.Invoke();
        }
    }

    // Coroutine to handle the glow effect
    private IEnumerator GlowRoutine()
    {
        // Update the glow settings
        UpdateGlowSettings();

        // Fade in
        yield return StartCoroutine(FadeInRoutine());

        // If duration is set, wait for that duration
        if (glowDuration > 0)
        {
            yield return new WaitForSeconds(glowDuration);

            // Fade out
            yield return StartCoroutine(FadeOutRoutine());

            // If looping, wait for interval and start again
            if (looping)
            {
                yield return new WaitForSeconds(loopInterval);
                activeGlowRoutine = StartCoroutine(GlowRoutine());
            }
        }
    }

    // Coroutine to fade in the glow
    private IEnumerator FadeInRoutine()
    {
        // Apply the material first
        ApplyGlowMaterial();

        // Set initial alpha to 0
        Color startColor = glowColor;
        startColor.a = 0;
        instancedGlowMaterial.SetColor("_GlowColor", startColor);

        // Invoke start event
        OnGlowStart.Invoke();
        isGlowing = true;

        // Fade in over time
        float elapsed = 0;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            Color lerpedColor = glowColor;
            lerpedColor.a = Mathf.Lerp(0, glowColor.a, t);
            instancedGlowMaterial.SetColor("_GlowColor", lerpedColor);

            yield return null;
        }

        // Ensure full color at the end
        instancedGlowMaterial.SetColor("_GlowColor", glowColor);
    }

    // Coroutine to fade out the glow
    private IEnumerator FadeOutRoutine()
    {
        // Get starting color
        Color startColor = instancedGlowMaterial.GetColor("_GlowColor");

        // Fade out over time
        float elapsed = 0;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            Color lerpedColor = startColor;
            lerpedColor.a = Mathf.Lerp(startColor.a, 0, t);
            instancedGlowMaterial.SetColor("_GlowColor", lerpedColor);

            yield return null;
        }

        // Remove the material
        RemoveGlowMaterial();

        // Invoke end event
        OnGlowEnd.Invoke();
    }

    // Coroutine to check proximity to target
    private IEnumerator CheckProximityRoutine()
    {
        while (true)
        {
            if (proximityTarget != null)
            {
                float distance = Vector3.Distance(transform.position, proximityTarget.position);

                if (distance <= proximityDistance)
                {
                    if (!isGlowing)
                    {
                        StartGlowEffect();
                    }
                }
                else
                {
                    if (isGlowing)
                    {
                        StopGlowEffect(true);
                    }
                }
            }

            yield return new WaitForSeconds(proximityCheckInterval);
        }
    }

    // Coroutine for timed interval triggering
    private IEnumerator TimedTriggerRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(triggerInterval);
            StartGlowEffect();
        }
    }

    // Public methods for external control

    // Set the glow color
    public void SetGlowColor(Color color)
    {
        glowColor = color;
        UpdateGlowSettings();
    }

    // Set the glow intensity
    public void SetGlowIntensity(float intensity)
    {
        glowIntensity = Mathf.Clamp(intensity, 0, 10);
        UpdateGlowSettings();
    }

    // Toggle the glow effect on/off
    public void ToggleGlow()
    {
        if (isGlowing)
        {
            StopGlowEffect();
        }
        else
        {
            StartGlowEffect();
        }
    }

    // Public method to set all glow parameters at once
    public void ConfigureGlow(Color color, float intensity, float speed, float min, float max)
    {
        glowColor = color;
        glowIntensity = Mathf.Clamp(intensity, 0, 10);
        glowSpeed = Mathf.Clamp(speed, 0.1f, 5.0f);
        glowMinimum = Mathf.Clamp01(min);
        glowMaximum = Mathf.Clamp01(max);
        UpdateGlowSettings();
    }
}