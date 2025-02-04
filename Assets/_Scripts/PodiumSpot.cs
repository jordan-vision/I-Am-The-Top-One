using TMPro;
using UnityEngine;

public class PodiumSpot : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    PlayerMovement claimedPlayer = null;

    [SerializeField] int pointValue;
    [SerializeField] TextMeshProUGUI text;

    private void Start()
    {
        text.text = $"+{pointValue}";
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contacts[0].normal.y < 0 && collision.gameObject.TryGetComponent<PlayerMovement>(out var player) && player != claimedPlayer)
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
}
