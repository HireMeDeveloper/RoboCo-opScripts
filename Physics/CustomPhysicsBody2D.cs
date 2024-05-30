using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class CustomPhysicsBody2D : MonoBehaviour {
    public float gravity = 1.0f;
    public float defaultDampingFactor = 0.98f; // Damping factor for regular surfaces
    public float iceDampingFactor = 0.99f; // Damping factor on ice
    public int additionalJumps = 1; // Number of additional jumps after the first

    private Vector2 customVelocity;
    private int jumpsRemaining;
    private bool isOnIce = false;
    private bool isGrounded = true; // Track grounded state

    // References
    private Rigidbody2D rb;

    // UnityEvents for ground state changes
    public UnityEvent OnLeaveGround = new UnityEvent();
    public UnityEvent OnLand = new UnityEvent();

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; // Disable the built-in gravity
    }

    void FixedUpdate() {
        // Apply custom gravity
        var gravityVector = new Vector2(0.0f, -gravity);
        AddCustomForce(gravityVector * 100.0f);

        // Apply damping to custom velocity based on the surface type
        float currentDampingFactor = isOnIce ? iceDampingFactor : defaultDampingFactor;
        customVelocity *= currentDampingFactor;

        // Apply custom velocity to the rigidbody
        rb.MovePosition(rb.position + customVelocity * Time.fixedDeltaTime);

        // Ground check and invoke events if necessary
        bool wasGrounded = isGrounded;
        isGrounded = IsGrounded();

        if (wasGrounded && !isGrounded) {
            OnLeaveGround.Invoke();
        } else if (!wasGrounded && isGrounded) {
            OnLand.Invoke();
        }

        // Reset vertical velocity if grounded
        if (isGrounded) {
            customVelocity.y = 0;
            jumpsRemaining = additionalJumps; // Reset jumps when grounded
        }
    }

    public void SetCustomVelocity(Vector2 velocity) {
        customVelocity = velocity;
    }

    public void AddCustomForce(Vector2 force) {
        customVelocity += force * Time.fixedDeltaTime;
    }

    public void Jump(float jumpForce) {
        if (isGrounded || jumpsRemaining > 0) {
            customVelocity.y = jumpForce * 10.0f;
            if (!isGrounded) {
                jumpsRemaining--;
            }
        }
    }

    public void SetIsOnIce(bool onIce) {
        isOnIce = onIce;
    }

    private bool IsGrounded() {
        // Simple ground check (example, adjust as needed)
        return Physics2D.Raycast(transform.position, Vector2.down, 1.1f);
    }
}
