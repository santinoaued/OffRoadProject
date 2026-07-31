using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
public enum InputDeviceType
{
    KeyboardMouse,
    Gamepad
}

[RequireComponent(typeof(PlayerInput))]
public class InputDeviceManager : MonoBehaviour
{
    public static InputDeviceManager Instance { get; private set; }

    [Header("events")]
    [Tooltip("fired whenever the active input device changes")]
    public UnityEvent<InputDeviceType> onDeviceChanged;

    public InputDeviceType CurrentDevice { get; private set; } = InputDeviceType.KeyboardMouse;

    private PlayerInput playerInput;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        playerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        playerInput.onControlsChanged += HandleControlsChanged;
    }

    private void OnDisable()
    {
        playerInput.onControlsChanged -= HandleControlsChanged;
    }

    private void HandleControlsChanged(PlayerInput input)
    {
        InputDeviceType newDevice = input.currentControlScheme == "Gamepad"
            ? InputDeviceType.Gamepad
            : InputDeviceType.KeyboardMouse;

        if (newDevice == CurrentDevice) return;

        CurrentDevice = newDevice;
        onDeviceChanged?.Invoke(CurrentDevice);
    }
}
