using System.Linq;
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
        var heightList = CreateHeightList(activeSpots);

        // Moving and scaling spots
        for (int i = 0; i < podiumSpots.Length; i++)
        {
            var spotTransform = podiumSpots[i].transform;
            spotTransform.localScale = new(spotWidth, spotTransform.localScale.y);

            var xPosition = leftmost + i * spotWidth;
            var yPosition = -5.25f + (float)(heightList[i] - 1) / (activeSpots - 1) * 3.0f;
            podiumSpots[i].SetPointValue(heightList[i]);
            spotTransform.parent.position = new(xPosition, yPosition);
        }
    }

    private int[] CreateHeightList(int activeSpots)
    {

        var strategy = Random.Range(1, 4);
        var heightList = new int[podiumSpots.Length]; 

        switch (strategy)
        {
            // Regular podium
            case 1:
                var endOfFirstHalf = (activeSpots - 1) / 2;
                var currentHeight = 1;

                for (var i = 0; i < heightList.Length; i++)
                {
                    heightList[i] = currentHeight;

                    if (i <= endOfFirstHalf)
                    {
                        if (i != endOfFirstHalf)
                        {
                            currentHeight += 2;
                        }
                        else
                        {
                            currentHeight -= 1;
                        }
                    }
                    else
                    {
                        currentHeight -= 2;
                    }
                }

                break;
            
            // Staircase
            case 2:
                var coinFlip = Random.Range(0.0f, 1.0f) < 0.5f;
                currentHeight = coinFlip ? 1 : activeSpots;

                for (var i = 0; i < heightList.Length; i++)
                {
                    heightList[i] = currentHeight;

                    currentHeight = coinFlip ? currentHeight + 1 : currentHeight - 1;
                }

                break;

            // Random
            case 3:
                var randomHeights = new int[activeSpots];
                for (var i = 0; i < activeSpots; i++)
                {
                    randomHeights[i] = i + 1;
                }

                randomHeights = randomHeights.OrderBy(i => Random.value).ToArray();

                for (var i = 0; i < randomHeights.Length; i++)
                {
                    heightList[i] = randomHeights[i];
                }

                break;
        }

        return heightList;
    }
}
