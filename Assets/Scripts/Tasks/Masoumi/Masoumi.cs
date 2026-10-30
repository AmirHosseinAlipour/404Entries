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

    public override void HideMinigameUI()
    {
        base.HideMinigameUI();
        if (speechPanel != null) speechPanel.gameObject.SetActive(false);
        if (missionPanel != null) missionPanel.gameObject.SetActive(false);
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

        if (missionPanel != null)
        {
            missionPanel.gameObject.SetActive(true);
            UIUtils.SetAlpha(missionPanel.gameObject, 1f);
            UIAnimationManager.Instance.ShowWindow(missionPanel, 0.5f);
        }

        if (speechPanel != null)
        {
            speechPanel.gameObject.SetActive(true);
            UIUtils.SetAlpha(speechPanel.gameObject, 1f);
            UIAnimationManager.Instance.ShowWindow(speechPanel, 0.5f);
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
                        items[i].rectTransform.anchoredPosition = Vector2.zero;
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
            textLevels.fontSize = 28;
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
                    optTxt.fontSize = 22;
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
        if (missionPanel != null) UIAnimationManager.Instance.HideWindow(missionPanel, 0.5f);
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

        // 1. Root speechPanel: transparent background so it doesn't cover the screen
        if (speechPanel != null)
        {
            Image panelImg = speechPanel.GetComponent<Image>();
            if (panelImg != null)
            {
                panelImg.color = new Color(0, 0, 0, 0);
            }
        }

        // 2. Speech Bubble Card (the actual card at the top holding textLevels)
        Transform speechCard = textLevels != null ? textLevels.transform.parent : (speechPanel != null ? speechPanel.Find("Text") : null);
        if (speechCard != null)
        {
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

        // 3. Masoumi Speech Text (textLevels)
        if (textLevels != null)
        {
            textLevels.color = Color.white;
            textLevels.alignment = TextAnchor.MiddleCenter;
            textLevels.fontSize = 28;

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

        // 4. Draggable Items (Option 1, 2, 3)
        if (items != null)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null) continue;

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

        // 5. Options Texts
        if (itemsOptionsTexts != null)
        {
            for (int i = 0; i < itemsOptionsTexts.Length; i++)
            {
                if (itemsOptionsTexts[i] == null) continue;
                Text optTxt = itemsOptionsTexts[i];
                optTxt.color = Color.white;
                optTxt.alignment = TextAnchor.MiddleCenter;
                optTxt.fontSize = 22;
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

        // 6. Target Drop Zones (PlaceHolders 1, 2, 3)
        if (zonesToDrop != null)
        {
            for (int i = 0; i < zonesToDrop.Length; i++)
            {
                if (zonesToDrop[i] == null) continue;
                Transform ph = zonesToDrop[i].transform.parent;
                if (ph != null)
                {
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
                            numTexts[t].color = new Color(0.35f, 0.88f, 1f, 1f);
                            numTexts[t].alignment = TextAnchor.MiddleCenter;
                            numTexts[t].fontSize = 34;
                            numTexts[t].raycastTarget = false;

                            Shadow s = numTexts[t].GetComponent<Shadow>();
                            if (s == null) s = numTexts[t].gameObject.AddComponent<Shadow>();
                            s.effectColor = new Color(0f, 0f, 0f, 0.9f);
                            s.effectDistance = new Vector2(1.5f, -1.5f);
                        }
                    }
                }
            }
        }

        // 7. Initial Zones (where items start at the bottom)
        if (initialZones != null)
        {
            for (int i = 0; i < initialZones.Length; i++)
            {
                if (initialZones[i] == null) continue;
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
    }
}