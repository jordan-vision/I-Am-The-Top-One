using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    PlayerController controller;
    Rigidbody2D rb;
    float baseGravity;
    bool canJumpAgain = false;
    int moveDirection;

    [SerializeField] int runspeed, jumpForce;
    [SerializeField] float lowJumpModifier, fallModifier;
    [SerializeField] Transform groundCheck1, groundCheck2;

    private void Start()
    {
        controller = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
        baseGravity = rb.gravityScale;
    }

    private void Update()
    {
        // On landing, reset canJumAgain
        var isGrounded = ComputeIsGrounded();
        if (isGrounded)
        {
            canJumpAgain = false;
        }

        Move();

        // Double jump
        if (canJumpAgain && controller.GetJumpDown())
        {
            Jump();
            canJumpAgain = false;
        }
        // Jump
        if (isGrounded && controller.GetJump())
        {
            Jump();
            canJumpAgain = true;
        }
        
        // CHange gravity
        rb.gravityScale = GetGravity();
    }

    private void Move()
    {
        // Computing move direction
        if (controller.GetLeftDown())
        {
            moveDirection = -1;
        }
        if (controller.GetRightDown())
        {
            moveDirection = 1;
        }
        if (!controller.GetLeft() && !controller.GetRight())
        {
            moveDirection = 0;
        }

        // Flipping character
        if (moveDirection != 0)
        {
            transform.localScale = new(moveDirection, 1, 1);
        }

        // Moving character
        rb.velocity = (moveDirection * runspeed * Vector2.right) + (rb.velocity.y * Vector2.up);
    }
    
    private float GetGravity()
    {
        var returnVal = baseGravity;

        if (!controller.GetJump() && !ComputeIsGrounded())
        {
            returnVal += lowJumpModifier;
        }

        if (rb.velocity.y < 0)
        {
            returnVal += fallModifier;
        }

        return returnVal;
    }

    private void Jump()
    {
        rb.velocity = new(rb.velocity.x, 0);
        rb.AddForce(new(0, jumpForce), ForceMode2D.Impulse);
    }

    private bool ComputeIsGrounded()
    {
        var ray1 = Physics2D.RaycastAll(groundCheck1.position, Vector2.down, 1.0f / 16.0f);
        var ray2 = Physics2D.RaycastAll(groundCheck2.position, Vector2.down, 1.0f / 16.0f);

        foreach (var hit in ray1)
        {
            if (hit.collider != null && hit.collider.CompareTag("Ground"))
            {
                return true;
            }
        }

        foreach (var hit in ray2)
        {
            if (hit.collider != null && hit.collider.CompareTag("Ground"))
            {
                return true;
            }
        }

        return false;
    }
}
