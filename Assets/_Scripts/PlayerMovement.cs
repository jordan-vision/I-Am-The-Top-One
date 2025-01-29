using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    PlayerController controller;
    Rigidbody2D rb;
    float baseGravity;
    bool canJumpAgain = false, isAttacking = false, isInCooldown = false, isInKnockback = false;
    int moveDirection, acceleration;

    [SerializeField] int runspeed, jumpForce, baseAcceleration;
    [SerializeField] float lowJumpModifier, fallModifier;
    [SerializeField] Transform groundCheck1, groundCheck2;
    [SerializeField] Attack[] attacks;

    private void Start()
    {
        controller = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
        baseGravity = rb.gravityScale;
        acceleration = baseAcceleration;
    }

    private void Update()
    {
        // On landing, reset canJumAgain
        var isGrounded = ComputeIsGrounded();
        if (canJumpAgain && isGrounded && !isInKnockback)
        {
            canJumpAgain = false;
        }

        Move();

        // Double jump
        if (canJumpAgain && controller.GetJumpDown() && !isInKnockback)
        {
            Jump();
            canJumpAgain = false;
        }
        // Jump
        else if (isGrounded && controller.GetJump())
        {
            Jump();
            canJumpAgain = true;
        }
        // Change gravity
        else
        {
            rb.gravityScale = GetGravity();
        }

        if (controller.GetAttackDown() && !isInCooldown)
        {
            StartCoroutine(Attack());
        }
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
        rb.velocity = Mathf.MoveTowards(rb.velocity.x, moveDirection * runspeed, acceleration * Time.deltaTime) * Vector2.right
            + rb.velocity.y * Vector2.up;
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

            // Recover from knockback
            if (isInKnockback)
            {
                isInKnockback = false;
                acceleration = baseAcceleration;
            }
        }

        return returnVal;
    }

    private void Jump()
    {
        rb.gravityScale = baseGravity;
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

    private IEnumerator Attack()
    {
        isInCooldown = true;
        isAttacking = true;

        var isGrounded = ComputeIsGrounded();
        var attack = isGrounded ? attacks[0] : attacks[1];

        StartCoroutine(StopAttacking(attack.hitboxLength));
        StartCoroutine(StopCooldown(attack.cooldown));

        // Sweetspot timing
        var inHitbox = Physics2D.OverlapCircleAll(attack.hitbox.position, 0.25f);
        foreach (var hit in inHitbox)
        {
            if (hit.gameObject != gameObject && hit.CompareTag("Player"))
            {
                hit.gameObject.GetComponent<PlayerMovement>().TakeKnockback(8, (int)transform.localScale.x);
                isAttacking = false;
            }
        }

        // Sourspot timing
        while (isAttacking)
        {
            inHitbox = Physics2D.OverlapCircleAll(attack.hitbox.position, 0.25f);
            foreach (var hit in inHitbox)
            {
                if (hit.gameObject != gameObject && hit.CompareTag("Player"))
                {
                    hit.gameObject.GetComponent<PlayerMovement>().TakeKnockback(4, (int)transform.localScale.x);
                    isAttacking = false;
                }
            }

            if (attack.cancelOnGroundHit && ComputeIsGrounded())
            {
                isAttacking = false;
            }

            yield return null;
        }

        while (isInCooldown)
        {
            yield return null;
        }
    }

    private IEnumerator StopAttacking(float hitBoxLength)
    {
        yield return new WaitForSeconds(hitBoxLength);
        isAttacking = false;
    }

    private IEnumerator StopCooldown(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);
        isInCooldown = false;
    }

    public void TakeKnockback(int baseKnockback, int attackDirection)
    {
        isInKnockback = true;

        // If hit off the ground
        if (ComputeIsGrounded())
        {
            canJumpAgain = true;
        }

        rb.gravityScale = baseGravity;
        rb.velocity = Vector3.zero;
        acceleration = 0;
        rb.AddForce((baseKnockback * attackDirection * Vector2.right) + (baseKnockback * Vector2.up), ForceMode2D.Impulse);
    }
}
