using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using DG.Tweening;

public class NPCInteractUI : MonoBehaviour
{
    public static NPCInteractUI Instance { get; private set; }

    private static readonly List<enterdialouge> _activeNPCs = new List<enterdialouge>();
    private static enterdialouge _currentNPC;
    private static Sprite _ePromptSprite;

    private Canvas _canvas;
    private CanvasGroup _canvasGroup;
    private RectTransform _buttonRect;
    private Button _eButton;
    private PlayerController _playerController;

    public static enterdialouge CurrentNPC => _currentNPC;

    public static Sprite EPromptSprite
    {
        get
        {
            if (_ePromptSprite == null)
            {
                _ePromptSprite = Resources.Load<Sprite>("UI/prompt_e_key");
            }
            return _ePromptSprite;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoInit()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("NPCInteractUI");
            Instance = go.AddComponent<NPCInteractUI>();
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
        CreateMobileUI();
    }

    private void OnEnable()
    {
        InputReader.OnInteractPressed -= HandleInteractInput;
        InputReader.OnInteractPressed += HandleInteractInput;
    }

    private void OnDisable()
    {
        InputReader.OnInteractPressed -= HandleInteractInput;
    }

    private void CreateMobileUI()
    {
        // 1. Create Canvas
        _canvas = gameObject.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 45; // Below dialogue (sortingOrder 50) and above game

        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>();

        // 2. Create Mobile E Button Container on the BOTTOM-RIGHT side
        GameObject btnObj = new GameObject("Mobile_E_Button");
        btnObj.transform.SetParent(transform, false);

        _buttonRect = btnObj.AddComponent<RectTransform>();
        _buttonRect.anchorMin = new Vector2(1f, 0f);
        _buttonRect.anchorMax = new Vector2(1f, 0f);
        _buttonRect.pivot = new Vector2(0.5f, 0.5f);
        // Positioned comfortably in the bottom-right action thumb zone (symmetrical to joystick)
        _buttonRect.anchoredPosition = new Vector2(-160f, 160f);
        _buttonRect.sizeDelta = new Vector2(130f, 130f);
        _buttonRect.localScale = new Vector3(0.7f, 0.7f, 1f);

        _canvasGroup = btnObj.AddComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;

        // Image
        Image img = btnObj.AddComponent<Image>();
        img.sprite = EPromptSprite;
        img.preserveAspect = true;

        // Button
        _eButton = btnObj.AddComponent<Button>();
        _eButton.transition = Selectable.Transition.None;
        _eButton.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        if (_buttonRect != null)
        {
            _buttonRect.DOKill();
            _buttonRect.localScale = Vector3.one;
            _buttonRect.DOPunchScale(new Vector3(-0.18f, -0.18f, 0f), 0.18f, 1, 0.5f);
        }

        TriggerCurrentInteraction();
    }

    private void HandleInteractInput()
    {
        TriggerCurrentInteraction();
    }

    private void TriggerCurrentInteraction()
    {
        UpdateCurrentNPC();
        if (_currentNPC != null)
        {
            _currentNPC.TriggerInteraction();
        }
    }

    public static void Register(enterdialouge npc)
    {
        if (npc == null) return;
        if (!_activeNPCs.Contains(npc))
        {
            _activeNPCs.Add(npc);
        }
        UpdateCurrentNPC();
    }

    public static void Unregister(enterdialouge npc)
    {
        if (npc == null) return;
        _activeNPCs.Remove(npc);
        UpdateCurrentNPC();
    }

    public static void UpdateCurrentNPC()
    {
        _activeNPCs.RemoveAll(n => n == null || !n.gameObject.activeInHierarchy);
        if (_activeNPCs.Count == 0)
        {
            _currentNPC = null;
            return;
        }

        if (_activeNPCs.Count == 1)
        {
            _currentNPC = _activeNPCs[0];
            return;
        }

        // Pick closest in-range NPC to the player
        Vector3 playerPos = Vector3.zero;
        var p = GameObject.FindWithTag("Player");
        if (p != null) playerPos = p.transform.position;

        enterdialouge closest = null;
        float minDist = float.MaxValue;
        for (int i = 0; i < _activeNPCs.Count; i++)
        {
            var item = _activeNPCs[i];
            if (item == null) continue;
            float d = Vector2.Distance(playerPos, item.transform.position);
            if (d < minDist)
            {
                minDist = d;
                closest = item;
            }
        }
        _currentNPC = closest ?? _activeNPCs[_activeNPCs.Count - 1];
    }

    private void Update()
    {
        // 1. Check PC Keyboard E key (direct fallback in case InputReader event was missed)
        bool ePressed = false;
        if (Keyboard.current != null && (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.fKey.wasPressedThisFrame))
            ePressed = true;
        else if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.F))
            ePressed = true;

        if (ePressed)
        {
            TriggerCurrentInteraction();
        }

        // 2. Periodic active NPC update
        UpdateCurrentNPC();

        // 3. Update Mobile On-Screen Button visibility
        // - Shown if PlatformHelper.IsMobile() is TRUE
        // - An NPC is in range
        // - Dialogue / cutscene is not currently active
        if (_playerController == null)
        {
            _playerController = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
        }

        bool isMobile = PlatformHelper.IsMobile();
        bool isDialogueActive = (_currentNPC != null && _currentNPC.IsDialogueActive) ||
                                DialogueSequenceTrigger.IsAnyConversationActive;
        bool isMinigameActive = BaseProfessors.IsAnyMinigameActive ||
                                (BaseProfessors.ActiveProfessor != null && BaseProfessors.ActiveProfessor.isMinigameActive) ||
                                (_playerController != null && _playerController.isUIActive);

        bool shouldShowMobile = isMobile && _currentNPC != null && !isDialogueActive && !isMinigameActive;

        if (_canvasGroup != null)
        {
            float targetAlpha = shouldShowMobile ? 1f : 0f;
            _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, targetAlpha, Time.unscaledDeltaTime * 8f);

            // Pop / scale effect while showing / hiding
            float targetScale = shouldShowMobile ? 1f : 0.7f;
            if (_buttonRect != null && !DOTween.IsTweening(_buttonRect))
            {
                float currentScale = _buttonRect.localScale.x;
                float newScale = Mathf.MoveTowards(currentScale, targetScale, Time.unscaledDeltaTime * 6f);
                _buttonRect.localScale = new Vector3(newScale, newScale, 1f);
            }

            bool canInteract = shouldShowMobile && _canvasGroup.alpha > 0.4f;
            _canvasGroup.blocksRaycasts = canInteract;
            _canvasGroup.interactable = canInteract;
        }
    }
}
