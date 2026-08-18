using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MainMenu : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("Panels")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject versionPanel;

    [Header("Gamepad Navigation - Main Menu")]
    [Tooltip("Botón seleccionado apenas arranca el menú (ej. PlayButton)")]
    [SerializeField] private GameObject firstSelectedMainMenuButton;

    [Header("Gamepad Navigation - Panels")]
    [Tooltip("Primer elemento a seleccionar dentro de OptionsPanel (ej. Quality Dropdown)")]
    [SerializeField] private GameObject optionsFirstSelected;
    [Tooltip("Primer elemento a seleccionar dentro de CreditsPanel (ej. su botón Close)")]
    [SerializeField] private GameObject creditsFirstSelected;
    [Tooltip("Primer elemento a seleccionar dentro de VersionPanel (ej. su botón Close)")]
    [SerializeField] private GameObject versionFirstSelected;

    [Header("Input")]
    [Tooltip("Acción de Cancelar: Escape (teclado) / East button - B,Circle (gamepad)")]
    [SerializeField] private InputActionReference cancelAction;

    private GameObject[] AllPanels => new[] { optionsPanel, creditsPanel, versionPanel };
    private GameObject lastSelectedBeforePanel;

    private void OnEnable()
    {
        if (cancelAction != null)
        {
            cancelAction.action.Enable();
            cancelAction.action.performed += OnCancelPressed;
        }
    }

    private void OnDisable()
    {
        if (cancelAction != null)
        {
            cancelAction.action.performed -= OnCancelPressed;
            cancelAction.action.Disable();
        }
    }

    private void Start()
    {
        SelectObject(firstSelectedMainMenuButton);
    }

    private void OnCancelPressed(InputAction.CallbackContext context)
    {
        if (IsAnyPanelOpen())
        {
            ClosePanel();
        }
    }

    private bool IsAnyPanelOpen()
    {
        foreach (var panel in AllPanels)
        {
            if (panel != null && panel.activeSelf) return true;
        }
        return false;
    }

    public void Play()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenOptions()
    {
        OpenPanel(optionsPanel, optionsFirstSelected);
    }

    public void OpenCredits()
    {
        OpenPanel(creditsPanel, creditsFirstSelected);
    }

    public void OpenVersion()
    {
        OpenPanel(versionPanel, versionFirstSelected);
    }

    public void ClosePanel()
    {
        foreach (var panel in AllPanels)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        GameObject target = lastSelectedBeforePanel != null ? lastSelectedBeforePanel : firstSelectedMainMenuButton;
        SelectObject(target);
    }

    private void OpenPanel(GameObject panelToOpen, GameObject firstSelectedInPanel)
    {
        lastSelectedBeforePanel = EventSystem.current.currentSelectedGameObject;

        foreach (var panel in AllPanels)
        {
            if (panel != null)
            {
                panel.SetActive(panel == panelToOpen);
            }
        }

        SelectObject(firstSelectedInPanel);
    }

    private void SelectObject(GameObject target)
    {
        if (EventSystem.current == null || target == null) return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(target);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}