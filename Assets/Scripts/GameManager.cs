using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

// this script contains the victory and defeat conditions and other details about the current game
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Timer Settings")]
    [SerializeField] private float timeLimit = 120f;

    [Header("References")]
    [SerializeField] private GameObject playerVehicle;

    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Events (scalable from the Inspector)")]
    public UnityEvent onTimeExpired;

    public UnityEvent onGoalReached;

    [Header("UI")]
    [SerializeField] private GameObject defeatPanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject victoryPanel;

    [Header("UI - Gamepad Navigation")]
    [SerializeField] private GameObject defeatFirstSelectedButton;
    [SerializeField] private GameObject victoryFirstSelectedButton;

    private float timeRemaining;
    private bool isMatchRunning;
    private VehicleHealth playerVehicleHealth;
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

        if (playerVehicle != null)
            playerVehicleHealth = playerVehicle.GetComponent<VehicleHealth>();
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
        if (isMatchOver) return;

        isMatchRunning = false;
        isMatchOver = true;

        if (defeatPanel != null)
            defeatPanel.SetActive(true);
        if (hudPanel != null)
            hudPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SelectFirstButton(defeatFirstSelectedButton);
    }

    public void StartMatch()
    {
        timeRemaining = timeLimit;
        isMatchRunning = true;
        UpdateUI();
    }

    public void ReachGoal()
    {
        if (isMatchOver) return;

        isMatchRunning = false;
        isMatchOver = true;

        if (playerVehicleHealth != null)
            playerVehicleHealth.DisableDamage();

        if (victoryPanel != null)
            victoryPanel.SetActive(true);

        if (hudPanel != null)
            hudPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SelectFirstButton(victoryFirstSelectedButton);

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

    private void SelectFirstButton(GameObject button)
    {
        if (button == null || EventSystem.current == null) return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(button);
    }

    public float TimeRemaining => timeRemaining;
    public bool IsMatchRunning => isMatchRunning;
}