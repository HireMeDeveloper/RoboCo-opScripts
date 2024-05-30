using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Player : NetworkBehaviour, ISpawned, IDespawned {
    // Config Stats
    [SerializeField] private float _moveSpeed = 4.0f;
    private float _accelerationRate = 9.0f;
    private float _decelerationRate = 10.0f;

    private float dashDampening = 2.0f;
    private float dashSpeed = 10.0f;

    private float _treadMoveSpeed = 2.0f;
    private float _iceSpeedModifier = 1.3f;
    private float iceSlideModifier = 0.1f;
    private float _airbornModifier = 0.6f;
    [Space]
    private float _jumpVelocity = 13f;
    private float _doubleJumpVelocity = 11f;

    private float _crouchJumpBonusVelocity = 0f;
    private float _crouchJumpBonusDelay = 0.25f;
    [Space]
    private float _fallSpeed = 3.5f;
    private float _endJumpGravityModifier = 80f;
    [Space]
    private float _apexBonusDuration = 0.1f;
    private float _apexSpeedModifier = 1.4f;
    private float _apexGravityModifier = 0.9f;
    [Space]
    private float _jumpBuffer = 0.1f;
    private float _coyoteTime = 0.16f;

    // Input and State
    private Vector2 currentMovement;
    private Vector2 treadMovement;
    private Vector2 previousPosition;

    private float lastYVelocity;

    private int jumpCount = 2;
    private int remainingJumps = 1;

    private float targetMoveSpeed;
    private float currentMoveSpeed;

    private float _targetAccelarationRate;
    private float _currentAccelarationRate;

    private float _targetDecelerationRate;
    private float _currentDecelerationRate;

    private float targetGravityScale;
    private float currentGravityScale;

    private bool isCrouched;
    private float lastCrouchStart;

    private bool groundedOnAWallThisFrame;

    private bool isHoldingJump;
    private bool jumpThisFrame;
    private bool endedJumpEarly;
    private float lastJumpPressed;

    private bool landedThisFrame;
    private bool leftTheGroundThisframe;
    private float lastFallTime;

    private bool isApexModifierActive;

    // Dash movement
    private float currentDashMovement;

    // Collision
    private List<Collision2D> validColliders = new List<Collision2D>();

    private bool isGrounded;
    private bool isDropping = false;
    [HideInInspector] public List<Collider2D> twoWayPlatformColliders = new List<Collider2D>();
    [HideInInspector] public List<Collider2D> removeAfterDropDown = new List<Collider2D>();

    private List<GameObject> currentIceBlocks = new List<GameObject>();
    private List<GameObject> currentLeftTreads = new List<GameObject>();
    private List<GameObject> currentRightTreads = new List<GameObject>();

    // Animation
    private Animator anim;

    // Component references
    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private PlayerRenderer playerRenderer;
    private PlayerParticleSystem playerParticleSystem;

    // Camera
    private GameplayCameraController cameraController;
    [SerializeField] private Transform interpolationTarget;

    // Networking
    [Networked] public NetworkButtons ButtonsPrevious { get; set; }
    [HideInInspector] private bool validXMovement { get; set; }

    [Networked, OnChangedRender(nameof(OnFlipChanged))] private bool flipSprite { get; set; }
    private Vector2 targetMovement { get; set; }

    //private bool IsGrounded() => validColliders.Count > 0;

    // Events
    public UnityEvent onJump;

    #region Unity event methods

    public override void Spawned() {
        cameraController = Camera.main.GetComponent<GameplayCameraController>();
        if (cameraController != null) cameraController.AddPlayer(interpolationTarget);

        // Subscribe to block events
        var toggleBlocks = GameObject.FindObjectsOfType<ToggleBlock>();
        foreach (var toggleBlock in toggleBlocks) {
            //toggleBlock.SubscribeToJump(this);
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState) {
        if (Camera.main != null) {
            cameraController = Camera.main.GetComponent<GameplayCameraController>();
            if (cameraController != null) cameraController.RemovePlayer(interpolationTarget);
        }

        // Unsubscribe from block events
        var toggleBlocks = GameObject.FindObjectsOfType<ToggleBlock>();
        foreach (var toggleBlock in toggleBlocks) {
            //toggleBlock.UnsubscribeToJump(this);
        }
    }

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();

        playerCollider = GetComponent<Collider2D>();
        playerRenderer = GetComponent<PlayerRenderer>();
        playerParticleSystem = GetComponent<PlayerParticleSystem>();
        anim = GetComponent<Animator>();
    }

    private void Start() {
        previousPosition = transform.position;

        targetMoveSpeed = _moveSpeed;
        targetGravityScale = _fallSpeed;

        _currentAccelarationRate = _targetAccelarationRate;
        _currentDecelerationRate = _targetDecelerationRate;
    }

    public override void FixedUpdateNetwork() {
        if (HasStateAuthority) {
            CheckForGround();

            HandleInput();
            HandleMovement();
        }

        //if (Runner.IsForward) SetAnimationParameters();
        SetAnimationParameters();
    }

    private void LateUpdate() {
        if (landedThisFrame == true) landedThisFrame = false;

        if (leftTheGroundThisframe == true) leftTheGroundThisframe = false;
    }
    #endregion

    private void HandleInput() {
        if (!GetInput(out NetworkInputData data)) return;

        var pressed = data.buttons.GetPressed(ButtonsPrevious);
        var released = data.buttons.GetReleased(ButtonsPrevious);
        ButtonsPrevious = data.buttons;

        targetMovement = (isCrouched) ? new Vector2(0, data.movement.y) : data.movement;
        targetMovement = new Vector2(Mathf.Clamp(targetMovement.x, -1, 1), Mathf.Clamp(targetMovement.y, -1, 1));

        var attemptedJump = pressed.IsSet(MyButtons.Jump);

        // Check if the player attempted a jump
        if (attemptedJump) {
            // Either jump if grounded, or buffer
            if (isGrounded || lastFallTime + _coyoteTime > Time.time) {
                jumpThisFrame = true;
            } else if (remainingJumps != jumpCount && remainingJumps > 0) {
                jumpThisFrame = true;
            } else {
                lastJumpPressed = Time.time;
            }
        } else {
            // Start Coyote timer if player left the ground this frame
            if (leftTheGroundThisframe) lastFallTime = Time.time;
        }

        if (landedThisFrame && lastJumpPressed + _jumpBuffer > Time.time) {
            jumpThisFrame = true;
        }

        isHoldingJump = data.buttons.IsSet(MyButtons.Jump);
    }

    private void HandleMovement() {
        // Check crouch status
        if (!isCrouched) {
            if (targetMovement.y < 0 && isGrounded) lastCrouchStart = Time.time;
        } else {
            if (landedThisFrame) lastCrouchStart = Time.time;
        }
        isCrouched = targetMovement.y < 0 && isGrounded;

        // Calculate current move speed
        if (isApexModifierActive) {
            targetMoveSpeed = _moveSpeed * _apexSpeedModifier;
        } else if (currentIceBlocks.Count > 0) {
            targetMoveSpeed = _moveSpeed * _iceSpeedModifier;
        } else {
            targetMoveSpeed = _moveSpeed;
        }
        currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, targetMoveSpeed, Runner.DeltaTime * 2f);

        // Settle the accelaeration and deceleration as well as current movement
        CalculateAccelerationAndDeceleration();

        // Move player based on current movement and treads
        var hasRightTreads = currentRightTreads.Count > 0;
        var hasLeftTreads = currentLeftTreads.Count > 0;

        var treadMovement = new Vector2((hasLeftTreads) ? -1 : 0, 0) + new Vector2((hasRightTreads) ? 1 : 0, 0);

        var movement = new Vector2(((currentMovement.x * currentMoveSpeed) + (treadMovement.x * _treadMoveSpeed)) * Runner.DeltaTime, 0.0f);

        // Dampen the horizontal velocity
        //var currentHorizontalVelocity = rb.velocity.x;
        //if (Mathf.Abs(currentHorizontalVelocity) < 0.25f) {
        //    var dotProduct = movement.x * currentHorizontalVelocity;
        //
        //    if (dotProduct < 0) {
        //        // Different direction
        //        currentHorizontalVelocity = Mathf.Lerp(currentHorizontalVelocity, 0.0f, Runner.DeltaTime * 4.0f);
        //        if (!isGrounded) movement = Vector2.zero;
        //    } else {
        //        currentHorizontalVelocity = Mathf.Lerp(currentHorizontalVelocity, 0.0f, Runner.DeltaTime * 2.5f);
        //    }
        //
        //    rb.velocity = new Vector2(currentHorizontalVelocity, rb.velocity.y);
        //}

        CheckForValidMovement(movement);
        if (validXMovement) {
            rb.AddForce(movement);
        }

        // Update Movement Particles
        playerParticleSystem.isWalking = targetMovement.x != 0 && isGrounded && validXMovement;

        // Adjust gravity scale
        currentGravityScale = Mathf.Lerp(currentGravityScale, targetGravityScale, Runner.DeltaTime * 6f);
        rb.gravityScale = currentGravityScale;

        // Dampen upward velocity if player ended jump early
        if (endedJumpEarly && rb.velocity.y > 0) {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y - _endJumpGravityModifier * Runner.DeltaTime);
        }

        // Store last Y velocity for later
        lastYVelocity = rb.velocity.y;

        // Either jump or Drop through a two way platform
        if (jumpThisFrame) {
            if (isCrouched && twoWayPlatformColliders.Count > 0 /* && Time.time < lastCrouchStart + _crouchJumpBonusDelay */) {
                // Check to see if there is more ground that would prevent the player from dropping
                var startPosition = new Vector2(playerCollider.transform.position.x, playerCollider.transform.position.y +  + 0.5f);
                var layerMask = LayerMask.GetMask("Default");
                RaycastHit2D[] hits = Physics2D.BoxCastAll(startPosition, playerCollider.bounds.size, 0f, Vector2.down, 0.1f, layerMask);
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

        // Flip Sprite based on movement
        if (targetMovement.x != 0) {
            flipSprite = targetMovement.x < 0;
        }
    }

    private void CheckForValidMovement(Vector2 movement) {
        validXMovement = CanMove(movement);
    }

    bool CanMove(Vector2 movement) {
        var startPosition = new Vector2(playerCollider.transform.position.x, playerCollider.transform.position.y +  + 0.5f);

        var layerMask = LayerMask.GetMask("Default");
        // Perform boxcast to check for obstacles
        RaycastHit2D[] hits = Physics2D.BoxCastAll(startPosition, playerCollider.bounds.size, 0f, movement.normalized, 0.1f, layerMask);
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

    private void CalculateAccelerationAndDeceleration() {
        if (currentIceBlocks.Count > 0) {
            _currentAccelarationRate = _accelerationRate * iceSlideModifier * 0.75f;
            _currentDecelerationRate = _decelerationRate * iceSlideModifier;
        } else {
            _currentAccelarationRate = Mathf.Lerp(_currentAccelarationRate, _accelerationRate, Runner.DeltaTime * 3.0f);
            _currentDecelerationRate = Mathf.Lerp(_currentDecelerationRate, _decelerationRate, Runner.DeltaTime * 3.0f);
        }

        // Apply accelaration/deceleration to movement based on input magnitude
        var preSmoothingRate = (targetMovement.magnitude == 0) ? _currentDecelerationRate : _currentAccelarationRate;

        var preModifiedSmoothingRate = (!isGrounded) ? preSmoothingRate * _airbornModifier : preSmoothingRate;
        currentMovement = Vector2.Lerp(currentMovement, targetMovement, Runner.DeltaTime * preModifiedSmoothingRate);


        var applyStop = false;
        if (currentMovement.x > 0 && !CanMove(new Vector2(1, 0))) {
            applyStop = true;
        } else if (currentMovement.x < 0 && !CanMove(new Vector2(-1, 0))) {
            applyStop = true;
        }

        if (applyStop) {
            var modifiedSmoothingRate = (!isGrounded) ? _decelerationRate * _airbornModifier : _decelerationRate;
            currentMovement = Vector2.Lerp(currentMovement, targetMovement, Runner.DeltaTime * modifiedSmoothingRate);
        }
    }

    private void SetAnimationParameters() {

        // Update the animator parameter for speed
        anim.SetFloat("speed", (validXMovement) ? Mathf.Abs(targetMovement.x) : 0);

        anim.SetBool("grounded", isGrounded);
        anim.SetBool("crouch", targetMovement.y < 0);

        if (landedThisFrame) {
            anim.SetTrigger("land");
        }
    }

    private void OnFlipChanged() {
        playerRenderer.SetFlip(flipSprite);
    }

    private void Jump() {
        jumpThisFrame = false;

        if (remainingJumps == jumpCount) {
            anim.SetTrigger("jump");
            rb.velocity = new Vector2(rb.velocity.x, _jumpVelocity);
        } else {
            anim.SetTrigger("jump");
            rb.velocity = new Vector2(rb.velocity.x, _doubleJumpVelocity);
        }

        remainingJumps--;

        //var crouchBonus = ((isCrouched && Time.time > lastCrouchStart + _crouchJumpBonusDelay) ? _crouchJumpBonusVelocity : 0);

        StartCoroutine(JumpTimer());
        OnJumpRpc();
    }

    private IEnumerator JumpTimer() {
        endedJumpEarly = false;
        yield return new WaitForSeconds(0.025f);

        while (rb.velocity.y > 0) {

            if (!isHoldingJump) {
                endedJumpEarly = true;
                break;
            }

            yield return new WaitForEndOfFrame();
        }

        isApexModifierActive = true;
        targetGravityScale *= _apexGravityModifier;

        yield return new WaitForSeconds(_apexBonusDuration);

        isApexModifierActive = false;
        targetGravityScale /= _apexGravityModifier;
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
                if (hit.collider.tag == "TwoWayPlatform" && lastYVelocity > 0) continue;

                validHit = true;
                break;
            }
        }

        var previouslyGrounded = isGrounded;
        isGrounded = validHit;

        if (isGrounded && rb.velocity.y < 0.0f) {
            endedJumpEarly = false;
        }

        if (isGrounded == true && previouslyGrounded == false) {
            // New Ground Collision
            targetGravityScale = _fallSpeed;
            //currentDashMovement = 0.0f;

            landedThisFrame = true;
            remainingJumps = jumpCount;

            playerParticleSystem.SpawnLandParticleRpc();
        } else if (isGrounded == false && previouslyGrounded == true) {
            // Just left the ground
            leftTheGroundThisframe = true;
        }
    }

    private bool IsGroundCollision(RaycastHit2D hit) {
        return hit.normal.y > 0.7f;
    }

    public void RemoveTwoWayPlatfromCollider(Collider2D twoWayCollider) {
        if (isDropping) {
            removeAfterDropDown.Add(twoWayCollider);
        } else {
            twoWayPlatformColliders.Remove(twoWayCollider);
        }
    }

    private void DropThrough() {
        jumpThisFrame = false;
        StartCoroutine(DropTimer());
    }

    private IEnumerator DropTimer() {
        isDropping = true;
        foreach (var twoWayCollider in twoWayPlatformColliders) {
            if (twoWayCollider == null) continue;
            Physics2D.IgnoreCollision(playerCollider, twoWayCollider, true);
        }

        yield return new WaitForSeconds(0.2f);

        foreach (var twoWayCollider in twoWayPlatformColliders) {
            if (twoWayCollider == null) continue;
            Physics2D.IgnoreCollision(playerCollider, twoWayCollider, false);
        }
        isDropping = false;

        foreach (var twoWayCollider in removeAfterDropDown) {
            twoWayPlatformColliders.Remove(twoWayCollider);
        }
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

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void OnJumpRpc() {
        onJump.Invoke();
    }

    public void SetDash(Vector2 direction) {
        //rb.velocity = new Vector2(direction.x * 20, direction.y * 25);

        playerParticleSystem.SpawnFireBounceParticlesRpc();
    }
}
