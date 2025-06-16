using UnityEngine;

public class CircleProgress : MonoBehaviour
{
    public LineRenderer lineRenderer;
    [Range(0, 1)] public float progress = 0f; // Progress from 0 to 1
    public int segments = 100;               // Number of segments for the circle
    public float radius = 1f;                // Radius of the circle

    void Start()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.useWorldSpace = false; // Use local space
    }

    void Update()
    {
        DrawProgressCircle(progress);
    }

    public void DrawProgressCircle(float progress)
    {
        int points = Mathf.CeilToInt(segments * progress); // Calculate points based on progress
        lineRenderer.positionCount = points + 1;

        for (int i = 0; i <= points; i++)
        {
            float angle = (2f * Mathf.PI * i / segments); // Calculate angle for this segment
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;

            lineRenderer.SetPosition(i, new Vector3(x, y, 0)); // Set position in local space
        }
    }
}
