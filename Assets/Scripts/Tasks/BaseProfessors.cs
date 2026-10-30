using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using ArabicSupport;

public enum ProfessorUIType
{
    ScreenSpaceUI,
    InWorldGame,
    InWorldStealth,
    InteractiveHUD
}

public class BaseProfessors : MonoBehaviour
{
    public virtual ProfessorUIType UIType => ProfessorUIType.ScreenSpaceUI;

    [Header("Base Fields")]
    public RectTransform StartPanel;
    public RectTransform acceptRect;
    public Text[] listOfTexts;
    public Target target;

    private const int InitialDialogueCount = 3;
    
    public int TaskOrderNumber;
    public TaskData assignedTask;
    
    [Header("After mission")]
    public RectTransform afterWinDialogue;
    public RectTransform firstWinDialogue;
    public FarsiTypewriter firstWinDialogueFtw;
    
    [Header("After task not complete")]
    public RectTransform afterFailDialogue;
    public RectTransform firstFailDialogue;
    public FarsiTypewriter firstFailDialogueFtw;
    
    public static bool IsAnyMinigameActive = false;
    public static BaseProfessors ActiveProfessor = null;
    public bool isMinigameActive = false;

    protected Button _allTasksBackButton;
    protected Button _currentTaskButton;
    protected GameObject _joyStick;
    protected PlayerController _player;

    protected virtual void Awake()
    {
        FindReferences();
        target = gameObject.GetComponent<Target>();
        AutoDiscoverUI();
        EnsureDialogueSortingOrder(afterWinDialogue);
        EnsureDialogueSortingOrder(firstWinDialogue);
        EnsureDialogueSortingOrder(afterFailDialogue);
        EnsureDialogueSortingOrder(firstFailDialogue);
        CleanDialogueContainers();
    }

    protected Coroutine _textSequenceCoroutine;

