using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameplayTracker : MonoBehaviour
{
    [Header("Gameplay Tracker")]
    [Header("Gameplay values")]
    public float currentScore;//current score
    [SerializeField] private GameObject startPoint;//start point of the level
    [SerializeField] private GameObject endPoint;//end point of the level
    [SerializeField] private GameObject player;//player object
    [SerializeField] private TextMeshProUGUI scoreText;//score text
    [SerializeField] private TextMeshProUGUI bubbleText;//bubble text
    [SerializeField] private GameObject[] bubbleBuddy;//bubble buddy prefab
    public int bubblesSaved;//bubbles saved
    [Header("Obstacle values")]
    [SerializeField] private float obstacleSpeed;//speed of the obstacles
    [SerializeField] private Obstacle[] obstacles;//obstacles
    [Header("Checkpoint values")]
    [SerializeField] private int checkpointCount;//number of checkpoints
    [Header("Class calls")]
    [SerializeField] private GameManage gameMange;//game manager
    [SerializeField] private LevelManager levelManager;//level manager
    private float totalDistance;
    private float pointsPerSecond = 0.1f; // Points added per second
    private float waveFrequency = 2f; // Frequency of the wave effect
    private float waveAmplitude = 5f; // Amplitude of the wave effect
    private float rainbowSpeed = 2f; // Speed of the rainbow effect
    private int previousBubbleBuddyCount; // Previous bubble buddy count
    private Coroutine gameOverCoroutine;
    private Coroutine delayedBubbleBuddyCheckCoroutine;
    
    private void Start()
    {
        gameMange = FindObjectOfType<GameManage>();
        FindCheckpoints();
        if(startPoint == null)
        {
            startPoint = GameObject.FindGameObjectWithTag("StartPoint");
        }
        if(endPoint == null)
        {
            endPoint = GameObject.FindGameObjectWithTag("WinTrig");
        }
        if (startPoint != null && endPoint != null)
        {
            totalDistance = Vector3.Distance(startPoint.transform.position, endPoint.transform.position);
        }
        previousBubbleBuddyCount = bubbleBuddy.Length;
        StartCoroutine(DelayedBubbleBuddyCheck());
    }

    private void Update()
    {
        FindBubbleBuddies();
        if(gameMange.isPaused == false && levelManager.scenename == "Game")
        {
            AddScorePerSecond();   
        }
        UpdateScoreText();
    }
    /// <summary>
    /// Track the player distance
    /// </summary>
    public void TrackPlayerDistance()
    {
        if (player != null && startPoint != null)
        {
            float distance = Vector3.Distance(player.transform.position, startPoint.transform.position);

        }
    }
    /// <summary>
    /// Find the Bubble Buddies
    /// </summary>
    public void FindBubbleBuddies()
    {
        int currentBubbleBuddyCount = GameObject.FindGameObjectsWithTag("Bubble").Length;
        if (currentBubbleBuddyCount < previousBubbleBuddyCount)
        {
            gameMange.soundManager.PlaySFX("BubblePop4");
        }
        if (currentBubbleBuddyCount == 0 && gameOverCoroutine == null && levelManager.scenename == "Game")
        {
            gameOverCoroutine = StartCoroutine(GameOverDelay());
        }
        else if (currentBubbleBuddyCount > 0 && gameOverCoroutine != null)
        {
            StopCoroutine(gameOverCoroutine);
            gameOverCoroutine = null;
        }
        previousBubbleBuddyCount = currentBubbleBuddyCount;
        bubbleBuddy = GameObject.FindGameObjectsWithTag("Bubble");
      
        
    }

    /// <summary>
    /// Add score
    /// </summary>
    // public void AddScore(float score)
    // {
    //     currentScore += score;
    //     UpdateScoreText();
    // }
    /// <summary>
    /// Update the score text with wave and rainbow effect
    /// </summary>
    private void UpdateScoreText()
    {
        string scoreString = "Score: " + currentScore.ToString("F2");
        if (bubbleBuddy.Length > 0)
        {
            bubbleText.text = " Bubbles: " + bubbleBuddy.Length;
            bubbleText.text = ApplyWaveAndRainbowEffect(bubbleText.text);
        }
        else 
        {
            bubbleText.text = "";
        }

        
        
        scoreText.text = scoreString;
    }

    /// <summary>
    /// Apply wave and rainbow effect to the text
    /// </summary>
    private string ApplyWaveAndRainbowEffect(string text)
    {
        string result = "";
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            float wave = Mathf.Sin(Time.time * waveFrequency + i * 0.5f) * waveAmplitude;
            Color color = Color.HSVToRGB((Time.time * rainbowSpeed + i * 0.1f) % 1f, 1f, 1f);
            string colorHex = ColorUtility.ToHtmlStringRGB(color);
            result += $"<color=#{colorHex}><size={wave + 50}>{c}</size></color>";
        }
        return result;
    }

    /// <summary>
    /// Add score per second
    /// </summary>
    private void AddScorePerSecond()
    {
        if (bubbleBuddy.Length > 0) // Check if there are any bubble buddies left
        {
            currentScore += pointsPerSecond;
        }
        UpdateScoreText();
    }
    /// <summary>
    /// Find the Checkpoints
    /// </summary>
    public void FindCheckpoints()
    {
        GameObject[] checkpoints = GameObject.FindGameObjectsWithTag("Checkpoint");
        checkpointCount = checkpoints.Length;
    }
    /// <summary>
    /// Update the checkpoint count and add bonus score
    /// </summary>
    public void CheckpointReached()
    {
        FindObstacles();
    }
    /// <summary>
    /// Bubble Tracker for final score
    /// </summary>
    public void BubbleTracker()
    {
        foreach (GameObject bubble in bubbleBuddy)
        {
            if (bubble != null)
            {
                bubblesSaved++;
                gameMange.soundManager.PlaySFX("Select");
            }
        }
    }
    /// <summary>
    /// Find all the obstacles in the scene
    /// </summary>
    public void FindObstacles()
    {
        obstacles = FindObjectsOfType<Obstacle>();
        foreach (Obstacle obstacle in obstacles)
        {
            if (obstacle != null)
            {  
                obstacle.speed += obstacleSpeed;
            }
        }
    }
    
    private IEnumerator GameOverDelay()
    {
        yield return new WaitForSeconds(2f); // Wait for 2 seconds
        if (GameObject.FindGameObjectsWithTag("Bubble").Length == 0)
        {
            ShowFinalScoreAndRestart();
        }
    }
    
    private IEnumerator DelayedBubbleBuddyCheck()
    {
        yield return new WaitForSeconds(5f); // Wait for 5 seconds before starting the check
        while (true)
        {
            FindBubbleBuddies();
            yield return new WaitForSeconds(1f); // Check every 1 second
        }
    }
    
    
    public void ShowFinalScoreAndRestart()
    {
        // Stop the delayed bubble buddy check coroutine
        if (delayedBubbleBuddyCheckCoroutine != null)
        {
            StopCoroutine(delayedBubbleBuddyCheckCoroutine);
            delayedBubbleBuddyCheckCoroutine = null;
        }

        // Show the final score on the canvas
        gameMange.uiManager.SetUI("GameOver");
        Debug.Log("Game Over");
        TextMeshProUGUI finalScoreText = GameObject.Find("FinalScoreText").GetComponent<TextMeshProUGUI>();
        finalScoreText.text = "Final Score: " + currentScore.ToString("F2");

        // Restart the application after a delay
        StartCoroutine(RestartApplication());
    }
    
    private IEnumerator RestartApplication()
    {
        yield return new WaitForSeconds(3f); // Wait for 3 seconds
        System.Diagnostics.Process.Start(Application.dataPath.Replace("_Data", ".exe")); // Start a new instance of the application
        Application.Quit();
    }
}
