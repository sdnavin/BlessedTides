using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;

public class FishSpawner : MonoBehaviour
{
    public GameObject fishPrefab;    // Reference to the fish prefab
    public int numberOfFish = 10;    // Number of fish to spawn
    public float spawnRadius = 15f;  // Radius to spawn fish within
    public UnityEvent onEnable;
    public float respawnTime = 5f;   // Time to re-enable a deactivated fish

    private List<GameObject> fishList = new List<GameObject>(); // Track spawned fish

    private void OnEnable()
    {
        onEnable.Invoke();
    }

    void Start()
    {
        // Spawn a flock of fish
        for (int i = 0; i < numberOfFish; i++)
        {
            Vector3 spawnPosition = transform.position + Random.insideUnitSphere * spawnRadius;
            spawnPosition.y = 0; // Ensure fish spawn at Y = 0
            GameObject fish = Instantiate(fishPrefab, spawnPosition, Quaternion.identity, transform);
            fishList.Add(fish); // Add fish to the list
        }
    }

    public void RespawnFish(GameObject fish)
    {
        if(gameObject.active)
        StartCoroutine(RespawnAfterDelay(fish));
    }

    private IEnumerator RespawnAfterDelay(GameObject fish)
    {
        print("Respawn ");
        yield return new WaitForSeconds(respawnTime);

        // Reset the fish position (optional) and re-enable it
        fish.transform.position = fish.transform.position + Random.insideUnitSphere * spawnRadius;
        fish.transform.position = new Vector3(fish.transform.position.x, 0, fish.transform.position.z); // Y = 0
        if(fish.TryGetComponent<FishFlock>(out FishFlock flock))
        {
            flock.enabled = true;
        }
        fish.SetActive(true);
    }
}
