using TMPro;
using UnityEngine;

public class PodiumSpot : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    PlayerMovement claimedPlayer = null;
    Rigidbody2D rb;
    ContactFilter2D playerContact;

    [SerializeField] int pointValue;
    [SerializeField] TextMeshProUGUI text;

    private void Start()
    {
        text.text = $"+{pointValue}";
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        playerContact = new();
        playerContact.SetLayerMask(LayerMask.GetMask("Fighter"));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Let player who just landed claim the spot
        if (collision.contacts[0].normal.y < 0 && collision.gameObject.TryGetComponent<PlayerMovement>(out var player) && player != claimedPlayer)
        {
            SetClaimedPlayer(player);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Claimed player leaces spot
        if (collision.gameObject.TryGetComponent<PlayerMovement>(out var player) && player == claimedPlayer)
        {
            // Check if there is another player on the spot
            var colliders = new Collider2D[1];
            rb.OverlapCollider(playerContact, colliders);

            if (colliders[0] == null)
            {
                return;
            }

            colliders[0].TryGetComponent<PlayerMovement>(out var playerStillOnSpot);
            
            if (playerStillOnSpot == null || playerStillOnSpot == claimedPlayer)
            {
                return;    
            }

            // Attribute spot to other player
            SetClaimedPlayer(playerStillOnSpot);
        }
    }

    public void Reset()
    {
        claimedPlayer = null;
        spriteRenderer.color = Color.white;
    }

    public void SetPointValue(int value)
    {
        pointValue = value;
        text.text = $"+{pointValue}";
    }

    private void SetClaimedPlayer(PlayerMovement player)
    {
        if (claimedPlayer != null)
        {
            claimedPlayer.LosePoints(pointValue);
        }

        claimedPlayer = player;
        claimedPlayer.GainPoints(pointValue);

        spriteRenderer.color = claimedPlayer.GetComponent<SpriteRenderer>().color;
    }
}
