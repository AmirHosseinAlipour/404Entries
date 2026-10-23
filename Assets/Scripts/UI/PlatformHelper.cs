using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Helper utility to detect target runtime platform and automatically handle
/// platform-specific UI elements such as the mobile virtual joystick on PC/Desktop.
/// </summary>
public static class PlatformHelper
{
    /// <summary>
    /// When true, forces the mobile UI (e.g. onscreen joystick) to show even on PC / Editor.
    /// Useful for testing mobile controls with mouse inside the Unity Editor.
    /// </summary>
    public static bool forceShowMobileControls = false;

    private static GameObject _cachedJoystickCanvas;

    /// <summary>
    /// Returns true if the game is running on a mobile / handheld platform or if forceShowMobileControls is active.
    /// </summary>
    public static bool IsMobile()
    {
        if (forceShowMobileControls) return true;

#if UNITY_EDITOR
        return false;
#elif UNITY_ANDROID || UNITY_IOS
        return true;
#else
        return Application.isMobilePlatform || SystemInfo.deviceType == DeviceType.Handheld;
#endif
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InitPlatformUI()
    {
        _cachedJoystickCanvas = null;
        ApplyJoystickVisibility();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _cachedJoystickCanvas = null;
        ApplyJoystickVisibility();
    }

    /// <summary>
    /// Finds the JoyStick_Canvas in the scene even if it is currently inactive.
    /// </summary>
    public static GameObject GetJoystickCanvas()
    {
        if (_cachedJoystickCanvas != null) return _cachedJoystickCanvas;

        // Search active objects first
        GameObject go = GameObject.Find("JoyStick_Canvas");
        if (go != null)
        {
            _cachedJoystickCanvas = go;
            return _cachedJoystickCanvas;
        }

        // Search all Canvases including inactive ones
        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var c in canvases)
        {
            if (c != null && c.gameObject != null && c.gameObject.name.Contains("JoyStick"))
            {
                _cachedJoystickCanvas = c.gameObject;
                return _cachedJoystickCanvas;
            }
        }

        return null;
    }

    /// <summary>
    /// Sets visibility of the mobile joystick canvas based on platform and forceShowMobileControls.
    /// </summary>
    public static void ApplyJoystickVisibility()
    {
        bool show = IsMobile();
        GameObject joystickCanvas = GetJoystickCanvas();
        if (joystickCanvas != null)
        {
            joystickCanvas.SetActive(show);
        }

        GameObject joystickObj = GameObject.FindWithTag("JoyStick");
        if (joystickObj != null)
        {
            UIUtils.SetAlpha(joystickObj, show ? 1f : 0f);
        }
    }

    /// <summary>
    /// Dynamically forces or un-forces the mobile joystick on screen.
    /// </summary>
    public static void SetForceShowJoystick(bool force)
    {
        forceShowMobileControls = force;
        ApplyJoystickVisibility();
    }
}
