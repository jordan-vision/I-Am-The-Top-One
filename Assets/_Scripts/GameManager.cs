using UnityEngine;

public class GameManager : MonoBehaviour
{
    float roundTimer = 20;

    [SerializeField] private PodiumSpot[] podiumSpots;

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

        if (roundTimer <= 0)
        {
            NewRound();
        }
    }
    
    private void NewRound()
    {
        // Determining winner
        roundTimer = 20;

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

        // Resetting points and stage
        Player1.
    }
}
