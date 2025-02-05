using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    PlayerController controller;
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    Vector3 spawnPoint, spawnScale;
    float baseGravity;
    bool canJumpAgain = false, isAttacking = false, isInCooldown = false, isInKnockback = false;
    int moveDirection, acceleration, hitsTaken = 0, score;

    [SerializeField] int runspeed, jumpForce, baseAcceleration;
    [SerializeField] float lowJumpModifier, fallModifier;
    [SerializeField] Transform groundCheck1, groundCheck2;
    [SerializeField] Attack[] attacks;
    [SerializeField] Image pointsImage;
    [SerializeField] TextMeshProUGUI pointsText;

    public int Score 
    {
        get { return score; }
        private set 
        {
            score = value;
            UpdateScoreText();
        }
    }

    private void Start()
    {
        controller = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        baseGravity = rb.gravityScale;
        acceleration = baseAcceleration;

        pointsImage.color = spriteRenderer.color;
        pointsText.color = spriteRenderer.color;

        spawnPoint = transform.position;
        spawnScale = transform.localScale;
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
                hit.gameObject.GetComponent<PlayerMovement>().TakeKnockback(attack.horizontalKnockback * 2, attack.verticalKnockback * 2, (int)transform.localScale.x);
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
                    hit.gameObject.GetComponent<PlayerMovement>().TakeKnockback(attack.horizontalKnockback, attack.verticalKnockback, (int)transform.localScale.x);
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

    public void TakeKnockback(int horizontalKnockback, int verticalKnockback, int attackDirection)
    {
        // If hit off the ground
        if (ComputeIsGrounded())
        {
            canJumpAgain = true;
        }

        rb.gravityScale = baseGravity;
        rb.velocity = Vector3.zero;
        acceleration = 0;
        rb.AddForce(((horizontalKnockback + hitsTaken) * attackDirection * Vector2.right) 
            + ((verticalKnockback + ((verticalKnockback > 0) ? hitsTaken : -hitsTaken)) * Vector2.up),
            ForceMode2D.Impulse);

        if (verticalKnockback > 0)
        {
            StartCoroutine(KnockbackTillFalling());
        } 
        else
        {
            StartCoroutine(KnockbackTillLanded());
        }

        hitsTaken++;
    }

    private IEnumerator KnockbackTillFalling()
    {
        isInKnockback = true;
        while (rb.velocity.y > 0)
        {
            yield return null;
        }

        isInKnockback = false;
        acceleration = baseAcceleration;
    }

    private IEnumerator KnockbackTillLanded()
    {
        isInKnockback = true;
        while (!ComputeIsGrounded())
        {
            yield return null;
        }

        isInKnockback = false;
        acceleration = baseAcceleration;
    }

    public void GainPoints(int value)
    {
        Score += value;
    }

    public void LosePoints(int value)
    {
        Score -= value;
    }

    private void UpdateScoreText()
    {
        pointsText.text = $"{Score} pts";
    }

    public void ResetPlayer()
    {
        Score = 0;
        hitsTaken = 0;

        transform.position = spawnPoint;
        transform.localScale = spawnScale;

        rb.velocity = Vector2.zero;
        rb.gravityScale = baseGravity;

        canJumpAgain = false;
        isAttacking = false;
        isInCooldown = false;
        isInKnockback = false;
        acceleration = baseAcceleration;
    }
}
