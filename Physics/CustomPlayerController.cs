using UnityEngine;
using UnityEngine.Events;

public class CustomPlayerController : MonoBehaviour {
    // Input
    private Vector2 movementInput;
    private bool jumpButton;

    // Movement
    private Vector2 targetMovement;
    private Vector2 currentMovement;

    // Input Time references
    private float lastCrouchStart;
    private float lastGroundTime;

    // Movement data model
    [SerializeField] private PlayerMovementData movementData;

    // References
    private Rigidbody2D rb;
    private Collider2D playerCollider;

    // Private state
    private bool landedThisFrame;

    // public State 
    public bool isGrounded { get; private set; }
    public bool isCrouched { get; private set; }

    // Events
    public UnityEvent OnLeaveGround = new UnityEvent();
    public UnityEvent OnLand = new UnityEvent();

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
    }

    public void FixedUpdate() {
        CheckForGround();

        HandleInput();
        HandleMovement();
    }

    private void HandleInput() {
        movementInput = new Vector2(Input.GetAxis("Horizontal"), 0.0f);
    }

    private void HandleMovement() {
        // Check crouch status
        if (!isCrouched) {
            if (movementInput.y < 0 && isGrounded) lastCrouchStart = Time.time;
        } else {
            if (landedThisFrame) lastCrouchStart = Time.time;
        }
        isCrouched = movementInput.y < 0 && isGrounded;

        targetMovement = new Vector2(movementInput.x, 0.0f);
        currentMovement = Vector2.Lerp(currentMovement, targetMovement, Time.fixedDeltaTime * 10f);

        var targetSpeed = currentMovement.x * movementData.runMaxSpeed;

        float accelRate;

        //Gets an acceleration value based on if we are accelerating (includes turning) 
        //or trying to decelerate (stop). As well as applying a multiplier if we're airborne.
        var runAccelAmount = movementData.GetRunAccelAmount(Time.fixedDeltaTime);
        var runDecelAmount = movementData.GetRunDecelAmount(Time.fixedDeltaTime);

        if (isGrounded)
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? runAccelAmount : runDecelAmount;
        else
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? runAccelAmount * movementData.accelInAir : runDecelAmount * movementData.deccelInAir;

        var speedDif = targetSpeed - rb.velocity.x;
        var force = speedDif * accelRate;

        rb.AddForce(Vector2.right * force, ForceMode2D.Force);
        //rb.velocity = new Vector2(rb.velocity.x + (Runner.DeltaTime  * force) / rb.mass, rb.velocity.y);
    }

    private void CheckForGround() {
        var startPosition = new Vector2(playerCollider.transform.position.x, playerCollider.transform.position.y +  + 0.5f);

        // Perform boxcast to check for obstacles
        RaycastHit2D[] hits = Physics2D.BoxCastAll(startPosition, playerCollider.bounds.size, 0f, Vector2.down, 0.1f);
        var validHit = false;

        foreach (var hit in hits) {

            if (hit.collider.tag == "Player") continue;
            if (hit.collider.isTrigger) continue;
            if (IsGroundCollision(hit)) {
                validHit = true;
                break;
            }
        }

        var previouslyGrounded = isGrounded;
        isGrounded = validHit;

        if (isGrounded && rb.velocity.y < 0.0f) {
            //endedJumpEarly = false;
        }

        if (isGrounded == true && previouslyGrounded == false) {
            // New Ground Collision
            //targetGravityScale = _fallSpeed;
            //currentDashMovement = 0.0f;

            landedThisFrame = true;
            //remainingJumps = jumpCount;

        } else if (isGrounded == false && previouslyGrounded == true) {
            // Just left the ground
            //leftTheGroundThisframe = true;
        }
    }

    private bool IsGroundCollision(RaycastHit2D hit) {
        return hit.normal.y > 0.7f;
    }
}
