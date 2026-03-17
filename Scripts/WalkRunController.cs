using UnityEngine;

public class WalkRunController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private float runSpeed = 7f;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform cameraTransform;
    
    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrainRate = 15f;
    [SerializeField] private float staminaRegenRate = 10f;
    [SerializeField] private float staminaRegenDelay = 1f;
    
    [Header("Gravity")]
    [SerializeField] private float groundDrag = 5f;
    
    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private float currentStamina;
    private float timeSinceLastRun = 0f;
    private bool isRunning = false;
    private bool canRun = true;
    
    private float gravity = -9.81f;
    private float verticalVelocity = 0f;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        if (animator == null)
            animator = GetComponent<Animator>();
        
        currentStamina = maxStamina;
    }

    private void Update()
    {
        HandleInput();
        HandleMovement();
        UpdateStamina();
        UpdateAnimations();
    }

    private void HandleInput()
    {
        // Get movement input relative to camera direction
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Calculate movement direction relative to camera
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        // Remove vertical tilt from camera
        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        moveDirection = (camForward * vertical + camRight * horizontal).normalized;

        // Get run input (Left Shift)
        bool runInput = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        // Only allow running if: moving, holding shift, have stamina, and enough stamina to continue
        if (runInput && moveDirection.magnitude > 0 && canRun && currentStamina > 5f)
        {
            isRunning = true;
            timeSinceLastRun = 0f;
        }
        else
        {
            isRunning = false;
        }
    }

    private void HandleMovement()
    {
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        // Horizontal movement
        Vector3 horizontalMove = moveDirection * currentSpeed;

        // Apply gravity
        if (characterController.isGrounded)
        {
            if (verticalVelocity < 0)
                verticalVelocity = -2f; // keeps player grounded
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 finalMove = new Vector3(horizontalMove.x, verticalVelocity, horizontalMove.z);

        characterController.Move(finalMove * Time.deltaTime);

        // Rotate player to face movement direction
        if (moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }
    }

    private void UpdateStamina()
    {
        if (isRunning)
        {
            // Drain stamina while running
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Max(0, currentStamina);

            // Disable running if out of stamina
            if (currentStamina <= 0)
            {
                isRunning = false;
                canRun = false;
                timeSinceLastRun = 0f;
            }

            timeSinceLastRun = 0f;
        }
        else
        {
            // Wait before regenerating stamina
            timeSinceLastRun += Time.deltaTime;

            if (timeSinceLastRun >= staminaRegenDelay)
            {
                // Regenerate stamina while not running
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Min(maxStamina, currentStamina);
            }

            // Allow running again once stamina is above threshold
            if (currentStamina > 30f)
            {
                canRun = true;
            }
        }
    }

    private void UpdateAnimations()
    {
        // Set animation parameters
        animator.SetFloat("Speed", moveDirection.magnitude);
        animator.SetBool("IsRunning", isRunning);
        animator.SetFloat("Stamina", currentStamina / maxStamina);
    }

    // Public methods for UI/other systems
    public float GetStaminaPercent()
    {
        return currentStamina / maxStamina;
    }

    public float GetCurrentStamina()
    {
        return currentStamina;
    }

    public bool IsCurrentlyRunning()
    {
        return isRunning;
    }

    public bool CanRun()
    {
        return canRun;
    }
}