    protected void AutoDiscoverUI()
    {
        if (acceptRect == null)
        {
            RectTransform[] all = GetComponentsInChildren<RectTransform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] == null) continue;
                string n = all[i].name.ToLower();
                if (n == "accept" || n == "acceptpanel")
                {
                    acceptRect = all[i];
                    break;
                }
            }
        }

        if (StartPanel == null)
        {
            RectTransform[] all = GetComponentsInChildren<RectTransform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] == null) continue;
                string n = all[i].name.ToLower();
                if (n == "background_panel" || n == "startpanel")
                {
                    StartPanel = all[i];
                    break;
                }
            }
        }
    }

    public virtual void HookAcceptButtons()
    {
        if (acceptRect == null) return;
        Button[] btns = acceptRect.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < btns.Length; i++)
        {
            if (btns[i] == null) continue;
            string btnName = btns[i].gameObject.name.ToLower();
            if (btnName.Contains("1") || btnName.Contains("yes") || btnName.Contains("accept") || i == 0)
            {
                btns[i].onClick.RemoveListener(AcceptChallenge);
                btns[i].onClick.AddListener(AcceptChallenge);
            }
            else if (btnName.Contains("2") || btnName.Contains("no") || btnName.Contains("decline") || btnName.Contains("cancel") || i == 1)
            {
                btns[i].onClick.RemoveListener(DeclineChallenge);
                btns[i].onClick.AddListener(DeclineChallenge);
            }
        }
    }

    public virtual void AcceptChallenge()
    {
        if (isMinigameActive || IsAnyMinigameActive) return;
        StartMinigame();
    }

    public virtual void DeclineChallenge()
    {
        GetBackToNormal();
    }

    public virtual void HideMinigameUI()
    {
        // Subclasses hide their specific minigame UIs here before minigame starts
    }

    public void EnsureDialogueSortingOrder(RectTransform rect)
    {
        if (rect == null) return;
        Canvas c = rect.GetComponentInParent<Canvas>();
        if (c != null && c.sortingOrder < 10)
        {
            c.overrideSorting = true;
            c.sortingOrder = 10;
        }
    }

    protected virtual void Start()
    {
        if (_player == null)
        {
            FindReferences();
        }
        AutoDiscoverUI();
        HookAcceptButtons();
        HideMinigameUI();
        ApplyModernVisualStyles();
    }

    protected virtual void Update()
    {
        if (_textSequenceCoroutine != null && InputReader.IsAnyClickDown())
        {
            SkipAllTextSequence();
        }
    }

    public void SkipAllTextSequence()
    {
        if (_textSequenceCoroutine != null)
        {
            StopCoroutine(_textSequenceCoroutine);
            _textSequenceCoroutine = null;
        }

        if (listOfTexts != null)
        {
            for (int i = 0; i < Mathf.Min(InitialDialogueCount, listOfTexts.Length); i++)
            {
                if (listOfTexts[i] != null)
                {
                    FarsiTypewriter ftw = listOfTexts[i].GetComponent<FarsiTypewriter>();
                    if (ftw != null)
                    {
                        ftw.SkipTyping();
                    }
                }
            }
        }

        if (acceptRect != null)
        {
            acceptRect.gameObject.SetActive(true);
            UIUtils.SetAlpha(acceptRect.gameObject, 1f);
        }
    }

    protected void FindReferences()
    {
        if (_player == null)
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null) _player = playerObj.GetComponent<PlayerController>();
        }
        
        if (_allTasksBackButton == null)
        {
            var backBtnObj = GameObject.FindWithTag("AllTasksBackButton");
            if (backBtnObj != null) _allTasksBackButton = backBtnObj.GetComponent<Button>();
        }

        if (_currentTaskButton == null)
        {
            var currBtnObj = GameObject.FindWithTag("CurrentTaskButton");
            if (currBtnObj != null) _currentTaskButton = currBtnObj.GetComponent<Button>();
        }

        if (_joyStick == null)
        {
            _joyStick = GameObject.FindWithTag("JoyStick");
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
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
            }
            else
            {
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
                
                // 1. Hide any minigame-specific UI elements before challenge starts
                HideMinigameUI();

                // 2. Ensure StartPanel (Background_Panel) is active and shown
                if (StartPanel != null)
                {
                    EnsureDialogueSortingOrder(StartPanel);
                    UIAnimationManager.Instance.ShowWindow(StartPanel, 0.4f);
                }

                // 3. Ensure acceptRect is active, visible, and all its parent GameObjects are active
                if (acceptRect != null)
                {
                    Transform curr = acceptRect.transform.parent;
                    while (curr != null && curr != transform)
                    {
                        curr.gameObject.SetActive(true);
                        curr = curr.parent;
                    }
                    acceptRect.gameObject.SetActive(true);
                    UIUtils.SetAlpha(acceptRect.gameObject, 1f);
                }

                // 4. Type challenge dialogue question
                if (listOfTexts != null && listOfTexts.Length > 0 && listOfTexts[0] != null)
                {
                    if (_textSequenceCoroutine != null)
                    {
                        StopCoroutine(_textSequenceCoroutine);
                    }
                    var ftw = listOfTexts[0].GetComponent<FarsiTypewriter>();
                    if (ftw != null) ftw.PrepareForTyping();
                    listOfTexts[0].text = "";
                    _textSequenceCoroutine = StartCoroutine(TextSequence());
                }
            }
        }
    }

    private IEnumerator TextSequence()
    {
        if (listOfTexts == null || listOfTexts.Length == 0) yield break;

        if (listOfTexts[0] != null)
        {
            FarsiTypewriter text = listOfTexts[0].gameObject.GetComponent<FarsiTypewriter>();
            if (text != null)
            {
                text.StartTyping();
                while (text.isTyping)
                {
                    if (InputReader.IsAnyClickDown())
                    {
                        text.SkipTyping();
                        break;
                    }
                    yield return null;
                }
            }
        }

        if (acceptRect != null)
        {
            acceptRect.gameObject.SetActive(true);
            UIUtils.SetAlpha(acceptRect.gameObject, 1f);
        }

        _textSequenceCoroutine = null;
    }

    protected virtual void OnEnable()
    {
        SwitchWindow.OnPanelStateChanged += HandlePanelStateChange;
    }

    protected virtual void OnDisable()
    {
        SwitchWindow.OnPanelStateChanged -= HandlePanelStateChange;
    }

    public void HideAllProfessorUI()
    {
        if (acceptRect != null)
        {
            acceptRect.gameObject.SetActive(false);
        }
        if (StartPanel != null)
        {
            StartPanel.gameObject.SetActive(false);
        }

        Transform bgPanel = transform.Find("Background_Panel");
        if (bgPanel != null)
        {
            bgPanel.gameObject.SetActive(false);
        }

        Transform taskCanvas = transform.Find("Task_Canvas");
        if (taskCanvas != null)
        {
            Transform innerBg = taskCanvas.Find("Background_Panel");
            if (innerBg != null) innerBg.gameObject.SetActive(false);
        }
    }

    public virtual void StartMinigame()
    {
        if (isMinigameActive) return;
        isMinigameActive = true;
        IsAnyMinigameActive = true;
        ActiveProfessor = this;
        FindReferences();

        if (_textSequenceCoroutine != null)
        {
            StopCoroutine(_textSequenceCoroutine);
            _textSequenceCoroutine = null;
        }

        TaskUI.SetTopUIVisible(false);
        SetMinimapVisible(false);

        switch (UIType)
        {
            case ProfessorUIType.InWorldGame:
                HideAllProfessorUI();
                PlayerUIModeHelper.PlayerEnterUIMode(_player);
                PlayerUIModeHelper.DisableTasksButton(_allTasksBackButton, _currentTaskButton);
                UIUtils.SetAlpha(_joyStick, 0f);
                break;

            case ProfessorUIType.InWorldStealth:
                HideAllProfessorUI();
                PlayerUIModeHelper.PlayerExitUIMode(_player);
                PlayerUIModeHelper.DisableTasksButton(_allTasksBackButton, _currentTaskButton);
                if (PlatformHelper.IsMobile())
                {
                    UIUtils.SetAlpha(_joyStick, 1f);
                }
                break;

            case ProfessorUIType.InteractiveHUD:
                HideAllProfessorUI();
                PlayerUIModeHelper.PlayerEnterUIMode(_player);
                PlayerUIModeHelper.DisableTasksButton(_allTasksBackButton, _currentTaskButton);
                UIUtils.SetAlpha(_joyStick, 0f);
                break;

            case ProfessorUIType.ScreenSpaceUI:
            default:
                if (acceptRect != null)
                {
                    acceptRect.gameObject.SetActive(false);
                }
                PlayerUIModeHelper.PlayerEnterUIMode(_player);
                PlayerUIModeHelper.DisableTasksButton(_allTasksBackButton, _currentTaskButton);
                UIUtils.SetAlpha(_joyStick, 0f);
                break;
        }
    }

    private void HandlePanelStateChange(bool isActive)
    {
        if (!isActive)
        {
            if (IsAnyMinigameActive || isMinigameActive)
            {
                return;
            }
            if (ActiveProfessor != null && ActiveProfessor != this)
            {
                return;
            }
            GetBackToNormal();
            if (ActiveProfessor == this)
            {
                ActiveProfessor = null;
            }
        }
    }

    public virtual void CompleteMission()
    {
        isMinigameActive = false;
        IsAnyMinigameActive = false;
        if (ActiveProfessor == this)
        {
            ActiveProfessor = null;
        }

        if (_textSequenceCoroutine != null)
        {
            StopCoroutine(_textSequenceCoroutine);
            _textSequenceCoroutine = null;
        }

        if (assignedTask != null && TaskManager.Instance != null)
        {
            TaskManager.Instance.CompleteTask(assignedTask);
        }
        else if (TaskManager.Instance != null)
        {
            TaskManager.Instance.CompleteTask(TaskOrderNumber);
        }

        HideAllProfessorUI();
        HideMinigameUI();
        CleanDialogueContainers();

        if (firstWinDialogue != null && UIAnimationManager.Instance != null)
        {
            PlayerUIModeHelper.PlayerEnterUIMode(_player);
            PlayerUIModeHelper.DisableTasksButton(_allTasksBackButton, _currentTaskButton);
            TaskUI.SetTopUIVisible(false);
            SetMinimapVisible(false);
            UIUtils.SetAlpha(_joyStick, 0f);
            EnsureDialogueSortingOrder(firstWinDialogue);
            UIAnimationManager.Instance.ShowDialogueWindow(firstWinDialogue, 0.5f, firstWinDialogueFtw);
        }
        else
        {
            GetBackToNormal();
        }

        var music = GetComponent<MusicChange>();
        if (music != null)
        {
            music.ToggleMusic();
            music.PlayMainMusic();
        }

        if (target != null)
        {
            target.DisableTarget();
        }
    }

    public virtual void FailMission()
    {
        isMinigameActive = false;
        IsAnyMinigameActive = false;
        if (ActiveProfessor == this)
        {
            ActiveProfessor = null;
        }

        if (_textSequenceCoroutine != null)
        {
            StopCoroutine(_textSequenceCoroutine);
            _textSequenceCoroutine = null;
        }

        HideAllProfessorUI();
        HideMinigameUI();
        CleanDialogueContainers();

        if (firstFailDialogue != null && UIAnimationManager.Instance != null)
        {
            PlayerUIModeHelper.PlayerEnterUIMode(_player);
            PlayerUIModeHelper.DisableTasksButton(_allTasksBackButton, _currentTaskButton);
            TaskUI.SetTopUIVisible(false);
            SetMinimapVisible(false);
            UIUtils.SetAlpha(_joyStick, 0f);
            EnsureDialogueSortingOrder(firstFailDialogue);
            UIAnimationManager.Instance.ShowDialogueWindow(firstFailDialogue, 0.5f, firstFailDialogueFtw);
        }
        else
        {
            GetBackToNormal();
        }

        var music = GetComponent<MusicChange>();
        if (music != null)
        {
            music.ToggleMusic();
            music.PlayMainMusic();
        }
    }

    public void GetBackToNormal()
    {
        if (IsAnyMinigameActive || isMinigameActive)
        {
            return;
        }

        if (_textSequenceCoroutine != null)
        {
            StopCoroutine(_textSequenceCoroutine);
            _textSequenceCoroutine = null;
        }

        HideAllProfessorUI();
        HideMinigameUI();
        CleanDialogueContainers();
        FindReferences();
        PlayerUIModeHelper.PlayerExitUIMode(_player);
        PlayerUIModeHelper.EnableTasksButton(_allTasksBackButton, _currentTaskButton);
        TaskUI.SetTopUIVisible(true);
        SetMinimapVisible(true);
        if (PlatformHelper.IsMobile())
        {
            UIUtils.SetAlpha(_joyStick, 1f);
        }
        if (TaskUI.Instance != null)
        {
            TaskUI.Instance.UpdateUI();
        }
    }

    private static GameObject _cachedMiniMap;

    public void SetMinimapVisible(bool visible)
    {
        try
        {
            if (_cachedMiniMap == null)
            {
                _cachedMiniMap = GameObject.Find("MiniMapCanvas");
                if (_cachedMiniMap == null) _cachedMiniMap = GameObject.Find("MiniMap_Canvas");
                if (_cachedMiniMap == null) _cachedMiniMap = GameObject.Find("MiniMap");
            }

            if (_cachedMiniMap != null)
            {
                _cachedMiniMap.SetActive(visible);
                UIUtils.SetAlpha(_cachedMiniMap, visible ? 1f : 0f);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("SetMinimapVisible safe catch: " + ex.Message);
        }
    }

    public virtual void ApplyModernVisualStyles()
    {
        Sprite cardPanel = Resources.Load<Sprite>("UI/dialogue_card_panel");
        Sprite btnNormal = Resources.Load<Sprite>("UI/dialogue_button_normal");
        Sprite btnHover = Resources.Load<Sprite>("UI/dialogue_button_hover");
        Sprite btnPressed = Resources.Load<Sprite>("UI/dialogue_button_pressed");

        // 1. Style StartPanel (the outer Background_Panel dialog card)
        if (StartPanel != null)
        {
            Image startImg = StartPanel.GetComponent<Image>();
            if (startImg != null && cardPanel != null)
            {
                StyleCardPanel(startImg, cardPanel);
            }
        }

        // Also style any Background_Panel or StartPanel found under this professor
        Image[] allImages = GetComponentsInChildren<Image>(true);
        for (int i = 0; i < allImages.Length; i++)
        {
            if (allImages[i] == null) continue;
            string goName = allImages[i].gameObject.name.ToLower();
            if (goName == "background_panel" || goName == "startpanel")
            {
                StyleCardPanel(allImages[i], cardPanel);
            }
        }

        // 2. Style acceptRect (including inner TextPanel speech card and choice buttons)
        if (acceptRect != null)
        {
            StyleAcceptPanel(acceptRect, cardPanel, btnNormal, btnHover, btnPressed);
        }

        // 3. Style prompt question texts
        if (listOfTexts != null)
        {
            for (int i = 0; i < listOfTexts.Length; i++)
            {
                if (listOfTexts[i] != null)
                {
                    StyleText(listOfTexts[i]);
                }
            }
        }

        CleanDialogueContainers();
    }

    public static void StyleCardPanel(Image img, Sprite cardPanel)
    {
        if (img == null || cardPanel == null) return;
        img.material = null;
        img.sprite = cardPanel;
        img.type = Image.Type.Sliced;
        img.color = Color.white;

        // Disable RoundedCorners component so it doesn't clip or override our 9-sliced card
        MonoBehaviour[] monos = img.GetComponents<MonoBehaviour>();
        for (int m = 0; m < monos.Length; m++)
        {
            if (monos[m] != null && monos[m].GetType().Name.Contains("RoundedCorners"))
            {
                monos[m].enabled = false;
            }
        }
    }

    public static void StyleAcceptPanel(RectTransform accept, Sprite cardPanel, Sprite btnNormal, Sprite btnHover, Sprite btnPressed)
    {
        if (accept == null) return;

        // 1. Style inner dialogue speech panels (e.g. TextPanel)
        Image[] imgs = accept.GetComponentsInChildren<Image>(true);
        for (int i = 0; i < imgs.Length; i++)
        {
            if (imgs[i] == null) continue;
            if (imgs[i].GetComponentInParent<Button>() != null) continue;
            if (imgs[i].rectTransform == accept) continue;

            string imgName = imgs[i].gameObject.name.ToLower();

            // Background dimmers or full-screen dark overlays
            if (imgName.Contains("blackback") || imgName.Contains("overlay"))
            {
                Color c = imgs[i].color;
                c.a = 0f;
                imgs[i].color = c;
                continue;
            }

            // Character portraits, avatars, icons should NEVER be styled as panels
            if (imgName.Contains("person") || imgName.Contains("avatar") || imgName.Contains("portrait") || 
                imgName.Contains("icon") || imgName.Contains("ui") || imgName.Contains("gfx"))
            {
                continue;
            }

            // Style TextPanel and any inner dialogue cards
            if (cardPanel != null)
            {
                StyleCardPanel(imgs[i], cardPanel);
            }
        }

        // 2. Style choice buttons (Option 1 = "بله", Option 2 = "خیر")
        Button[] btns = accept.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < btns.Length; i++)
        {
            if (btns[i] == null) continue;
            string btnName = btns[i].gameObject.name.ToLower();
            bool isYes = btnName.Contains("1") || btnName.Contains("yes") || btnName.Contains("accept") || i == 0;
            bool isNo = btnName.Contains("2") || btnName.Contains("no") || btnName.Contains("reject") || btnName.Contains("cancel") || i == 1;

            StyleChoiceButton(btns[i], btnNormal, btnHover, btnPressed, isYes ? "بله" : (isNo ? "خیر" : null));
        }

        // 3. Style text
        Text[] txts = accept.GetComponentsInChildren<Text>(true);
        for (int i = 0; i < txts.Length; i++)
        {
            if (txts[i] != null)
            {
                StyleText(txts[i]);
            }
        }
    }

    public static void StyleChoiceButton(Button btn, Sprite btnNormal, Sprite btnHover, Sprite btnPressed, string defaultPersianText)
    {
        if (btn == null) return;

        Image img = btn.GetComponent<Image>();
        if (img != null)
        {
            img.material = null;
            if (btnNormal != null)
            {
                img.sprite = btnNormal;
                img.type = Image.Type.Sliced;
            }
            img.color = Color.white;
        }

        MonoBehaviour[] monos = btn.GetComponents<MonoBehaviour>();
        for (int m = 0; m < monos.Length; m++)
        {
            if (monos[m] != null && monos[m].GetType().Name.Contains("RoundedCorners"))
            {
                monos[m].enabled = false;
            }
        }

        if (btnHover != null && btnPressed != null)
        {
            btn.transition = Selectable.Transition.SpriteSwap;
            SpriteState ss = btn.spriteState;
            ss.highlightedSprite = btnHover;
            ss.pressedSprite = btnPressed;
            ss.selectedSprite = btnHover;
            ss.disabledSprite = btnNormal != null ? btnNormal : (img != null ? img.sprite : null);
            btn.spriteState = ss;
        }

        Text btnTxt = btn.GetComponentInChildren<Text>(true);
        if (btnTxt != null)
        {
            var ftw = btnTxt.GetComponent<FarsiTypewriter>();
            if (ftw != null)
            {
                ftw.typeWriterEffect = false;
                ftw.allowClickToSkip = false;
                ftw.centerAlign = true;
                ftw.middleAlign = true;
                if (!string.IsNullOrEmpty(defaultPersianText))
                {
                    ftw.SetText(defaultPersianText);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(defaultPersianText))
                {
                    string target = LocalizationManager.Translate(defaultPersianText);
                    btnTxt.text = LocalizationManager.IsRTL ? ArabicFixer.Fix(target, true, true) : target;
                }
            }

            btnTxt.color = Color.white;
            btnTxt.alignment = TextAnchor.MiddleCenter;

            RectTransform txtRect = btnTxt.rectTransform;
            if (txtRect != null)
            {
                txtRect.anchorMin = Vector2.zero;
                txtRect.anchorMax = Vector2.one;
                txtRect.offsetMin = Vector2.zero;
                txtRect.offsetMax = Vector2.zero;
                txtRect.pivot = new Vector2(0.5f, 0.5f);
            }

            StyleText(btnTxt);
        }
    }

    public static void StyleButton(Button btn, Sprite btnNormal, Sprite btnHover, Sprite btnPressed)
    {
        StyleChoiceButton(btn, btnNormal, btnHover, btnPressed, null);
    }

    public static void StyleText(Text txt)
    {
        if (txt == null) return;
        txt.color = Color.white;
        Shadow shadow = txt.GetComponent<Shadow>();
        if (shadow == null)
        {
            shadow = txt.gameObject.AddComponent<Shadow>();
        }
        shadow.effectColor = new Color(0f, 0f, 0f, 0.9f);
        shadow.effectDistance = new Vector2(1.2f, -1.2f);
    }

    private void CleanDialogueContainers()
    {
        MakeContainerTransparent(firstWinDialogue);
        MakeContainerTransparent(afterWinDialogue);
        MakeContainerTransparent(firstFailDialogue);
        MakeContainerTransparent(afterFailDialogue);

        Transform comp = transform.Find("DialoguesAfterTaskCompletion");
        if (comp != null) MakeContainerTransparent(comp as RectTransform);

        Transform notComp = transform.Find("DialoguesAfterTaskNotCompletion");
        if (notComp != null) MakeContainerTransparent(notComp as RectTransform);
    }

    private void MakeContainerTransparent(RectTransform container)
    {
        if (container == null) return;
        Image img = container.GetComponent<Image>();
        if (img != null)
        {
            Color c = img.color;
            c.a = 0f;
            img.color = c;
            img.raycastTarget = true;
        }

        Image[] childImgs = container.GetComponentsInChildren<Image>(true);
        for (int i = 0; i < childImgs.Length; i++)
        {
            if (childImgs[i] == null) continue;
            string goName = childImgs[i].gameObject.name.ToLower();
            if (goName.Contains("person") || goName.Contains("dialogueafter") || goName.Contains("blackback"))
            {
                Color c = childImgs[i].color;
                c.a = 0f;
                childImgs[i].color = c;
            }
        }
    }
}
