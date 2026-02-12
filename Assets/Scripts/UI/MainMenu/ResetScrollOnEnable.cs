using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required for drag events

// Add this component to your ScrollView GameObject
[RequireComponent(typeof(ScrollRect))]
public class ResetScrollOnEnable : MonoBehaviour, IBeginDragHandler
{
    [Header("Auto Scroll Settings")]
    [Tooltip("The time to wait (in seconds) before starting to scroll.")]
    public float initialWaitTime = 3.0f;

    [Tooltip("The speed of the scroll. A value of 0.1 means it takes 10 seconds to scroll the full height.")]
    public float scrollSpeed = 0.1f;

    private ScrollRect scrollRect;
    private Coroutine scrollingCoroutine;
    private bool isAutoScrolling = false;

    void Awake()
    {
        // Get the ScrollRect component
        scrollRect = GetComponent<ScrollRect>();
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
    }
}