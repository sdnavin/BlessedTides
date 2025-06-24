using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BoatHealth : MonoBehaviour
{
    public int maxHealth = 100;   // Maximum health of the boat
    public float currentHealth;    // Current health of the boat
    public Vector3 startPos;
    public Quaternion startRot;

    private Coroutine currentCoroutine;

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
            if(currentCoroutine != null) 
            {
              StopCoroutine(currentCoroutine);
              ResetBoatTransform();
            }
          currentCoroutine = StartCoroutine(DestroyBoat());
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

        // Disable movement or controls here if needed

        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(
            startRotation.eulerAngles.x,
            startRotation.eulerAngles.y + 180f,
            startRotation.eulerAngles.z
        );

        float duration = 1.5f; // Duration of flip in seconds
        float elapsed = 0f;

        // Smoothly rotate over time
        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure final rotation is exact
        transform.rotation = targetRotation;

        yield return new WaitForSeconds(5 - duration); // Wait remaining time

        ResetBoatTransform();

        // Enable movement or controls again if needed
    }

    private void ResetBoatTransform()
    {
        // Reset to original state
        transform.position = startPos;
        transform.rotation = startRot;
        currentHealth = maxHealth;
    }

    public float GetHealth()
    {
        return currentHealth;
    }
}
