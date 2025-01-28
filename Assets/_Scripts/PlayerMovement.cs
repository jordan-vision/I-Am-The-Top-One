using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    PlayerController controller;
    Rigidbody2D rb;
    float baseGravity;
    bool canJumpAgain = false;

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
        var isGrounded = ComputeIsGrounded();
        if (isGrounded)
        {
            canJumpAgain = false;
        }

        // Move left and right
        var moveDirection = GetMoveDirection();
        if (moveDirection != 0)
        {
            transform.localScale = new(moveDirection, 1, 1);
        }

        rb.velocity = (moveDirection * runspeed * Vector2.right) + (rb.velocity.y * Vector2.up);

        // Jump
        if (isGrounded && controller.GetJump())
        {
            Jump();
            canJumpAgain = true;
        }
        // Double jump
        if (canJumpAgain && controller.GetJumpDown())
        {
            Jump();
            canJumpAgain = false;
        }

        // CHange gravity
        rb.gravityScale = GetGravity();
    }

    private int GetMoveDirection()
    {
        var returnVal = 0;

        // Direction held down
        if (controller.GetLeft())
        {
            returnVal = -1;
        }
        if (controller.GetRight())
        {
            returnVal = 1;
        }

        // Switching direction
        if (controller.GetLeftDown())
        {
            returnVal = -1;
        }
        if (controller.GetRightDown())
        {
            returnVal = 1;
        }

        return returnVal;
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
