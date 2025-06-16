using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BoatHealth : MonoBehaviour
{
    public int maxHealth = 100;   // Maximum health of the boat
    public float currentHealth;    // Current health of the boat
    public Vector3 startPos;
    public Quaternion startRot;
    private void Start()
    {
        // Initialize the boat's health
        currentHealth = maxHealth;
        startPos=transform.position;
        startRot=transform.rotation;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        // Clamp health to ensure it doesn't drop below zero
        currentHealth = Mathf.Max(0, currentHealth);

        Debug.Log("Boat took damage! Current health: " + currentHealth);

        // Check if the boat is destroyed
        if (currentHealth <= 0)
        {
            StartCoroutine(DestroyBoat());
        }
        transform.position = new Vector3(
     transform.position.x,
     -((maxHealth - currentHealth) / maxHealth), // Maps health range to [0, -1]
     transform.position.z
 );

    }





    private IEnumerator DestroyBoat()
    {
        Debug.Log("The boat has been destroyed!");
        yield return new WaitForSeconds(5);
        //SceneManager.LoadScene(0);
        transform.position = startPos;
        transform.rotation = startRot;
        currentHealth = maxHealth;
        // Add boat destruction logic here (e.g., trigger game over or respawn)
    }

    public float GetHealth()
    {
        return currentHealth;
    }
}
