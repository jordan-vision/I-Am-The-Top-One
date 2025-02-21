using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    [SerializeField] GameObject endScreen;
    [SerializeField] Image player1, player2;
    [SerializeField] TextMeshProUGUI p1Score, p2Score;

    public void ShowEndScreen(PlayerMovement p1, PlayerMovement p2)
    {
        endScreen.SetActive(true);

        player1.color = p1.GetComponent<SpriteRenderer>().color;
        player2.color = p2.GetComponent<SpriteRenderer>().color;

        p1Score.color = p1.GetComponent<SpriteRenderer>().color;
        p2Score.color = p2.GetComponent<SpriteRenderer>().color;

        p1Score.text = p1.GameScore.ToString();
        p2Score.text = p2.GameScore.ToString();
    }
}
