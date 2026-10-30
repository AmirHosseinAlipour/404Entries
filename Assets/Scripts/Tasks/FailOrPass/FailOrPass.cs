using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using ArabicSupport;

public class FailOrPass : BaseProfessors
{
    public override ProfessorUIType UIType => ProfessorUIType.ScreenSpaceUI;

    [Header("Mini Game")]
    public RectTransform task;
    public FarsiTypewriter letter;
    public Button firstLetter;
    private MusicChange musicChange;

    private const string challengeDialogue = "خب آماده ای برگه ها رو تصحیح کنی؟";

    protected override void OnEnable()
    {
        base.OnEnable();
        DialogueSequenceTrigger.OnAnyConversationCompleted -= HandleConversationFinished;
        DialogueSequenceTrigger.OnAnyConversationCompleted += HandleConversationFinished;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        DialogueSequenceTrigger.OnAnyConversationCompleted -= HandleConversationFinished;
    }

    protected override void Start()
    {
        base.Start();
        DialogueSequenceTrigger.OnAnyConversationCompleted -= HandleConversationFinished;
        DialogueSequenceTrigger.OnAnyConversationCompleted += HandleConversationFinished;

        if (letter != null)
        {
            letter.OnTypingFinished += EnableButton;
        }

        musicChange = GetComponent<MusicChange>();
        HideMinigameUI();
    }

