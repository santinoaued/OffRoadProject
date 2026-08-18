using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PausePanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pausePanel;

    [Tooltip("First button to select when the pause panel opens (for gamepad navigation)")]
    [SerializeField] private GameObject firstSelectedButton;

    [Header("Options Panel")]
    [SerializeField] private GameObject optionsPanel;

    [Tooltip("First selectable element inside the options panel (for gamepad navigation)")]
    [SerializeField] private GameObject firstSelectedOptionsElement;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Input")]
    [Tooltip("Button action: Escape (keyboard) / Start (gamepad)")]
    [SerializeField] private InputActionReference pauseAction;

    private bool pause = false;
    private bool optionsOpen = false;

    private void OnEnable()
    {
        pauseAction.action.Enable();
        pauseAction.action.performed += OnPausePressed;
    }

    private void OnDisable()
    {
        pauseAction.action.performed -= OnPausePressed;
        pauseAction.action.Disable();
    }

    private void OnPausePressed(InputAction.CallbackContext context)
    {
        if (optionsOpen)
        {
            CloseOptions();
            return;
        }

        TogglePause();
    }

    public void TogglePause()
    {
        if (GameManager.Instance.isMatchOver) return;
        if (optionsOpen) return;

        pause = !pause;
        pausePanel.SetActive(pause);
        Time.timeScale = pause ? 0f : 1f;
        Cursor.lockState = pause ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = pause;

        if (pause)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
        else
        {
            optionsOpen = false;
            optionsPanel.SetActive(false);

            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void Resume() => TogglePause();

    public void OpenOptions()
    {
        optionsOpen = true;

        pausePanel.SetActive(false);
        optionsPanel.SetActive(true);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelectedOptionsElement);
    }

    public void CloseOptions()
    {
        optionsOpen = false;

        optionsPanel.SetActive(false);
        pausePanel.SetActive(true);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}