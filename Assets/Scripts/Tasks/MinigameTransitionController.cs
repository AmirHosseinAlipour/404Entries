using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MinigameTransitionController : MonoBehaviour
{
    public static MinigameTransitionController Instance { get; private set; }

    [Header("Transition Settings")]
    public float transitionDuration = 0.25f;

    private GameObject _miniMap;
    private GameObject _joystick;
    private MusicChange _musicChange;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("MinigameTransitionController");
            Instance = go.AddComponent<MinigameTransitionController>();
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

    private void FindHUDReferences()
    {
        if (_miniMap == null) _miniMap = GameObject.FindWithTag("MiniMap");
        if (_joystick == null) _joystick = GameObject.FindWithTag("JoyStick");
    }

    public void LaunchMinigame(BaseMinigame minigame, RectTransform minigamePanel = null, AudioClip minigameMusic = null, Action onLaunched = null)
    {
        StartCoroutine(LaunchRoutine(minigame, minigamePanel, minigameMusic, onLaunched));
    }

    private IEnumerator LaunchRoutine(BaseMinigame minigame, RectTransform minigamePanel, AudioClip minigameMusic, Action onLaunched)
    {
        FindHUDReferences();
        PlayerUIModeHelper.PlayerEnterUIMode();

        if (_miniMap != null) _miniMap.SetActive(false);
        if (_joystick != null) UIUtils.SetAlpha(_joystick, 0f);

        if (minigameMusic != null)
        {
            AudioManager.Instance?.PlayBGM(minigameMusic, transitionDuration);
        }

        if (minigamePanel != null)
        {
            UIAnimationManager.Instance.ShowWindow(minigamePanel, transitionDuration);
        }

        yield return new WaitForSecondsRealtime(transitionDuration);

        if (minigame != null)
        {
            minigame.StartMinigame();
        }

        onLaunched?.Invoke();
    }

    public void EndMinigame(BaseMinigame minigame, bool won, RectTransform minigamePanel = null, Action onEnded = null)
    {
        StartCoroutine(EndRoutine(minigame, won, minigamePanel, onEnded));
    }

    private IEnumerator EndRoutine(BaseMinigame minigame, bool won, RectTransform minigamePanel, Action onEnded)
    {
        FindHUDReferences();

        if (minigamePanel != null)
        {
            UIAnimationManager.Instance.HideWindow(minigamePanel, transitionDuration);
        }

        yield return new WaitForSecondsRealtime(transitionDuration);

        if (_miniMap != null) _miniMap.SetActive(true);
        if (_joystick != null && PlatformHelper.IsMobile()) UIUtils.SetAlpha(_joystick, 1f);

        PlayerUIModeHelper.PlayerExitUIMode();

        if (minigame != null)
        {
            if (won)
            {
                minigame.CompleteMinigame();
            }
            else
            {
                minigame.FailMinigame();
            }
        }

        onEnded?.Invoke();
    }
}
