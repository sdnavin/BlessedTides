using UnityEngine;

public class QuadWarpController : MonoBehaviour
{
    public Material quadWarpMaterial;

    [Header("Corner Positions (x, y)")]
    public Vector3 bottomLeft = new Vector3(-0.5f, -0.5f, 0);
    public Vector3 bottomRight = new Vector3(0.5f, -0.5f, 0);
    public Vector3 topRight = new Vector3(0.5f, 0.5f, 0);
    public Vector3 topLeft = new Vector3(-0.5f, 0.5f, 0);


    /// <summary>
    /// Updates the corner values from outside scripts.
    /// </summary>
    public void UpdateCorners(Vector3 bl, Vector3 br, Vector3 tr, Vector3 tl)
    {
        bottomLeft = bl;
        bottomRight = br;
        topRight = tr;
        topLeft = tl;

        ApplyToMaterial();
    }

    /// <summary>
    /// Applies the current fields to the material.
    /// </summary>
    private void ApplyToMaterial()
    {
        if (quadWarpMaterial == null) return;

        quadWarpMaterial.SetVector("_P00", new Vector4(bottomLeft.x, bottomLeft.y, 0f, 1f));
        quadWarpMaterial.SetVector("_P10", new Vector4(bottomRight.x, bottomRight.y, 0f, 1f));
        quadWarpMaterial.SetVector("_P11", new Vector4(topRight.x, topRight.y, 0f, 1f));
        quadWarpMaterial.SetVector("_P01", new Vector4(topLeft.x, topLeft.y, 0f, 1f));
    }
}
