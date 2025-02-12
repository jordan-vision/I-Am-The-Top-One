using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    float roundTimer = 15;
    bool isRoundEnding = false;
    Dictionary<(int, int), int> pointTable;
    int roundNumber = 1;

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

        // Setting points table
        pointTable = new Dictionary<(int, int), int>();

        // 3 spots
        pointTable[(3, 1)] = 7;
        pointTable[(3, 2)] = 15;
        pointTable[(3, 3)] = 23;

        // 5 spots
        pointTable[(5, 1)] = 3;
        pointTable[(5, 2)] = 6;
        pointTable[(5, 3)] = 9;
        pointTable[(5, 4)] = 12;
        pointTable[(5, 5)] = 15;

        // 7 spots
        pointTable[(7, 1)] = 1;
        pointTable[(7, 2)] = 3;
        pointTable[(7, 3)] = 5;
        pointTable[(7, 4)] = 6;
        pointTable[(7, 5)] = 7;
        pointTable[(7, 6)] = 9;
        pointTable[(7, 7)] = 11;

        // 9 spots
        pointTable[(9, 1)] = 1;
        pointTable[(9, 2)] = 2;
        pointTable[(9, 3)] = 3;
        pointTable[(9, 4)] = 4;
        pointTable[(9, 5)] = 5;
        pointTable[(9, 6)] = 6;
        pointTable[(9, 7)] = 7;
        pointTable[(9, 8)] = 8;
        pointTable[(9, 9)] = 9;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        roundTimer -= Time.deltaTime;
        timerImage.fillAmount = roundTimer / 15.0f;

        if (roundTimer <= 0 && !isRoundEnding)
        {
            StartCoroutine(EndRound());
        }
    }
    
    private IEnumerator EndRound()
    {
        isRoundEnding = true;

        // Resetting player and stage
        Player1.ResetPlayer();
        Player2.ResetPlayer();
        Player1.gameObject.SetActive(false);
        Player2.gameObject.SetActive(false);

        yield return new WaitForSeconds(1.0f);

        if (roundNumber == 10)
        {
            EndGame();
        } else
        {
            NewRound();
        }
    }

    private void NewRound()
    {
        roundNumber++;
        roundTimer = 15;

        foreach (var spot in podiumSpots)
        {
            spot.Reset();
        }

        BuildStage();

        Player1.gameObject.SetActive(true);
        Player2.gameObject.SetActive(true);

        isRoundEnding = false;
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

            if (pointTable.ContainsKey((activeSpots, heightList[i])))
            {
                podiumSpots[i].SetPointValue(pointTable[(activeSpots, heightList[i])]);
            }

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

    public PlayerMovement OtherPlayer(PlayerMovement caller)
    {
        return (caller == Player1) ? Player2 : Player1;
    }

    private void EndGame()
    {
        Debug.Log($"{Player1.name}, {Player1.GameScore}");
        Debug.Log($"{Player2.name}, {Player2.GameScore}");
    }
}
