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
    Vector2 storedVelocity;
    float baseGravity;
    bool canJumpAgain = false, isAttacking = false, isInCooldown = false, isInKnockback = false, frozen = false;
    int moveDirection, acceleration, hitsTaken = 0, roundScore, gameScore;

    [SerializeField] int runspeed, jumpForce, doubleJumpForce, baseAcceleration;
    [SerializeField] float lowJumpModifier, fallModifier;
    [SerializeField] Transform groundCheck1, groundCheck2;
    [SerializeField] Attack[] attacks;
    [SerializeField] Image pointsImage;
    [SerializeField] TextMeshProUGUI pointsText;

    public PlayerController Controller => controller;

    public int RoundScore 
    {
        get { return roundScore; }
        private set 
        {
            roundScore = value;
            UpdateScoreText();
        }
    }

    public int GameScore => gameScore;

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

        controller.Setup();

    }

    private void Update()
    {
        if (frozen)
        {
            return;
        }

        ManageVertical();
        Move();
        CheckForFlip();
    }

    private void Move()
    {
        // Computing move direction
        if (controller.GetLeft())
        {
            moveDirection = -1;
        }
        else if (controller.GetRight())
        {
            moveDirection = 1;
        }
        else
        {
            moveDirection = 0;
        }

        // Moving character
        rb.velocity = Mathf.MoveTowards(rb.velocity.x, moveDirection * runspeed, acceleration * Time.deltaTime) * Vector2.right
            + rb.velocity.y * Vector2.up;
    }

    private void ManageVertical()
    {
        // On landing, reset canJumAgain
        var isGrounded = ComputeIsGrounded();
        if (canJumpAgain && isGrounded && !isInKnockback)
        {
            canJumpAgain = false;
        }

        // Double jump
        if (canJumpAgain && controller.GetJumpDown() && !isInKnockback)
        {
            Jump(doubleJumpForce);
            canJumpAgain = false;
        }
        // Jump
        else if (isGrounded && controller.GetJump())
        {
            Jump(jumpForce);
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

    public void CheckForFlip()
    {
        var faceLeft = GameManager.Instance.OtherPlayer(this).transform.position.x < transform.position.x;
        transform.localScale = new(faceLeft ? -1 : 1, 1, 1);
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

    private void Jump(int verticalForce)
    {
        rb.gravityScale = baseGravity;
        rb.velocity = new(rb.velocity.x, 0);
        rb.AddForce(new(0, verticalForce), ForceMode2D.Impulse);
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
                hit.gameObject.GetComponent<PlayerMovement>().TakeKnockback(attack.horizontalKnockback * 2, attack.verticalKnockback * 2, (int)transform.localScale.x, true);
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
                    hit.gameObject.GetComponent<PlayerMovement>().TakeKnockback(attack.horizontalKnockback, attack.verticalKnockback, (int)transform.localScale.x, false);
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

    public void TakeKnockback(int horizontalKnockback, int verticalKnockback, int attackDirection, bool freeze)
    {
        if (isInKnockback)
        {
            return;
        }

        // If hit off the ground
        if (ComputeIsGrounded())
        {
            canJumpAgain = true;
        }

        rb.gravityScale = baseGravity;
        rb.velocity = Vector3.zero;
        acceleration = 0;
        var force = ((horizontalKnockback + hitsTaken) * attackDirection * Vector2.right)
            + ((verticalKnockback + ((verticalKnockback > 0) ? hitsTaken : -hitsTaken)) * Vector2.up);
        rb.AddForce(force, ForceMode2D.Impulse);

        if (verticalKnockback > 0)
        {
            StartCoroutine(KnockbackTillFalling());
        }
        else
        {
            StartCoroutine(KnockbackTillLanded());
        }

        hitsTaken++;

        if (freeze && !frozen)
        {
            StartCoroutine(GameManager.Instance.FreezePlayers(Mathf.Min(force.sqrMagnitude / 2000, 0.5f)));
        }
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
        RoundScore += value;
    }

    public void LosePoints(int value)
    {
        RoundScore -= value;
    }

    private void UpdateScoreText()
    {
        pointsText.text = $"{RoundScore} pts";
    }

    public void ResetPlayer()
    {
        if (frozen)
        {
            Unfreeze();
        }

        gameScore += RoundScore;
        RoundScore = 0;

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

    public void Freeze()
    {
        storedVelocity = rb.velocity;
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        frozen = true;
    }

    public void Unfreeze()
    {
        rb.velocity = storedVelocity;
        rb.isKinematic = false;
        frozen = false;
    }
}
