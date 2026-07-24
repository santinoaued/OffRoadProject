using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Timer Settings")]
    [Tooltip("Total match time in seconds")]
    [SerializeField] private float timeLimit = 120f;

    [Header("References")]
    [Tooltip("Player's vehicle")]
    [SerializeField] private GameObject playerVehicle;

    [Tooltip("Remaining time text")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Events (scalable from the Inspector)")]
    [Tooltip("Fired when time reaches 0 without reaching the goal")]
    public UnityEvent onTimeExpired;

    [Tooltip("Fired when the player reaches the goal before time runs out")]
    public UnityEvent onGoalReached;

    [Header("Defeat UI")]
    [SerializeField] private GameObject defeatPanel;
    [SerializeField] private GameObject hudPanel;

    private float timeRemaining;
    private bool isMatchRunning;
    public bool isMatchOver { get; private set; }

    private void Awake()
    {
        // basic singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        StartMatch();
    }

    private void Update()
    {
        if (!isMatchRunning) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            UpdateUI();
            TimeExpired();
            return;
        }

        UpdateUI();
    }

    public void TriggerDefeat()
    {
        Debug.Log("GameManager: TriggerDefeat called");
        if (isMatchOver) return;

        isMatchRunning = false;
        isMatchOver = true;

        if (defeatPanel != null)
            defeatPanel.SetActive(true);
        if (hudPanel != null)
            hudPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void StartMatch()
    {
        timeRemaining = timeLimit;
        isMatchRunning = true;
        UpdateUI();
    }

    public void ReachGoal()
    {
        if (!isMatchRunning) return;

        isMatchRunning = false;
        onGoalReached?.Invoke();
    }

    private void TimeExpired()
    {
        if (!isMatchRunning) return;

        isMatchRunning = false;
        onTimeExpired?.Invoke();
    }

    public void DestroyVehicle()
    {
        if (playerVehicle != null)
        {
            Destroy(playerVehicle);
        }
    }

    private void UpdateUI()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public float TimeRemaining => timeRemaining;
    public bool IsMatchRunning => isMatchRunning;
}