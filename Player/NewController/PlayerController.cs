using Fusion;
using UnityEngine;

public class PlayerController : NetworkBehaviour, ISpawned, IDespawned {


    // References
    private PlayerPhysics2D playerPhysics;
    private Animator anim;
    private PlayerRenderer playerRenderer;
    private PlayerParticleSystem playerParticleSystem;

    // Camera
    private GameplayCameraController cameraController;
    [SerializeField] private Transform interpolationTarget;

    // Networking

    // Events

    public override void Spawned() {
        cameraController = Camera.main.GetComponent<GameplayCameraController>();
        if (cameraController != null) cameraController.AddPlayer(interpolationTarget);

        // Subscribe to physics events
        playerPhysics.OnLand.AddListener(() => OnLand());

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

        // Unsubscribe from physics events
        playerPhysics.OnLand.RemoveListener(() => OnLand());

        // Unsubscribe from block events
        var toggleBlocks = GameObject.FindObjectsOfType<ToggleBlock>();
        foreach (var toggleBlock in toggleBlocks) {
            //toggleBlock.UnsubscribeToJump(this);
        }
    }

    private void Awake() {
        playerPhysics = GetComponent<PlayerPhysics2D>();
        playerRenderer = GetComponent<PlayerRenderer>();
        playerParticleSystem = GetComponent<PlayerParticleSystem>();
        anim = GetComponent<Animator>();
    }

    public override void FixedUpdateNetwork() {
        if (Runner.IsForward) SetAnimationParameters();
    }

    private void SetAnimationParameters() {
        // Update the animator parameter for speed
        anim.SetFloat("speed", Mathf.Abs(playerPhysics.velocity.x));

        anim.SetBool("grounded", playerPhysics.isGrounded);
        anim.SetBool("crouch", playerPhysics.isCrouched);

        playerRenderer.SetFlip(playerPhysics.isFlipped);
    }

    private void OnLand() {
        anim.SetTrigger("land");
    }
}
