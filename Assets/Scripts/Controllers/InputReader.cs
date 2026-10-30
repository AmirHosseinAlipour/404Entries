using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour
{
    public static InputReader Instance { get; private set; }

    public static event Action OnPausePressed;
    public static event Action OnToggleTasksPressed;
    public static event Action OnInteractPressed;
    public static event Action OnSubmitPressed;
    public static event Action OnSkipPressed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("InputReader");
            Instance = go.AddComponent<InputReader>();
            DontDestroyOnLoad(go);
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        CheckPause();
        CheckTasks();
        CheckInteract();
        CheckSubmit();
    }

    private void CheckPause()
    {
        bool pressed = false;
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            pressed = true;
        else if (Input.GetKeyDown(KeyCode.Escape))
            pressed = true;

        if (pressed)
        {
            OnPausePressed?.Invoke();
        }
    }

    private void CheckTasks()
    {
        bool pressed = false;
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
            pressed = true;
        else if (Input.GetKeyDown(KeyCode.Tab))
            pressed = true;

        if (pressed)
        {
            OnToggleTasksPressed?.Invoke();
        }
    }

    private void CheckInteract()
    {
        bool pressed = false;
        if (Keyboard.current != null && (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.fKey.wasPressedThisFrame))
            pressed = true;
        else if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.F))
            pressed = true;

        if (pressed)
        {
            OnInteractPressed?.Invoke();
        }
    }

    private void CheckSubmit()
    {
        bool pressed = false;
        if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
            pressed = true;
        else if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            pressed = true;

        if (pressed)
        {
            OnSubmitPressed?.Invoke();
            OnSkipPressed?.Invoke();
        }
    }

    public static bool IsAnyClickDown()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            return true;
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            return true;
        if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
            return true;
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            return true;
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == UnityEngine.TouchPhase.Began)
            return true;
        return false;
    }
}
