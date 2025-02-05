using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    float roundTimer = 15;

    [SerializeField] PodiumSpot[] podiumSpots;
    [SerializeField] Image timerImage;

    public static GameManager Instance;
    public PlayerMovement Player1, Player2;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        roundTimer -= Time.deltaTime;
        timerImage.fillAmount = roundTimer / 15.0f;

        if (roundTimer <= 0)
        {
            NewRound();
        }
    }
    
    private void NewRound()
    {
        // Determining winner
        roundTimer = 15;

        if (Player1.Score > Player2.Score)
        {
            Debug.Log("Player 1 wins");
        }
        else if (Player1.Score < Player2.Score)
        {
            Debug.Log("Player 2 wins");
        }
        else
        {
            Debug.Log("Tie");
        }

        // Resetting player and stage
        Player1.ResetPlayer();
        Player2.ResetPlayer();

        foreach (var spot in podiumSpots)
        {
            spot.Reset();
        }

        BuildStage();
    }

    private void BuildStage()
    {
        var activeSpots = 1 + 2 * Random.Range(1, 5);
        var spotWidth = 16.0f / activeSpots;
        var leftmost = -8 + spotWidth / 2;
        var endOfFirstHalf = (activeSpots - 1) / 2;
        var pointValue = 1;

        for (int i = 0; i < podiumSpots.Length; i++)
        {
            var spotTransform = podiumSpots[i].transform;
            spotTransform.localScale = new(spotWidth, spotTransform.localScale.y);

            var xPosition = leftmost + i * spotWidth;
            var yPosition = -5.25f + (float)(pointValue - 1) / (activeSpots - 1) * 3.0f;
            podiumSpots[i].SetPointValue(pointValue);

            if (i <= endOfFirstHalf)
            {
                if (i != endOfFirstHalf)
                {
                    pointValue += 2;
                }
                else
                {
                    pointValue -= 1;
                }
            } else
            {
                pointValue -= 2;
            }

            spotTransform.parent.position = new(xPosition, yPosition);
        }
    }
}
