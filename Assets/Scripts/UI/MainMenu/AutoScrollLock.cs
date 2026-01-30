using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Add this component to your ScrollView GameObject
[RequireComponent(typeof(ScrollRect))]
public class AutoScrollLock : MonoBehaviour
{
    [Header("Auto Scroll Settings")]
    [Tooltip("The time to wait (in seconds) before starting to scroll.")]
    public float initialWaitTime = 3.0f;

    [Tooltip("The speed of the scroll. A value of 0.1 means it takes 10 seconds to scroll the full height.")]
    public float scrollSpeed = 0.1f;

    [Header("Interaction Settings")]
    [Tooltip("When true, the player cannot manually scroll the content.")]
    public bool lockPlayerInteraction = true; 
    
    // We keep the ScrollRect public to allow the DragHandler (below) to access it if needed,
    // but the main script only uses the private reference.
    private ScrollRect scrollRect;
    private Coroutine scrollingCoroutine;
    private bool isAutoScrolling = false;
    
    // --- Lifecycle Methods ---

    void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        // Ensure that the horizontal scroll is disabled if we only want vertical auto-scroll
        scrollRect.horizontal = false; 
    }

    void OnEnable()
    {
        StartScrolling();
    }

    void OnDisable()
    {
        StopScrolling();
    }
    
    // --- Scrolling Control ---

    /// <summary>
    /// Stops any active scrolling coroutine and optionally unlocks player interaction.
    /// </summary>
    private void StopScrolling()
    {
        isAutoScrolling = false;
        if (scrollingCoroutine != null)
        {
            StopCoroutine(scrollingCoroutine);
            scrollingCoroutine = null;
        }

        // IMPORTANT: Re-enable the ScrollRect interaction if it was locked.
        if (lockPlayerInteraction && scrollRect != null)
        {
            scrollRect.enabled = true;
        }
    }

    /// <summary>
    /// Starts the entire reset, wait, and scroll process.
    /// </summary>
    private void StartScrolling()
    {
        StopScrolling();
        
        // Lock player interaction immediately before starting to scroll
        if (lockPlayerInteraction && scrollRect != null)
        {
            // Temporarily disable the ScrollRect component itself.
            // This prevents all input (drag, mouse wheel, etc.) from affecting it.
            scrollRect.enabled = false;
        }
        
        scrollingCoroutine = StartCoroutine(ResetAndAutoScroll());
    }

    // --- Coroutine for Auto-Scrolling ---

    private IEnumerator ResetAndAutoScroll()
    {
        // 1. Wait for layout and reset to top
        yield return new WaitForEndOfFrame();
        yield return null; 

        if (scrollRect != null)
        {
            // Set position to the very top (1 = top, 0 = bottom)
            scrollRect.verticalNormalizedPosition = 1f; 
        }

        // 2. Initial Wait
        yield return new WaitForSeconds(initialWaitTime);

        // 3. Auto Scroll
        isAutoScrolling = true;
        float currentPosition = scrollRect.verticalNormalizedPosition;

        // Scroll as long as we are auto-scrolling and haven't hit the bottom
        while (isAutoScrolling && currentPosition > 0f)
        {
            // Calculate movement based on speed and frame time
            currentPosition -= scrollSpeed * Time.deltaTime;
            
            // Apply new position, ensuring it doesn't go below 0
            scrollRect.verticalNormalizedPosition = Mathf.Max(currentPosition, 0f);
            
            yield return null; 
        }

        // 4. Cleanup
        StopScrolling();
    }
}