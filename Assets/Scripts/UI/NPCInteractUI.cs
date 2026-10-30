using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using DG.Tweening;

public class NPCInteractUI : MonoBehaviour
{
    public static NPCInteractUI Instance { get; private set; }

    private static enterdialouge _currentNPC;
    private static Sprite _ePromptSprite;

    private Canvas _canvas;
    private CanvasGroup _canvasGroup;
    private RectTransform _buttonRect;
    private Button _eButton;

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

        // 2. Create Mobile E Button Container on the right side
        GameObject btnObj = new GameObject("Mobile_E_Button");
        btnObj.transform.SetParent(transform, false);

        _buttonRect = btnObj.AddComponent<RectTransform>();
        _buttonRect.anchorMin = new Vector2(1f, 0.5f);
        _buttonRect.anchorMax = new Vector2(1f, 0.5f);
        _buttonRect.pivot = new Vector2(1f, 0.5f);
        _buttonRect.anchoredPosition = new Vector2(-80f, 0f); // Right-middle thumb zone
        _buttonRect.sizeDelta = new Vector2(116f, 116f);

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
            _buttonRect.DOPunchScale(new Vector3(-0.15f, -0.15f, 0f), 0.15f, 1, 0.5f);
        }

        if (_currentNPC != null)
        {
            _currentNPC.TriggerInteraction();
        }
    }

    public static void Register(enterdialouge npc)
    {
        _currentNPC = npc;
    }

    public static void Unregister(enterdialouge npc)
    {
        if (_currentNPC == npc)
        {
            _currentNPC = null;
        }
    }

    private void Update()
    {
        // 1. Check PC Keyboard E key
        bool ePressed = false;
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            ePressed = true;
        else if (Input.GetKeyDown(KeyCode.E))
            ePressed = true;

        if (ePressed)
        {
            if (_currentNPC != null)
            {
                _currentNPC.TriggerInteraction();
            }
        }

        // 2. Update Mobile On-Screen Button visibility
        // Requirements:
        // - Only show if PlatformHelper.IsMobile() is TRUE (F2 hides it!)
        // - An NPC is in range
        // - Dialogue is not currently open
        bool isMobile = PlatformHelper.IsMobile();
        bool shouldShowMobile = isMobile &&
                                _currentNPC != null &&
                                !_currentNPC.IsDialogueActive;

        if (_canvasGroup != null)
        {
            if (!isMobile)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.interactable = false;
            }
            else
            {
                float targetAlpha = shouldShowMobile ? 1f : 0f;
                _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, targetAlpha, Time.deltaTime * 8f);
                _canvasGroup.blocksRaycasts = shouldShowMobile && _canvasGroup.alpha > 0.5f;
                _canvasGroup.interactable = shouldShowMobile && _canvasGroup.alpha > 0.5f;
            }
        }
    }
}
