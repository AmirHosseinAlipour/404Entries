using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required for drag events
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

// Add this component to your ScrollView GameObject
[RequireComponent(typeof(ScrollRect))]
public class ResetScrollOnEnable : MonoBehaviour, IBeginDragHandler
{
    [Header("Auto Scroll Settings")]
    [Tooltip("The time to wait (in seconds) before starting to scroll.")]
    public float initialWaitTime = 3.0f;

    [Tooltip("The speed of the scroll. A value of 0.1 means it takes 10 seconds to scroll the full height.")]
    public float scrollSpeed = 0.1f;

    [Header("Ending Scene Options")]
    [Tooltip("Delay in seconds after reaching the end before returning to main menu.")]
    public float waitAtEndBeforeMenu = 2.5f;

    private ScrollRect scrollRect;
    private Coroutine scrollingCoroutine;
    private bool isAutoScrolling = false;

    private bool _isEndingScene = false;
    private bool _isTransitioning = false;
    private float _timeSinceStart = 0f;
    private float _reachBottomTimer = 0f;

    void Awake()
    {
        // Get the ScrollRect component
        scrollRect = GetComponent<ScrollRect>();
    }

    void Start()
    {
        _isEndingScene = SceneManager.GetActiveScene().name == "Ending";
    }

    void OnEnable()
    {
        // When the panel is enabled, start the whole process
        StartScrolling();
    }

    void OnDisable()
    {
        // When the panel is disabled, stop everything
        StopScrolling();
    }

    void Update()
    {
        if (!_isEndingScene || _isTransitioning) return;

        _timeSinceStart += Time.deltaTime;

        // Allow skipping credits after 1.5s via Space, Escape, Enter
        if (_timeSinceStart > 1.5f)
        {
            bool skip = false;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.spaceKey.wasPressedThisFrame ||
                    Keyboard.current.escapeKey.wasPressedThisFrame ||
                    Keyboard.current.enterKey.wasPressedThisFrame)
                {
                    skip = true;
                }
            }
            if (!skip && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return)))
            {
                skip = true;
            }

            if (skip)
            {
                CompleteEndingAndReturnToMenu();
                return;
            }
        }

        // Check if scroll reached the bottom (either by auto scroll or manual drag)
        if (scrollRect != null && scrollRect.verticalNormalizedPosition <= 0.02f)
        {
            _reachBottomTimer += Time.deltaTime;
            if (_reachBottomTimer >= waitAtEndBeforeMenu)
            {
                CompleteEndingAndReturnToMenu();
            }
        }
        else
        {
            _reachBottomTimer = 0f;
        }
    }

    // This is called when the user first clicks or touches to drag
    public void OnBeginDrag(PointerEventData eventData)
    {
        // If the user drags, stop the auto-scroll
        StopScrolling();
    }

    /// <summary>
    /// Stops any active scrolling coroutine.
    /// </summary>
    private void StopScrolling()
    {
        isAutoScrolling = false;
        if (scrollingCoroutine != null)
        {
            StopCoroutine(scrollingCoroutine);
            scrollingCoroutine = null;
        }
    }

    /// <summary>
    /// Starts the entire reset, wait, and scroll process.
    /// </summary>
    private void StartScrolling()
    {
        // Stop any previous scrolling first
        StopScrolling();
        // Start the new coroutine
        scrollingCoroutine = StartCoroutine(ResetAndAutoScroll());
    }

    private IEnumerator ResetAndAutoScroll()
    {
        // --- 1. RESET TO TOP ---
        // Wait for the layout to be calculated, just like before
        yield return new WaitForEndOfFrame();
        yield return null; // Wait one more frame for safety

        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f; // 1 = top
        }

        // --- 2. INITIAL WAIT ---
        yield return new WaitForSeconds(initialWaitTime);

        // --- 3. AUTO SCROLL ---
        isAutoScrolling = true;
        float currentPosition = scrollRect.verticalNormalizedPosition;

        // Scroll as long as we are "autoScrolling" and not at the bottom
        while (isAutoScrolling && currentPosition > 0f)
        {
            // Move the position down based on speed and time
            currentPosition -= scrollSpeed * Time.deltaTime;
            
            // Apply the new position, clamping it at 0 (bottom)
            scrollRect.verticalNormalizedPosition = Mathf.Max(currentPosition, 0f);
            
            yield return null; // Wait for the next frame
        }

        // Coroutine finished
        isAutoScrolling = false;
        scrollingCoroutine = null;

        if (_isEndingScene && !_isTransitioning)
        {
            yield return new WaitForSeconds(waitAtEndBeforeMenu);
            CompleteEndingAndReturnToMenu();
        }
    }

    public void CompleteEndingAndReturnToMenu()
    {
        if (_isTransitioning) return;
        _isTransitioning = true;

        Debug.Log("[Ending] Ending sequence complete. Deleting all save data and returning to Main Menu...");

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.DeleteSaveAndRestart();
        }
        else
        {
            string savePath = System.IO.Path.Combine(Application.persistentDataPath, SaveManager.SaveFileName);
            if (System.IO.File.Exists(savePath))
            {
                try
                {
                    System.IO.File.Delete(savePath);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("[Ending] Failed to delete save file: " + ex.Message);
                }
            }

            if (SelectionPlayer.instance != null)
            {
                Destroy(SelectionPlayer.instance.gameObject);
            }
            BaseProfessors.IsAnyMinigameActive = false;
            BaseProfessors.ActiveProfessor = null;
            Time.timeScale = 1f;
            SceneManager.LoadScene(0);
        }
    }
}
