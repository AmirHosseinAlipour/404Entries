using UnityEngine;
using UnityEngine.SceneManagement;

public static class PlatformHelper
{
    public static bool forceShowMobileControls = true;

    private static GameObject _cachedJoystickCanvas;

    public static bool IsMobile()
    {
        if (!forceShowMobileControls) return false;

#if UNITY_EDITOR
        return forceShowMobileControls;
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

    public static GameObject GetJoystickCanvas()
    {
        if (_cachedJoystickCanvas != null) return _cachedJoystickCanvas;

        GameObject go = GameObject.Find("JoyStick_Canvas");
        if (go != null)
        {
            _cachedJoystickCanvas = go;
            return _cachedJoystickCanvas;
        }

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

    public static void SetForceShowJoystick(bool force)
    {
        forceShowMobileControls = force;
        ApplyJoystickVisibility();
    }
}