    public override void HideMinigameUI()
    {
        base.HideMinigameUI();
        if (task != null) task.gameObject.SetActive(false);
        if (StartPanel != null) StartPanel.gameObject.SetActive(false);
        if (acceptRect != null) acceptRect.gameObject.SetActive(false);
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (isMinigameActive || IsAnyMinigameActive)
            {
                return;
            }

            if (_player == null) _player = other.gameObject.GetComponent<PlayerController>();

            bool isTaskDone = false;
            if (assignedTask != null && TaskManager.Instance != null)
            {
                isTaskDone = TaskManager.Instance.IsTaskCompleted(assignedTask);
            }
            else if (TaskManager.Instance != null && TaskManager.Instance.taskCompleted != null &&
                     TaskOrderNumber >= 0 && TaskOrderNumber < TaskManager.Instance.taskCompleted.Length)
            {
                isTaskDone = TaskManager.Instance.taskCompleted[TaskOrderNumber];
            }

            if (isTaskDone)
            {
                if (afterWinDialogue != null)
                {
                    EnsureDialogueSortingOrder(afterWinDialogue);
                    UIAnimationManager.Instance.ShowWindow(afterWinDialogue, 0.25f);
                }
                return;
            }

            ActiveProfessor = this;
            FindReferences();
            AutoDiscoverUI();
            HookAcceptButtons();
            PlayerUIModeHelper.PlayerEnterUIMode(_player);
            PlayerUIModeHelper.DisableTasksButton(_allTasksBackButton, _currentTaskButton);
            TaskUI.SetTopUIVisible(false);
            SetMinimapVisible(false);
            UIUtils.SetAlpha(_joyStick, 0f);
            ApplyModernVisualStyles();

            HideMinigameUI();

            // Check if bottom dialogue sequence is present and needs to run first
            DialogueSequenceTrigger seq = FindAnyObjectByType<DialogueSequenceTrigger>();
            if (seq != null && !DialogueSequenceTrigger.HasKetabkhonehDialogueCompleted && !seq.HasCompleted)
            {
                // Ensure upper StartPanel and acceptRect are completely hidden while bottom dialogue is playing!
                if (StartPanel != null) StartPanel.gameObject.SetActive(false);
                if (acceptRect != null) acceptRect.gameObject.SetActive(false);

                DialogueSequenceTrigger.OnAnyConversationCompleted -= HandleConversationFinished;
                DialogueSequenceTrigger.OnAnyConversationCompleted += HandleConversationFinished;

                if (!seq.IsInConversation)
                {
                    seq.StartConversation();
                }
                return;
            }

            // If bottom dialogue already completed, show challenge prompt directly!
            ShowChallengePrompt();
        }
    }

    private void HandleConversationFinished()
    {
        DialogueSequenceTrigger.OnAnyConversationCompleted -= HandleConversationFinished;

        if (isMinigameActive || IsAnyMinigameActive)
        {
            return;
        }

        bool isTaskDone = false;
        if (assignedTask != null && TaskManager.Instance != null)
        {
            isTaskDone = TaskManager.Instance.IsTaskCompleted(assignedTask);
        }
        else if (TaskManager.Instance != null && TaskManager.Instance.taskCompleted != null &&
                 TaskOrderNumber >= 0 && TaskOrderNumber < TaskManager.Instance.taskCompleted.Length)
        {
            isTaskDone = TaskManager.Instance.taskCompleted[TaskOrderNumber];
        }

        if (isTaskDone)
        {
            GetBackToNormal();
            return;
        }

        ActiveProfessor = this;
        FindReferences();
        AutoDiscoverUI();
        HookAcceptButtons();
        ApplyModernVisualStyles();
        HideMinigameUI();

        PlayerUIModeHelper.PlayerEnterUIMode(_player);
        PlayerUIModeHelper.DisableTasksButton(_allTasksBackButton, _currentTaskButton);
        TaskUI.SetTopUIVisible(false);
        SetMinimapVisible(false);
        UIUtils.SetAlpha(_joyStick, 0f);

        StartCoroutine(ShowPromptAfterDelay(0.35f));
    }

    private IEnumerator ShowPromptAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowChallengePrompt();
    }

    public void ShowChallengePrompt()
    {
        FindReferences();
        AutoDiscoverUI();

        if (StartPanel != null)
        {
            Canvas c = StartPanel.GetComponentInParent<Canvas>(true);
            if (c != null)
            {
                c.overrideSorting = true;
                c.sortingOrder = 50;
            }
            UIAnimationManager.Instance.ShowWindow(StartPanel, 0.4f);
        }

        if (acceptRect != null)
        {
            Transform curr = acceptRect.transform.parent;
            while (curr != null && curr != transform)
            {
                curr.gameObject.SetActive(true);
                curr = curr.parent;
            }
            acceptRect.gameObject.SetActive(true);
            acceptRect.SetAsLastSibling();
            UIUtils.SetAlpha(acceptRect.gameObject, 1f);
        }

        Text dialogueText = GetDialogueText();
        FarsiTypewriter ftw = dialogueText != null ? dialogueText.GetComponent<FarsiTypewriter>() : null;

        if (ftw != null)
        {
            ftw.SetText(challengeDialogue);
            ftw.StartTyping();
        }
        else if (dialogueText != null)
        {
            string localized = LocalizationManager.Get(challengeDialogue, challengeDialogue);
            dialogueText.text = LocalizationManager.IsRTL ? ArabicFixer.Fix(localized, true, true) : localized;
        }

        SetAcceptButtonsVisible(true);
        HookAcceptButtons();
    }

    protected override void Update()
    {
        base.Update();
        if (ActiveProfessor == this && !isMinigameActive && InputReader.IsAnyClickDown())
        {
            Text dialogueText = GetDialogueText();
            if (dialogueText != null)
            {
                FarsiTypewriter ftw = dialogueText.GetComponent<FarsiTypewriter>();
                if (ftw != null && ftw.isTyping)
                {
                    ftw.SkipTyping();
                }
            }
        }
    }

    private Text GetDialogueText()
    {
        if (listOfTexts != null && listOfTexts.Length > 0 && listOfTexts[0] != null)
        {
            return listOfTexts[0];
        }
        if (acceptRect != null)
        {
            return acceptRect.GetComponentInChildren<Text>(true);
        }
        return null;
    }

    private void SetAcceptButtonsVisible(bool visible)
    {
        if (acceptRect == null) return;
        Button[] btns = acceptRect.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < btns.Length; i++)
        {
            if (btns[i] != null)
            {
                btns[i].gameObject.SetActive(visible);
                btns[i].interactable = true;
                Transform p = btns[i].transform.parent;
                while (p != null && p != acceptRect.transform.parent && p != transform)
                {
                    p.gameObject.SetActive(true);
                    p = p.parent;
                }
            }
        }
    }

    public override void AcceptChallenge()
    {
        DialogueSequenceTrigger.OnAnyConversationCompleted -= HandleConversationFinished;
        if (StartPanel != null) StartPanel.gameObject.SetActive(false);
        if (acceptRect != null) acceptRect.gameObject.SetActive(false);
        StartGame();
    }

    public override void DeclineChallenge()
    {
        DialogueSequenceTrigger.OnAnyConversationCompleted -= HandleConversationFinished;
        SetAcceptButtonsVisible(false);
        if (StartPanel != null) StartPanel.gameObject.SetActive(false);
        if (acceptRect != null) acceptRect.gameObject.SetActive(false);
        base.DeclineChallenge();
        if (ActiveProfessor == this)
        {
            ActiveProfessor = null;
        }
    }

    public void StartGame()
    {
        StartMinigame();
        UIAnimationManager.Instance.ShowWindow(task, 0.5f);

        if (PaperManager.Instance != null)
        {
            PaperManager.Instance.ResetGame();
        }

        if (firstLetter != null)
        {
            firstLetter.interactable = false;
        }
        if (musicChange != null)
        {
            musicChange.ToggleMusic();
            musicChange.PlayThemeMusic();
        }
        if (letter != null) letter.StartTyping();
    }

    private void EnableButton()
    {
        if (firstLetter != null)
        {
            firstLetter.interactable = true;
        }
    }

    private void OnDestroy()
    {
        DialogueSequenceTrigger.OnAnyConversationCompleted -= HandleConversationFinished;
        if (letter != null)
        {
            letter.OnTypingFinished -= EnableButton;
        }
    }
}
