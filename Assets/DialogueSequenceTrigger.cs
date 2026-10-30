using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class DialogueSequenceTrigger : MonoBehaviour
{
    [Header("Dialogue Windows (In Order)")]
    public List<RectTransform> dialogueWindows;
    public List<FarsiTypewriter> typewriters; // optional

    [Header("Animation Durations")]
    public float showDuration = 0.5f;
    public float hideDuration = 0.3f;

    [Header("After Finish")]
    public bool destroyAfterFinish = true;

    private int currentIndex = 0;
    private bool inConversation = false;
    private Coroutine _exitRoutine;

    public static event System.Action OnAnyConversationCompleted;
    public static bool IsAnyConversationActive { get; private set; } = false;
    public static bool HasKetabkhonehDialogueCompleted = false;

    public bool HasCompleted { get; private set; } = false;
    public bool IsInConversation => inConversation;

    private void Start()
    {
        if (TaskManager.Instance != null && TaskManager.Instance.taskCompleted != null && TaskManager.Instance.taskCompleted.Length > 15 && TaskManager.Instance.taskCompleted[15])
        {
            HasKetabkhonehDialogueCompleted = true;
            Destroy(gameObject);
            return;
        }

        // First: ensure all dialogue windows are inactive initially and have proper sorting and button hooks
        if (dialogueWindows != null)
        {
            foreach (var dlg in dialogueWindows)
            {
                if (dlg != null)
                {
                    dlg.gameObject.SetActive(false);
                    EnsureDialogueSortingOrder(dlg);

                    Button[] btns = dlg.GetComponentsInChildren<Button>(true);
                    foreach (var btn in btns)
                    {
                        if (btn != null)
                        {
                            btn.onClick.RemoveListener(NextDialogue);
                            btn.onClick.AddListener(NextDialogue);
                        }
                    }
                }
            }
        }

        CheckAndFixLibraryDialogue();
    }

    public static void EnsureDialogueSortingOrder(RectTransform rt)
    {
        if (rt == null) return;
        Canvas c = rt.GetComponentInParent<Canvas>(true);
        if (c != null)
        {
            c.overrideSorting = true;
            c.sortingOrder = 50;
        }
    }

    private void CheckAndFixLibraryDialogue()
    {
        if (typewriters != null && typewriters.Count > 0 && typewriters[0] != null)
        {
            Text t = typewriters[0].GetComponent<Text>();
            if (t != null && t.text.Contains("اسقایی"))
            {
                string fixedLine = "سلام من از طرف آموزش اومدم گفتن بیام از بایگانی مدارکمو بگیرم";
                t.text = fixedLine;
                typewriters[0].SetText(fixedLine);
            }
        }
    }

    private PlayerController _player;
    private Button _allTasksBackButton;
    private Button _currentTaskButton;
    private GameObject _joyStick;

    private void FindReferences()
    {
        try
        {
            if (_player == null)
            {
                var pObj = GameObject.FindWithTag("Player");
                if (pObj != null) _player = pObj.GetComponent<PlayerController>();
            }
        }
        catch {}

        try
        {
            if (_allTasksBackButton == null)
            {
                var backBtnObj = GameObject.FindWithTag("AllTasksBackButton");
                if (backBtnObj != null) _allTasksBackButton = backBtnObj.GetComponent<Button>();
            }
        }
        catch {}

        try
        {
            if (_currentTaskButton == null)
            {
                var currBtnObj = GameObject.FindWithTag("CurrentTaskButton");
                if (currBtnObj != null) _currentTaskButton = currBtnObj.GetComponent<Button>();
            }
        }
        catch {}

        try
        {
            if (_joyStick == null)
            {
                _joyStick = GameObject.FindWithTag("JoyStick");
            }
        }
        catch {}
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        StartConversation();
    }

    public void StartConversation()
    {
        if (inConversation || HasCompleted || HasKetabkhonehDialogueCompleted) return;

        if (_exitRoutine != null)
        {
            StopCoroutine(_exitRoutine);
            _exitRoutine = null;
        }

        FindReferences();
        PlayerUIModeHelper.PlayerEnterUIMode(_player);
        if (_player != null)
        {
            _player.isUIActive = true;
            var rb = _player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }
        PlayerUIModeHelper.DisableTasksButton(_allTasksBackButton, _currentTaskButton);
        TaskUI.SetTopUIVisible(false);
        if (_joyStick != null) UIUtils.SetAlpha(_joyStick, 0f);

        inConversation = true;
        IsAnyConversationActive = true;
        currentIndex = 0;
        ShowCurrent();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (!inConversation) return;

        if (_exitRoutine != null) StopCoroutine(_exitRoutine);
        _exitRoutine = StartCoroutine(ExitAndResetRoutine());
    }

    private float _lastActionTime = -1f;

    private void Update()
    {
        if (!inConversation) return;

        if (InputReader.IsAnyClickDown())
        {
            NextDialogue();
        }
    }

    public void NextDialogue()
    {
        if (!inConversation) return;

        // Prevent double trigger on the same frame (e.g. from UI button + InputReader)
        if (Time.time - _lastActionTime < 0.15f) return;

        // If current typewriter is still typing, skip typing first on click!
        if (typewriters != null && currentIndex >= 0 && currentIndex < typewriters.Count && typewriters[currentIndex] != null)
        {
            if (typewriters[currentIndex].isTyping)
            {
                typewriters[currentIndex].SkipTyping();
                _lastActionTime = Time.time;
                return;
            }
        }

        _lastActionTime = Time.time;
        HideCurrentAnimated();
        currentIndex++;

        if (currentIndex >= dialogueWindows.Count)
        {
            EndConversation();
            return;
        }

        ShowCurrent();
    }

    // ---------- Show / Hide ----------
    private void ShowCurrent()
    {
        if (currentIndex < 0 || currentIndex >= dialogueWindows.Count) return;

        var window = dialogueWindows[currentIndex];
        if (window == null) return;

        EnsureDialogueSortingOrder(window);

        if (typewriters != null &&
            currentIndex < typewriters.Count &&
            typewriters[currentIndex] != null)
        {
            UIAnimationManager.Instance.ShowDialogueWindow(window, showDuration, typewriters[currentIndex]);
        }
        else
        {
            UIAnimationManager.Instance.ShowWindow(window, showDuration);
        }
    }

    private void HideCurrentAnimated()
    {
        if (currentIndex < 0 || currentIndex >= dialogueWindows.Count) return;

        var window = dialogueWindows[currentIndex];
        if (window == null) return;

        UIAnimationManager.Instance.HideWindow(window, hideDuration);
    }

    private void HideAllAnimated()
    {
        if (dialogueWindows == null) return;
        foreach (var dlg in dialogueWindows)
        {
            if (dlg != null && dlg.gameObject.activeSelf)
                UIAnimationManager.Instance.HideWindow(dlg, hideDuration);
        }
    }

    // ---------- Exit / Reset ----------
    private IEnumerator ExitAndResetRoutine()
    {
        if (!inConversation) yield break;

        HideAllAnimated();
        yield return new WaitForSeconds(hideDuration);

        if (dialogueWindows != null)
        {
            foreach (var dlg in dialogueWindows)
            {
                if (dlg != null)
                    dlg.gameObject.SetActive(false);
            }
        }

        FindReferences();
        PlayerUIModeHelper.PlayerExitUIMode(_player);
        PlayerUIModeHelper.EnableTasksButton(_allTasksBackButton, _currentTaskButton);
        TaskUI.SetTopUIVisible(true);
        if (_joyStick != null) UIUtils.SetAlpha(_joyStick, 1f);

        currentIndex = 0;
        inConversation = false;
        IsAnyConversationActive = false;
        _exitRoutine = null;
    }

    private void EndConversation()
    {
        HideAllAnimated();
        inConversation = false;
        IsAnyConversationActive = false;
        HasCompleted = true;
        HasKetabkhonehDialogueCompleted = true;

        OnAnyConversationCompleted?.Invoke();

        if (destroyAfterFinish)
        {
            if (_exitRoutine != null) StopCoroutine(_exitRoutine);
            _exitRoutine = StartCoroutine(DestroyAfterHide());
        }
        else
        {
            if (BaseProfessors.ActiveProfessor == null && !BaseProfessors.IsAnyMinigameActive)
            {
                FindReferences();
                PlayerUIModeHelper.PlayerExitUIMode(_player);
                PlayerUIModeHelper.EnableTasksButton(_allTasksBackButton, _currentTaskButton);
                TaskUI.SetTopUIVisible(true);
                if (_joyStick != null) UIUtils.SetAlpha(_joyStick, 1f);
            }
        }
    }

    private IEnumerator DestroyAfterHide()
    {
        yield return new WaitForSeconds(hideDuration);

        if (BaseProfessors.ActiveProfessor == null && !BaseProfessors.IsAnyMinigameActive)
        {
            FindReferences();
            PlayerUIModeHelper.PlayerExitUIMode(_player);
            PlayerUIModeHelper.EnableTasksButton(_allTasksBackButton, _currentTaskButton);
            TaskUI.SetTopUIVisible(true);
            if (_joyStick != null) UIUtils.SetAlpha(_joyStick, 1f);
        }

        if (dialogueWindows != null)
        {
            foreach (var dlg in dialogueWindows)
            {
                if (dlg != null)
                    Destroy(dlg.gameObject);
            }
        }

        Destroy(gameObject);
    }
}
