using System;
using UnityEngine;

public class WorldFollowUIOffset : MonoBehaviour
{
    public RectTransform uiElement;  // Reference to the UI element
    Transform worldObject;    // World-space object to move
    public Camera uiCamera;          // Camera rendering the UI (only needed if not Screen Space Overlay)

    private Vector3 initialOffset;

    private void Awake()
    {
        worldObject = transform;
    }

    private void OnEnable()
    {
        GameSettingSetupController.OnGameSettingsSetupCompleted += Align;
    }

    private void OnDisable()
    {
        GameSettingSetupController.OnGameSettingsSetupCompleted -= Align;
    }

    private void Align()
    {
        if (uiElement == null || worldObject == null)
        {
            Debug.LogError("Assign references in WorldFollowUIOffset");
            return;
        }

        // Convert RectTransform position to world space
        Vector3 uiWorldPos = uiElement.position;

        // Calculate initial offset between world and UI object
        initialOffset = worldObject.position - uiWorldPos;
    }

    void Start()
    {
        if (uiElement == null || worldObject == null)
        {
            Debug.LogError("Assign references in WorldFollowUIOffset");
            return;
        }

        // Convert RectTransform position to world space
        Vector3 uiWorldPos = uiElement.position;

        // Calculate initial offset between world and UI object
        initialOffset = worldObject.position - uiWorldPos;
    }

    void LateUpdate()
    {
        if (uiElement == null || worldObject == null)
            return;

        Vector3 pos = uiElement.position + initialOffset; 
        // Update world object to follow UI element while maintaining offset
        worldObject.position = new Vector3(pos.x, worldObject.position.y, pos.z);
    }
}
