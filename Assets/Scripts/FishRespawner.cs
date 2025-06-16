using UnityEngine;

public class FishRespawner : MonoBehaviour
{
    private FishSpawner fishSpawner;

    private void Start()
    {
        // Find the spawner in the parent hierarchy
        fishSpawner = GetComponentInParent<FishSpawner>();
    }

    private void OnDisable()
    {
        if (fishSpawner != null)
        {
            // Notify the spawner to respawn this fish
            fishSpawner.RespawnFish(gameObject);
        }
    }
}
