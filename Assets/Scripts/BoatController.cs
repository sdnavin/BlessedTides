using System;
using UnityEngine;
using UnityEngine.Windows;

public class BoatController : MonoBehaviour
{
    public float speed = 5f;            // Movement speed
    public float turnSpeed = 180f;      // Turning speed
    public float swayAmount = 2f;       // The amount the boat sways left/right
    public float swaySpeed = 2f;        // Speed of the swaying motion

    public float acceleration = 2.5f;       // How fast it accelerates
    public float deceleration = 1.5f;       // How fast it slows down

    // Boost parameters
    public float boostMultiplier = 2.5f;  // How much faster the boat goes when boosting
    public float boostDuration = 1.0f;    // How long the boost lasts in seconds
    public float boostCooldown = 3.0f;    // Cooldown time before boost can be used again

    [SerializeField] GameObject boostTrail;
    private Rigidbody rb;               // Reference to the Rigidbody component
    private float swayTimer = 0f;       // Timer for the swaying motion
    private Transform childTransform;   // Reference to the first child object
    private Vector2 joystickInput = Vector2.zero; // Joystick input values

    // Boost state variables
    private bool isBoosting = false;
    private float boostTimer = 0f;
    private float cooldownTimer = 0f;

    public int slotID;
    public int slotoffset = 1;

    public BoatHealth boatHealth;

    private float moveValue;
    private float turn;

    private Vector3 velocity = Vector3.zero; // Current velocity



    private void Start()
    {
        boatHealth = GetComponent<BoatHealth>();
        rb = GetComponent<Rigidbody>(); // Get the Rigidbody component
        if (rb != null)
        {
            rb.useGravity = false;      // Disable gravity to prevent Y-axis movement
            rb.constraints = RigidbodyConstraints.FreezeRotationX |
                              RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY; // Freeze X/Z rotations
        }

        // Get the first child object of the boat (adjust this if needed)
        childTransform = transform.GetChild(0);
    }

    private void FixedUpdate()
    {
        HandleMovementJoystick();

        

        // Normal movement based on player input
        Vector3 forwardMovement = transform.forward * moveValue * Time.fixedDeltaTime;

        // Boost movement is always forward, regardless of moveValue
        Vector3 boostMovement = Vector3.zero;
        if (isBoosting)
        {
            boostMovement = transform.forward * boostMultiplier * Time.fixedDeltaTime;
        }

        // Combine both and apply movement
        rb.MovePosition(rb.position + forwardMovement + boostMovement);

        //// Calculate the forward movement direction
        //Vector3 forwardMovement = transform.forward * moveForward * Time.fixedDeltaTime;

        //// Apply boost if active
        //if (isBoosting)
        //{
        //    forwardMovement = transform.forward * Time.deltaTime * boostMultiplier;
        //    //transform.Translate(transform.forward * speed * boostMultiplier * Time.deltaTime, Space.World);
        //}

        //// Move the boat using Rigidbody
        //rb.MovePosition(rb.position + forwardMovement);

        // Rotate the boat using Rigidbody
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);

