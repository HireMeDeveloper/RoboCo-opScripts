using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerPhysics2D : NetworkBehaviour {
    // Input
    private Vector2 movementInput;
    private bool jumpThisFrame;
    private bool isHoldingJump;

    // Movement
    private Vector2 targetMovement;
    private Vector2 currentMovement;
    private float targetGravityScale;

    // Input Time references
    private float lastCrouchStart;
    private float lastGroundTime;
    private float lastFallTime;
    private float lastJumpTime;

    // Movement data model
    [SerializeField] private PlayerMovementData movementData;

    // References
    private Rigidbody2D rb;
    private Collider2D playerCollider;

    // Private state
    private bool landedThisFrame;
    private bool leftTheGroundThisFrame;
    private int remainingJumps;
    private bool isJumpCut;
    private bool isJumpFalling;
    private bool validXMovement;

    // Collision
    private bool isDropping = false;
    public List<TwoWayPlatform> twoWayPlatforms = new List<TwoWayPlatform>();

    private List<GameObject> currentIceBlocks = new List<GameObject>();
    private List<GameObject> currentLeftTreads = new List<GameObject>();
    private List<GameObject> currentRightTreads = new List<GameObject>();

    // public State 
    public bool isGrounded { get; private set; }
    public bool isCrouched { get; private set; }
    public bool isJumping { get; private set; }
    public bool isFlipped { get; private set; }
    public bool isMoving { get { return Mathf.Abs(movementInput.x) > 0.0f; } }
    public Vector2 velocity { get { return rb.velocity; } }

    // Networking
    [Networked] public NetworkButtons ButtonsPrevious { get; set; }

    // Events
    public UnityEvent OnLeaveGround = new UnityEvent();
    public UnityEvent OnLand = new UnityEvent();
    public UnityEvent OnJump = new UnityEvent();

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
    }

    public override void FixedUpdateNetwork() {
        if (HasStateAuthority) {
            CheckForGround();

            HandleInput();
            HandleMovement();
        }
    }

    private void LateUpdate() {
        if (landedThisFrame == true) landedThisFrame = false;
        if (leftTheGroundThisFrame == true) leftTheGroundThisFrame = false;
    }

    private void HandleInput() {
        if (!GetInput(out NetworkInputData data)) return;

        var pressed = data.buttons.GetPressed(ButtonsPrevious);
        var released = data.buttons.GetReleased(ButtonsPrevious);
        ButtonsPrevious = data.buttons;

        movementInput = (isCrouched) ? new Vector2(0, data.movement.y) : data.movement;
        movementInput = new Vector2(Mathf.Clamp(movementInput.x, -1, 1), Mathf.Clamp(movementInput.y, -1, 1));

        if (movementInput.x != 0.0f) {
            isFlipped = movementInput.x < 0.0f;
        }

        var attemptedJump = pressed.IsSet(MyButtons.Jump);
        var releasedJump = released.IsSet(MyButtons.Jump);

        // Check if the player attempted a jump
        if (attemptedJump) {
            // Either jump if grounded, or buffer
            if (isGrounded || lastFallTime + movementData.coyoteTime > Time.time) {
                jumpThisFrame = true;
            } else if (remainingJumps != movementData.jumpCount && remainingJumps > 0) {
                jumpThisFrame = true;
            } else {
                lastJumpTime = Time.time;
            }
        } else {
            // Start Coyote timer if player left the ground this frame
            if (leftTheGroundThisFrame) lastFallTime = Time.time;
        }

        if (landedThisFrame && lastJumpTime + movementData.jumpInputBufferTime > Time.time) {
            jumpThisFrame = true;
        }

        if (releasedJump && isJumping) {
            isJumpCut = true;
        }
    }

    private void HandleMovement() {
        // Jump Checks
        if (isJumping && rb.velocity.y < 0.0f) {
            isJumpFalling = true;
        }

        if (isJumpCut && !isJumpFalling) {
            isJumpCut = false;

            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * movementData.jumpCutGravityMult);
        }

        // Check crouch status
        if (!isCrouched) {
            if (movementInput.y < 0 && isGrounded) lastCrouchStart = Time.time;
        } else {
            if (landedThisFrame) lastCrouchStart = Time.time;
        }
        isCrouched = movementInput.y < 0 && isGrounded;

        targetMovement = new Vector2(movementInput.x, 0.0f);
        currentMovement = Vector2.Lerp(currentMovement, targetMovement, Runner.DeltaTime * 10f);

        var targetSpeed = currentMovement.x * movementData.runMaxSpeed;

        float accelRate;

        //Gets an acceleration value based on if we are accelerating (includes turning) 
        //or trying to decelerate (stop). As well as applying a multiplier if we're airborne.
        var runAccelAmount = movementData.GetRunAccelAmount(Runner.DeltaTime);
        var runDecelAmount = movementData.GetRunDecelAmount(Runner.DeltaTime);

        if (isGrounded)
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? runAccelAmount : runDecelAmount;
        else
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? runAccelAmount * movementData.accelInAir : runDecelAmount * movementData.deccelInAir;

        //We won't slow the player down if they are moving in their desired direction but at a greater speed than their maxSpeed
        if (movementData.doConserveMomentum && Mathf.Abs(rb.velocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(rb.velocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f && isGrounded) {
            //Prevent any deceleration from happening, or in other words conserve are current momentum
            //You could experiment with allowing for the player to slightly increae their speed whilst in this "state"
            accelRate = 0;
        }

        var speedDif = targetSpeed - rb.velocity.x;
        var force = speedDif * accelRate;

        CheckForValidMovement(Vector2.right * force);

        if (validXMovement) {
            rb.AddForce(Vector2.right * force, ForceMode2D.Force);
        } else {
            Debug.Log("Cannot move");
        }

        // Either jump or Drop through a two way platform
        if (jumpThisFrame) {

            CheckForTwoWayPlatforms();

            if (isCrouched && twoWayPlatforms.Count > 0) {
                // Check to see if there is more ground that would prevent the player from dropping
                var startPosition = new Vector2(playerCollider.transform.position.x, playerCollider.transform.position.y +  + 0.5f);
                var layerMask = LayerMask.GetMask("Default");
                RaycastHit2D[] hits = Physics2D.CapsuleCastAll(startPosition, playerCollider.bounds.size,CapsuleDirection2D.Vertical, 0f, Vector2.down, 0.25f, layerMask);
                var validHit = false;
                foreach (var hit in hits) {
                    if (hit.collider == null) continue;
                    if (hit.collider.tag == "TwoWayPlatform") continue;
                    if (!IsGroundCollision(hit)) continue;
                    validHit = true;
                    break;
                }

                if (validHit) Jump();
                else DropThrough();
            } else {
                Jump();
            }
        }
    }

    private void CheckForTwoWayPlatforms() {
        var startPosition = new Vector2(playerCollider.transform.position.x, playerCollider.transform.position.y +  + 0.5f);
        var layerMask = LayerMask.GetMask("Default");
        RaycastHit2D[] hits = Physics2D.CapsuleCastAll(startPosition, playerCollider.bounds.size,CapsuleDirection2D.Vertical, 0f, Vector2.down, 0.25f, layerMask);
        twoWayPlatforms.Clear();

        foreach (var hit in hits) {
            var twoWayCollider = hit.collider.GetComponent<TwoWayPlatform>();
            if (twoWayCollider == null) {
                continue;
            }
            twoWayPlatforms.Add(twoWayCollider);
        }
    }

    private void CheckForValidMovement(Vector2 movement) {
        validXMovement = CanMove(movement);
    }

    bool CanMove(Vector2 movement) {
        var startPosition = new Vector2(playerCollider.transform.position.x, playerCollider.transform.position.y + 0.5f);

        var layerMask = LayerMask.GetMask("Default");
        // Perform boxcast to check for obstacles
        RaycastHit2D[] hits = Physics2D.CapsuleCastAll(startPosition, playerCollider.bounds.size, CapsuleDirection2D.Vertical, 0f, movement.normalized, 0.2f, layerMask);
        var canMove = true;

        foreach (var hit in hits) {
            if (hit.collider == null) {
                continue;
            } else if (hit.collider.isTrigger) {
                continue;
            } else if (hit.collider.tag == "TwoWayPlatform") {
                continue;
            } else if (hit.collider.tag == "CollideAnyway") {
                continue;
            } else {
                if (Mathf.Abs(hit.normal.x) >= 0.5f && Mathf.Abs(hit.normal.y) <= 0.5f) {
                    canMove = false;
                    break;
                } else {
                    continue;
                }
            }
        }

        return canMove;
    }

    private void Jump() {
        jumpThisFrame = false;
        isJumping = true;
        isJumpCut = false;
        isJumpFalling = false;

        float force;

        if (remainingJumps == movementData.jumpCount) {
            // Jump like first jump
            force = movementData.primaryJumpForce;
        } else {
            // jump like second jump
            force = movementData.secondaryJumpForce;
        }

        if (rb.velocity.y < 0) force -= rb.velocity.y;
        rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);

        remainingJumps--;

        //var crouchBonus = ((isCrouched && Time.time > lastCrouchStart + _crouchJumpBonusDelay) ? _crouchJumpBonusVelocity : 0);
        OnJump.Invoke();
    }

    private void DropThrough() {
        jumpThisFrame = false;
        StartCoroutine(DropTimer());
    }

    private IEnumerator DropTimer() {
        isDropping = true;
        var dropDelay = 0.4f;

        foreach (var twoWayCollider in twoWayPlatforms) {
            if (twoWayCollider == null) {
                continue;
            }
            twoWayCollider.IgnoreCollisionForSeconds(playerCollider, dropDelay);
        }

        yield return new WaitForSeconds(dropDelay);
        isDropping = false;
    }

    private void CheckForGround() {
        var startPosition = new Vector2(playerCollider.transform.position.x, playerCollider.transform.position.y +  + 0.5f);

        // Perform boxcast to check for obstacles
        RaycastHit2D[] hits = Physics2D.CapsuleCastAll(startPosition, playerCollider.bounds.size, CapsuleDirection2D.Vertical, 0f, Vector2.down, 0.1f);
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

        //if (isGrounded && rb.velocity.y < 0.0f) {
        //    endedJumpEarly = false;
        //}

        if (isGrounded == true && previouslyGrounded == false) {
            // New Ground Collision
            LandOnTheGround();
        } else if (isGrounded == false && previouslyGrounded == true) {
            // Just left the ground
            LeaveTheGround();
        }
    }

    private void LeaveTheGround() {
        leftTheGroundThisFrame = true;

        if (!isJumping) remainingJumps--;

        OnLeaveGround.Invoke();
    }
    private void LandOnTheGround() {
        landedThisFrame = true;
        isJumping = false;

        //targetGravityScale = _fallSpeed;
        //currentDashMovement = 0.0f;
        remainingJumps = movementData.jumpCount;

        OnLand.Invoke();
    }

    private bool IsGroundCollision(RaycastHit2D hit) {
        return hit.normal.y > 0.7f;
    }

    public void AddIceBlock(GameObject iceBlock) {
        currentIceBlocks.Add(iceBlock);
    }

    public void RemoveIceBlock(GameObject iceBlock) {
        currentIceBlocks.Remove(iceBlock);
    }

    public void AddTreadBlock(GameObject treadBlock, bool movesRight) {
        if (movesRight) {
            currentRightTreads.Add(treadBlock);
        } else {
            currentLeftTreads.Add(treadBlock);
        }
    }

    public void RemoveTreadBlock(GameObject treadBlock, bool movesRight) {
        if (movesRight) {
            currentRightTreads.Remove(treadBlock);
        } else {
            currentLeftTreads.Remove(treadBlock);
        }
    }

    public void AddForce(Vector2 direction, float magnitude) {
        if (rb.velocity.y < 0) magnitude -= rb.velocity.y;
        rb.AddForce(direction * magnitude, ForceMode2D.Impulse);
    }
}
