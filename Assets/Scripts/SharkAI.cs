using UnityEngine;

public class SharkAI : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;
    public float detectionRadius = 10f;
    public float attackDistance = 1f;
    public Transform[] boats;                  // Array of all boat transforms
    private Transform currentTargetBoat;       // Closest boat to attack
    private Vector3 roamTarget;
    private bool isAttacking = false;
    public float fixedYPosition;

    private BoatHealth currentBoatHealth;
    public int damageMake;

    public Animator animator;
    public float damageCooldown = 0.2f;
    private float lastDamageTime = -999f;

    private bool isRetreating = false;
    private float retreatTime = 3f;
    private float retreatStartTime;
    private Vector3 retreatDirection;

    private void Start()
    {
        GenerateNewRoamTarget();
    }

    private void Update()
    {
        if (isRetreating)
        {
            Retreat();
            if (Time.time - retreatStartTime >= retreatTime)
            {
                isRetreating = false;
                GenerateNewRoamTarget(); // Resume roaming after retreat
            }
            return;
        }

        currentTargetBoat = FindClosestBoat();

        if (currentTargetBoat != null && Vector3.Distance(transform.position, currentTargetBoat.position) <= detectionRadius)
        {
            currentBoatHealth = currentTargetBoat.GetComponent<BoatHealth>();
            isAttacking = true;
            MoveTowardsBoat();
        }
        else
        {
            isAttacking = false;
            FreeRoam();
        }
    }


    //private void DealDamageToBoat()
    //{
    //    if (currentBoatHealth == null) return;

    //    if (Time.time >= lastDamageTime + damageCooldown)
    //    {
    //        currentBoatHealth.TakeDamage(damageMake);
    //        animator.SetTrigger("eat");
    //        lastDamageTime = Time.time;

    //        // Start retreat
    //        isRetreating = true;
    //        retreatStartTime = Time.time;
    //    }
    //}


    private Transform FindClosestBoat()
    {
        Transform closest = null;
        float minDistance = float.MaxValue;

        foreach (Transform boat in boats)
        {
            if (boat == null) continue;
            float distance = Vector3.Distance(transform.position, boat.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = boat;
            }
        }

        return closest;
    }

    private void MoveTowardsBoat()
    {
        Vector3 directionToBoat = (currentTargetBoat.position - transform.position).normalized;

        if (Vector3.Distance(transform.position, currentTargetBoat.position) > attackDistance)
        {
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(directionToBoat.x, 0f, directionToBoat.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            Vector3 movement = transform.forward * moveSpeed * 1.5f * Time.deltaTime;
            transform.position = new Vector3(transform.position.x + movement.x, fixedYPosition, transform.position.z + movement.z);
        }
        else
        {
            DealDamageToBoat();
        }
    }

    private void FreeRoam()
    {
        Vector3 directionToTarget = (roamTarget - transform.position).normalized;

        if (Vector3.Distance(transform.position, roamTarget) > 1.5f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(directionToTarget.x, 0f, directionToTarget.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            Vector3 movement = transform.forward * moveSpeed * Time.deltaTime;
            transform.position = new Vector3(transform.position.x + movement.x, fixedYPosition, transform.position.z + movement.z);
        }
        else
        {
            GenerateNewRoamTarget();
        }
    }

    private void DealDamageToBoat()
    {
        if (currentBoatHealth == null) return;

        if (Time.time >= lastDamageTime + damageCooldown)
        {
            currentBoatHealth.TakeDamage(damageMake);
            animator.SetTrigger("eat");
            lastDamageTime = Time.time;

            // Begin retreat in opposite direction of boat
            isRetreating = true;
            retreatStartTime = Time.time;

            // Calculate direction away from the boat
            Vector3 awayFromBoat = (transform.position - currentTargetBoat.position).normalized;
            retreatDirection = new Vector3(awayFromBoat.x, 0f, awayFromBoat.z); // flatten on Y
        }
    }


    private void Retreat()
    {
        // Smoothly rotate away from the boat
        Quaternion targetRotation = Quaternion.LookRotation(retreatDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Move in that direction
        Vector3 movement = retreatDirection * moveSpeed * Time.deltaTime;
        transform.position = new Vector3(transform.position.x + movement.x, fixedYPosition, transform.position.z + movement.z);
    }


    private void GenerateNewRoamTarget()
    {
        float roamRadius = 10f;
        Vector3 randomDirection = Random.insideUnitSphere * roamRadius;
        randomDirection.y = 0;
        roamTarget = new Vector3(transform.position.x + randomDirection.x, fixedYPosition, transform.position.z + randomDirection.z);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