        // Keep the boat at a constant height (no Y-axis movement)
        Vector3 position = rb.position;
        position.y = 0f; // Keep Y position at 0 (or adjust to the desired height)
        rb.position = position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Portal")
        {
            other.GetComponent<PortalReveal>().OnReveal();
        }
    }

    private void HandleMovementJoystick()
    {
        if (WebSocketClient.dataIn[slotID] == null)
            return;

        //if (slotID != WebSocketClient.dataIn[slotID].data.slotId)
        //{
        //    return;
        //}
        if (WebSocketClient.dataIn[slotID].status == "boost")
        {
            boostNow = true;
            WebSocketClient.dataIn[slotID].status = "";
        }

        if (WebSocketClient.dataIn != null)
            joystickInput = Vector2.Lerp(joystickInput, new Vector2((float)WebSocketClient.dataIn[slotID].data.x * slotoffset, (float)WebSocketClient.dataIn[slotID].data.y * slotoffset), speed * Time.deltaTime);
        else
        {
            joystickInput = Vector2.zero;
        }

        // Get joystick input
        // If there's no input, don't move the boat
        //if (joystickInput.sqrMagnitude <= 0.1f)
        //    return;

        // Normalize the input for consistent movement
        Vector2 normalizedInput = joystickInput;

        // Apply boost multiplier to joystick movement if boosting
        float currentSpeed = isBoosting ? speed * boostMultiplier : speed;

        Vector3 targetDirection = new Vector3(normalizedInput.x, 0, normalizedInput.y);
        Vector3 targetVelocity = targetDirection * currentSpeed;

        // Accelerate or decelerate towards the target velocity
        velocity = Vector3.MoveTowards(velocity, targetVelocity,
                    (targetVelocity.magnitude > 0 ? acceleration : deceleration) * Time.deltaTime);

        // Apply movement
        transform.Translate(velocity * Time.deltaTime, Space.World);

        // Calculate the movement direction based on joystick input
        Vector3 direction = new Vector3(normalizedInput.x, 0, normalizedInput.y);

        //// Move the boat in the direction of the joystick input
        //transform.Translate(direction * currentSpeed * Time.deltaTime, Space.World);

        // Rotate the boat to face the movement direction
        if (direction.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speed);
        }
    }

    private void Update()
    {
        // Add slight swaying effect (rotate the first child object left/right)
        SwayEffect();

        // Handle boost input and timers
        HandleBoost();

        // Boat movement controls (using arrow keys or joystick)
        moveValue = (UnityEngine.Input.GetAxis("Vertical")) * speed;
        turn = (UnityEngine.Input.GetAxis("Horizontal")) * turnSpeed * Time.fixedDeltaTime;

        boostTrail.SetActive(isBoosting);
    }
    bool boostNow = false;
    // New method to handle boost functionality
    private void HandleBoost()
    {
        // If cooldown is active, reduce the timer
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        // Check for boost input (space bar)
        if (boostNow && cooldownTimer <= 0 && !isBoosting)
        {
            // Activate boost
            isBoosting = true;
            boostTimer = boostDuration;
            boostNow = false;
            // Add visual or audio effects for boost here if desired
            // Example: PlayBoostEffect();
        }

        // If boost is active, count down the timer
        if (isBoosting)
        {
            boostTimer -= Time.deltaTime;

            // If boost timer runs out, deactivate boost and start cooldown
            if (boostTimer <= 0)
            {
                isBoosting = false;
                cooldownTimer = boostCooldown;

                // Add visual or audio effects for boost end here if desired
                // Example: StopBoostEffect();
            }
        }
    }

    internal void updateFishCount(int _totalCount, int _unloadAmount)
    {
        WebSocketClient.instance.SendFishUpdateMessage((int)_totalCount, (int)_totalCount, (slotID+1));
    }

    internal void updateFishCatch(int _totalCount)
    {
        WebSocketClient.instance.SendFishCatchUpdateMessage((int)_totalCount, (int)_totalCount, (slotID + 1));
    }

    // Sway effect simulating boat rocking, applied to the first child object
    void SwayEffect()
    {
        swayTimer += Time.deltaTime * swaySpeed;
        float sway = Mathf.Sin(swayTimer) * swayAmount; // Sinusoidal movement for smooth swaying

        // Apply sway only to the first child's rotation (without affecting the parent)
        if (childTransform != null)
        {
            Vector3 currentRotation = childTransform.localEulerAngles;
            childTransform.localRotation = Quaternion.Euler(currentRotation.x, currentRotation.y, sway);
        }
    }

    // Optionally add an up/down wave effect for a more dynamic boat motion
    void SwayUpDownEffect()
    {
        float waveHeight = Mathf.Sin(Time.time * swaySpeed) * 0.1f; // Up/down motion
        Vector3 position = rb.position;
        position.y += waveHeight;
        rb.position = position;
    }
}