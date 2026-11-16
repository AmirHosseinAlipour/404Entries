using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

[Serializable]
public class Level
{
    public int[] keys;
}

[Serializable]
public class Options
{
    [TextArea] public List<string> items;
}

public class DraggableItemHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private Vector3 originalScale = Vector3.one;
    private Image image;
    public Sprite normalSprite;
    public Sprite hoverSprite;
    public Sprite pressedSprite;
    private BaseDraggableItem draggable;

    private void Awake()
    {
        originalScale = Vector3.one;
        image = GetComponent<Image>();
        draggable = GetComponent<BaseDraggableItem>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.dragging) return;
        if (draggable != null && draggable.isLocked) return;
        if (image != null && hoverSprite != null)
        {
            image.sprite = hoverSprite;
        }
        transform.DOKill();
        transform.DOScale(originalScale * 1.05f, 0.15f).SetUpdate(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (image != null && normalSprite != null)
        {
            image.sprite = normalSprite;
        }
        transform.DOKill();
        transform.DOScale(originalScale, 0.15f).SetUpdate(true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (draggable != null && draggable.isLocked) return;
        if (image != null && pressedSprite != null)
        {
            image.sprite = pressedSprite;
        }
        transform.DOKill();
        transform.DOScale(originalScale * 0.97f, 0.1f).SetUpdate(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (image != null)
        {
            image.sprite = (hoverSprite != null) ? hoverSprite : normalSprite;
        }
        transform.DOKill();
        transform.DOScale(originalScale * 1.05f, 0.1f).SetUpdate(true);
    }

    public void ResetState()
    {
        transform.DOKill();
        transform.localScale = originalScale;
        if (image != null && normalSprite != null)
        {
            image.sprite = normalSprite;
        }
    }

    private void OnDisable()
    {
        ResetState();
    }
}

public class Masoumi : BaseProfessors
{
    public override ProfessorUIType UIType => ProfessorUIType.ScreenSpaceUI;

    [Header("Settings")]
    public BaseDropZone[] initialZones;
    public BaseDropZone[] zonesToDrop;
    public BaseDraggableItem[] items;
    public Text[] itemsOptionsTexts;
    [TextArea] public string[] texts;
    public Options[] textOptions;

    public List<Level> levelKeys;

    [Header("References")] 
    public RectTransform speechPanel;
    public RectTransform missionPanel;
    public Text textLevels;

    private int _currentLevel = 0;
    private MusicChange _musicChange;

    protected override void Awake()
    {
        base.Awake();
        _musicChange = GetComponent<MusicChange>();
    }

    protected override void Start()
    {
        base.Start();
        HideMinigameUI();
        ApplyModernVisualStyles();
    }

    private RectTransform GetActualMissionPanel()
    {
        if (missionPanel != null && missionPanel != StartPanel) return missionPanel;
        if (StartPanel != null)
        {
            Transform m = StartPanel.Find("Mission");
            if (m != null) return m as RectTransform;
            foreach (Transform ch in StartPanel)
            {
                if (ch != null && ch.name.ToLower().Contains("mission"))
                    return ch as RectTransform;
            }
        }
        return missionPanel;
    }

    public override void HideMinigameUI()
    {
        base.HideMinigameUI();
        if (speechPanel != null) speechPanel.gameObject.SetActive(false);
        RectTransform mp = GetActualMissionPanel();
        if (mp != null && mp != StartPanel) mp.gameObject.SetActive(false);
    }

    public override void AcceptChallenge()
    {
        StartGame();
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") && !isMinigameActive && !IsAnyMinigameActive)
        {
            _currentLevel = 0;
        }
        base.OnTriggerEnter2D(other);
    }

    private void HookAcceptButtons()
    {
        if (acceptRect == null) return;
        Button[] btns = acceptRect.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < btns.Length; i++)
        {
            if (btns[i] == null) continue;
            string btnName = btns[i].gameObject.name.ToLower();
            if (btnName.Contains("1") || btnName.Contains("yes") || btnName.Contains("accept") || i == 0)
            {
                btns[i].onClick.RemoveListener(StartGame);
                btns[i].onClick.AddListener(StartGame);

                // Disable conflicting SwitchWindow on the Yes button so it doesn't trigger ShowDialogueWindow on Mission
                SwitchWindow sw = btns[i].GetComponent<SwitchWindow>();
                if (sw != null) sw.enabled = false;
            }
        }
    }

    public void StartGame()
    {
        StartMinigame();
        _currentLevel = 0;

        if (acceptRect != null)
        {
            acceptRect.gameObject.SetActive(false);
        }

        missionPanel = GetActualMissionPanel();

        if (missionPanel != null)
        {
            missionPanel.gameObject.SetActive(true);
            UIUtils.SetAlpha(missionPanel.gameObject, 1f);
            UIAnimationManager.Instance.ShowWindow(missionPanel, 0.4f);
        }

        if (speechPanel != null)
        {
            speechPanel.gameObject.SetActive(true);
            UIUtils.SetAlpha(speechPanel.gameObject, 1f);
            UIAnimationManager.Instance.ShowWindow(speechPanel, 0.4f);
        }

        if (_musicChange != null)
        {
            _musicChange.ToggleMusic();
            _musicChange.PlayThemeMusic();
        }

        ApplyModernVisualStyles();
        SetupLevel(0);
    }

    public void StartMission()
    {
        StartGame();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        BaseDraggableItem.OnItemDropCompleted += CheckComplete;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        BaseDraggableItem.OnItemDropCompleted -= CheckComplete;
    }

    private void CheckComplete()
    {
        if (levelKeys == null || _currentLevel >= levelKeys.Count) return;
        
        Level currentLevelData = levelKeys[_currentLevel];
        if (currentLevelData == null || currentLevelData.keys == null) return;
        
        if (items == null || zonesToDrop == null || items.Length != currentLevelData.keys.Length)
        {
            return;
        }

        for (int i = 0; i < items.Length; i++)
        {
            BaseDraggableItem item = items[i];
            if (item == null) return;

            int correctZoneIndex = currentLevelData.keys[i];
            if (correctZoneIndex < 0 || correctZoneIndex >= zonesToDrop.Length) return;

            BaseDropZone correctZone = zonesToDrop[correctZoneIndex];
            if (item.currentDropZone != correctZone)
            {
                return;
            }
        }
        
        StartCoroutine(LoadNextLevelAfterDelay(0.5f));
    }

    private IEnumerator LoadNextLevelAfterDelay(float delay)
    {
        if (speechPanel != null) UIAnimationManager.Instance.HideWindow(speechPanel, 0.5f);
        yield return new WaitForSeconds(delay);
        NextLevel();
    }

    private void SetupLevel(int level)
    {
        _currentLevel = level;

        if (initialZones != null && items != null)
        {
            for (int i = 0; i < initialZones.Length; i++)
            {
                if (i < items.Length && items[i] != null && initialZones[i] != null)
                {
                    items[i].currentDropZone = null;
                    items[i].isLocked = false;
                    items[i].transform.SetParent(initialZones[i].transform, false);
                    items[i].transform.localScale = Vector3.one;

                    var hover = items[i].GetComponent<DraggableItemHover>();
                    if (hover != null) hover.ResetState();
                    
                    if (items[i].rectTransform != null)
                    {
                        items[i].rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                        items[i].rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                        items[i].rectTransform.pivot = new Vector2(0.5f, 0.5f);
                        items[i].rectTransform.anchoredPosition = Vector2.zero;
                        items[i].rectTransform.sizeDelta = new Vector2(330f, 140f);
                    }

                    Image itmImg = items[i].GetComponent<Image>();
                    if (itmImg != null)
                    {
                        itmImg.raycastTarget = true;
                    }
                }
            }
        }

        if (textLevels != null && texts != null && _currentLevel < texts.Length)
        {
            textLevels.color = Color.white;
            textLevels.alignment = TextAnchor.MiddleCenter;
            textLevels.fontSize = 24;
            var ftw = textLevels.GetComponent<FarsiTypewriter>();
            if (ftw != null)
            {
                ftw.centerAlign = true;
                ftw.middleAlign = true;
                ftw.SetText(texts[_currentLevel]);
                if (ftw.gameObject.activeInHierarchy)
                {
                    ftw.StartTyping();
                }
                else
                {
                    ftw.SkipTyping();
                }
            }
            else
            {
                textLevels.text = texts[_currentLevel];
            }
        }

        if (textOptions != null && _currentLevel < textOptions.Length && textOptions[_currentLevel].items != null)
        {
            for (int i = 0; i < textOptions[_currentLevel].items.Count; i++)
            {
                if (itemsOptionsTexts != null && i < itemsOptionsTexts.Length && itemsOptionsTexts[i] != null)
                {
                    Text optTxt = itemsOptionsTexts[i];
                    optTxt.color = Color.white;
                    optTxt.alignment = TextAnchor.MiddleCenter;
                    optTxt.fontSize = 18;
                    optTxt.raycastTarget = false;

                    var ftw = optTxt.GetComponent<FarsiTypewriter>();
                    if (ftw != null)
                    {
                        ftw.typeWriterEffect = false;
                        ftw.centerAlign = true;
                        ftw.middleAlign = true;
                        ftw.allowClickToSkip = false;
                        ftw.SetText(textOptions[_currentLevel].items[i]);
                        ftw.SkipTyping();
                    }
                    else
                    {
                        optTxt.text = textOptions[_currentLevel].items[i];
                    }
                }
            }
        }
    }

    private void NextLevel()
    {
        _currentLevel++;

        if (levelKeys == null || _currentLevel >= levelKeys.Count)
        {
            HandleEnding();
            return;
        }
        
        if (speechPanel != null) UIAnimationManager.Instance.ShowWindow(speechPanel, 0.5f);
        SetupLevel(_currentLevel);
    }

    private void HandleEnding()
    {
        if (speechPanel != null) UIAnimationManager.Instance.HideWindow(speechPanel, 0.5f);
        RectTransform mp = GetActualMissionPanel();
        if (mp != null && mp != StartPanel) UIAnimationManager.Instance.HideWindow(mp, 0.5f);
        if (StartPanel != null) UIAnimationManager.Instance.HideWindow(StartPanel, 0.5f);
        CompleteMission();
    }

    public override void ApplyModernVisualStyles()
    {
        base.ApplyModernVisualStyles();

        Sprite cardPanel = Resources.Load<Sprite>("UI/dialogue_card_panel");
        Sprite btnNormal = Resources.Load<Sprite>("UI/dialogue_button_normal");
        Sprite btnHover = Resources.Load<Sprite>("UI/dialogue_button_hover");
        Sprite btnPressed = Resources.Load<Sprite>("UI/dialogue_button_pressed");

        // 0. StartPanel / Background_Panel should be a clean, semi-transparent dark overlay, NOT a giant card with a border!
        if (StartPanel != null)
        {
            Image startImg = StartPanel.GetComponent<Image>();
            if (startImg != null)
            {
                startImg.material = null;
                startImg.sprite = null;
                startImg.color = new Color(0.04f, 0.07f, 0.12f, 0.88f);
            }
        }

        // 1. Root speechPanel: fills Mission completely, transparent background
        if (speechPanel != null)
        {
            speechPanel.anchorMin = Vector2.zero;
            speechPanel.anchorMax = Vector2.one;
            speechPanel.pivot = new Vector2(0.5f, 0.5f);
            speechPanel.anchoredPosition = Vector2.zero;
            speechPanel.sizeDelta = Vector2.zero;

            Image panelImg = speechPanel.GetComponent<Image>();
            if (panelImg != null)
            {
                panelImg.color = new Color(0, 0, 0, 0);
            }
        }

        // 2. Speech Bubble Card (holding textLevels at the top of the screen)
        Transform speechCard = textLevels != null ? textLevels.transform.parent : (speechPanel != null ? speechPanel.Find("Text") : null);
        if (speechCard != null)
        {
            RectTransform cardRt = speechCard as RectTransform;
            if (cardRt != null)
            {
                cardRt.anchorMin = new Vector2(0.5f, 1f);
                cardRt.anchorMax = new Vector2(0.5f, 1f);
                cardRt.pivot = new Vector2(0.5f, 1f);
                cardRt.anchoredPosition = new Vector2(-70f, -40f);
                cardRt.sizeDelta = new Vector2(1060f, 210f);
            }

            MonoBehaviour[] monos = speechCard.GetComponents<MonoBehaviour>();
            for (int m = 0; m < monos.Length; m++)
            {
                if (monos[m] != null && monos[m].GetType().Name.Contains("RoundedCorners"))
                {
                    monos[m].enabled = false;
                }
            }

            Image cardImg = speechCard.GetComponent<Image>();
            if (cardImg != null && cardPanel != null)
            {
                cardImg.material = null;
                cardImg.sprite = cardPanel;
                cardImg.type = Image.Type.Sliced;
                cardImg.color = Color.white;
            }
        }

        // 3. Masoumi Speech Text (textLevels) inside speechCard
        if (textLevels != null)
        {
            RectTransform txtRt = textLevels.rectTransform;
            if (txtRt != null)
            {
                txtRt.anchorMin = Vector2.zero;
                txtRt.anchorMax = Vector2.one;
                txtRt.pivot = new Vector2(0.5f, 0.5f);
                txtRt.anchoredPosition = Vector2.zero;
                txtRt.sizeDelta = new Vector2(-60f, -30f);
            }

            textLevels.color = Color.white;
            textLevels.alignment = TextAnchor.MiddleCenter;
            textLevels.fontSize = 24;

            Shadow shadow = textLevels.GetComponent<Shadow>();
            if (shadow == null)
            {
                shadow = textLevels.gameObject.AddComponent<Shadow>();
            }
            shadow.effectColor = new Color(0f, 0f, 0f, 0.95f);
            shadow.effectDistance = new Vector2(1.5f, -1.5f);

            var ftw = textLevels.GetComponent<FarsiTypewriter>();
            if (ftw != null)
            {
                ftw.centerAlign = true;
                ftw.middleAlign = true;
            }
        }

        // 4. PlaceHolders container (middle row of the screen)
        Transform phContainer = speechPanel != null ? speechPanel.Find("PlaceHolders") : null;
        if (phContainer != null)
        {
            RectTransform phRt = phContainer as RectTransform;
            if (phRt != null)
            {
                phRt.anchorMin = new Vector2(0.5f, 0.5f);
                phRt.anchorMax = new Vector2(0.5f, 0.5f);
                phRt.pivot = new Vector2(0.5f, 0.5f);
                phRt.anchoredPosition = new Vector2(-70f, 40f);
                phRt.sizeDelta = new Vector2(1060f, 160f);
            }
        }

        // Drop target slots
        if (zonesToDrop != null)
        {
            float[] xOffsets = new float[] { -360f, 0f, 360f };
            for (int i = 0; i < zonesToDrop.Length; i++)
            {
                if (zonesToDrop[i] == null) continue;
                Transform ph = zonesToDrop[i].transform.parent;
                if (ph != null)
                {
                    RectTransform phItemRt = ph as RectTransform;
                    if (phItemRt != null)
                    {
                        phItemRt.anchorMin = new Vector2(0.5f, 0.5f);
                        phItemRt.anchorMax = new Vector2(0.5f, 0.5f);
                        phItemRt.pivot = new Vector2(0.5f, 0.5f);
                        float x = i < xOffsets.Length ? xOffsets[i] : (i - 1) * 360f;
                        phItemRt.anchoredPosition = new Vector2(x, 0f);
                        phItemRt.sizeDelta = new Vector2(330f, 140f);
                    }

                    MonoBehaviour[] monos = ph.GetComponents<MonoBehaviour>();
                    for (int m = 0; m < monos.Length; m++)
                    {
                        if (monos[m] != null && monos[m].GetType().Name.Contains("RoundedCorners"))
                        {
                            monos[m].enabled = false;
                        }
                    }

                    Image phImg = ph.GetComponent<Image>();
                    if (phImg != null && btnNormal != null)
                    {
                        phImg.material = null;
                        phImg.sprite = btnNormal;
                        phImg.type = Image.Type.Sliced;
                        phImg.color = new Color(0.08f, 0.22f, 0.38f, 0.55f);
                    }

                    Text[] numTexts = ph.GetComponentsInChildren<Text>(true);
                    for (int t = 0; t < numTexts.Length; t++)
                    {
                        if (numTexts[t] != null && numTexts[t].gameObject != zonesToDrop[i].gameObject)
                        {
                            RectTransform numRt = numTexts[t].rectTransform;
                            if (numRt != null)
                            {
                                numRt.anchorMin = new Vector2(0.5f, 1f);
                                numRt.anchorMax = new Vector2(0.5f, 1f);
                                numRt.pivot = new Vector2(0.5f, 1f);
                                numRt.anchoredPosition = new Vector2(0f, 34f);
                                numRt.sizeDelta = new Vector2(120f, 35f);
                            }

                            numTexts[t].color = new Color(0.35f, 0.88f, 1f, 1f);
                            numTexts[t].alignment = TextAnchor.MiddleCenter;
                            numTexts[t].fontSize = 28;
                            numTexts[t].raycastTarget = false;

                            Shadow s = numTexts[t].GetComponent<Shadow>();
                            if (s == null) s = numTexts[t].gameObject.AddComponent<Shadow>();
                            s.effectColor = new Color(0f, 0f, 0f, 0.9f);
                            s.effectDistance = new Vector2(1.5f, -1.5f);
                        }
                    }

                    // Ensure dropzone fills parent slot
                    RectTransform zoneRt = zonesToDrop[i].transform as RectTransform;
                    if (zoneRt != null)
                    {
                        zoneRt.anchorMin = Vector2.zero;
                        zoneRt.anchorMax = Vector2.one;
                        zoneRt.pivot = new Vector2(0.5f, 0.5f);
                        zoneRt.anchoredPosition = Vector2.zero;
                        zoneRt.sizeDelta = Vector2.zero;
                    }
                }
            }
        }

        // 5. Phrases container (bottom row of the screen)
        Transform phrasesContainer = missionPanel != null ? missionPanel.Find("Phrases") : null;
        if (phrasesContainer != null)
        {
            RectTransform phrasesRt = phrasesContainer as RectTransform;
            if (phrasesRt != null)
            {
                phrasesRt.anchorMin = new Vector2(0.5f, 0f);
                phrasesRt.anchorMax = new Vector2(0.5f, 0f);
                phrasesRt.pivot = new Vector2(0.5f, 0f);
                phrasesRt.anchoredPosition = new Vector2(-70f, 50f);
                phrasesRt.sizeDelta = new Vector2(1060f, 160f);
            }
        }

        // Initial zones
        if (initialZones != null)
        {
            float[] xOffsets = new float[] { -360f, 0f, 360f };
            for (int i = 0; i < initialZones.Length; i++)
            {
                if (initialZones[i] == null) continue;
                RectTransform initRt = initialZones[i].transform as RectTransform;
                if (initRt != null)
                {
                    initRt.anchorMin = new Vector2(0.5f, 0.5f);
                    initRt.anchorMax = new Vector2(0.5f, 0.5f);
                    initRt.pivot = new Vector2(0.5f, 0.5f);
                    float x = i < xOffsets.Length ? xOffsets[i] : (i - 1) * 360f;
                    initRt.anchoredPosition = new Vector2(x, 0f);
                    initRt.sizeDelta = new Vector2(330f, 140f);
                }

                MonoBehaviour[] monos = initialZones[i].GetComponents<MonoBehaviour>();
                for (int m = 0; m < monos.Length; m++)
                {
                    if (monos[m] != null && monos[m].GetType().Name.Contains("RoundedCorners"))
                    {
                        monos[m].enabled = false;
                    }
                }

                Image initImg = initialZones[i].GetComponent<Image>();
                if (initImg != null && btnNormal != null)
                {
                    initImg.material = null;
                    initImg.sprite = btnNormal;
                    initImg.type = Image.Type.Sliced;
                    initImg.color = new Color(0.05f, 0.12f, 0.22f, 0.45f);
                }
            }
        }

        // 6. Draggable Items (Options 1, 2, 3)
        if (items != null)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null) continue;

                if (items[i].rectTransform != null)
                {
                    items[i].rectTransform.sizeDelta = new Vector2(330f, 140f);
                }

                MonoBehaviour[] monos = items[i].GetComponents<MonoBehaviour>();
                for (int m = 0; m < monos.Length; m++)
                {
                    if (monos[m] != null && monos[m].GetType().Name.Contains("RoundedCorners"))
                    {
                        monos[m].enabled = false;
                    }
                }

                Image itemImg = items[i].GetComponent<Image>();
                if (itemImg != null && btnNormal != null)
                {
                    itemImg.material = null;
                    itemImg.sprite = btnNormal;
                    itemImg.type = Image.Type.Sliced;
                    itemImg.color = Color.white;
                    itemImg.raycastTarget = true;
                }

                DraggableItemHover hover = items[i].GetComponent<DraggableItemHover>();
                if (hover == null)
                {
                    hover = items[i].gameObject.AddComponent<DraggableItemHover>();
                }
                hover.normalSprite = btnNormal;
                hover.hoverSprite = btnHover;
                hover.pressedSprite = btnPressed;

                // Ensure all child graphics don't block pointer clicks/drags on the parent
                Graphic[] childGraphics = items[i].GetComponentsInChildren<Graphic>(true);
                for (int g = 0; g < childGraphics.Length; g++)
                {
                    if (childGraphics[g] != null && childGraphics[g].gameObject != items[i].gameObject)
                    {
                        childGraphics[g].raycastTarget = false;
                    }
                }
            }
        }

        // 7. Option Texts inside draggable items
        if (itemsOptionsTexts != null)
        {
            for (int i = 0; i < itemsOptionsTexts.Length; i++)
            {
                if (itemsOptionsTexts[i] == null) continue;
                Text optTxt = itemsOptionsTexts[i];
                RectTransform txtRt = optTxt.rectTransform;
                if (txtRt != null)
                {
                    txtRt.anchorMin = Vector2.zero;
                    txtRt.anchorMax = Vector2.one;
                    txtRt.pivot = new Vector2(0.5f, 0.5f);
                    txtRt.anchoredPosition = Vector2.zero;
                    txtRt.sizeDelta = new Vector2(-30f, -20f);
                }

                optTxt.color = Color.white;
                optTxt.alignment = TextAnchor.MiddleCenter;
                optTxt.fontSize = 19;
                optTxt.raycastTarget = false;

                Shadow shadow = optTxt.GetComponent<Shadow>();
                if (shadow == null)
                {
                    shadow = optTxt.gameObject.AddComponent<Shadow>();
                }
                shadow.effectColor = new Color(0f, 0f, 0f, 0.9f);
                shadow.effectDistance = new Vector2(1.2f, -1.2f);

                var ftw = optTxt.GetComponent<FarsiTypewriter>();
                if (ftw != null)
                {
                    ftw.centerAlign = true;
                    ftw.middleAlign = true;
                    ftw.allowClickToSkip = false;
                }
            }
        }

        // 8. Masoumi Portrait on the right side (restored to original pixel-perfect size)
        Transform masoumiUi = transform.Find("Background_Panel/Masoumi_UI");
        if (masoumiUi == null && StartPanel != null) masoumiUi = StartPanel.Find("Masoumi_UI");
        if (masoumiUi != null)
        {
            RectTransform portraitRt = masoumiUi as RectTransform;
            if (portraitRt != null)
            {
                portraitRt.anchorMin = new Vector2(1f, 0f);
                portraitRt.anchorMax = new Vector2(1f, 0f);
                portraitRt.pivot = new Vector2(1f, 0f);
                portraitRt.anchoredPosition = new Vector2(-48f, 50f);
                portraitRt.sizeDelta = new Vector2(25f, 59f);
                portraitRt.localScale = new Vector3(10f, 10f, 10f);
            }
        }
    }
}