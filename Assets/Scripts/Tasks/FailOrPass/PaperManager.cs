using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ArabicSupport;

public class PaperManager : MonoBehaviour
{
    public static PaperManager Instance;
    
    [Header("Mini Game")]
    public RectTransform task;
    public FailOrPass prof;
    
    public FarsiTypewriter letterLow;
    public Button lastLetterLow;
    
    public FarsiTypewriter letterMedium;
    public Button lastLetterMedium;
    
    public FarsiTypewriter letterHigh;
    public Button lastLetterHigh;
    
    private FarsiTypewriter chosen;
    private Button chosenButton;

    public int[] counts;

    private int _passedCount;

    [Header("Papers")]
    public List<GameObject> listOfPapers;
    public float rotationDuration = 0.25f;
    public FarsiTypewriter counterFtw;

    private List<GameObject> _failedPapers;
    private List<GameObject> _passedPapers;
    
    [Header("Confirmation Dialogue")]
    public RectTransform dialogueBox;
    public Text[] texts;

    [Header("Stamp Settings")]
    public Sprite passStampSprite;
    public Sprite failStampSprite;
    public AudioClip stampAudioClip;

    private int _currentItemIndex;
    
    private DraggableItem _currentItem;
    private DropZone _currentZone;

    private int _textLength;

    private int _papersLength;
    private int _counter;

    private SoundPlayer _soundPlayer;
    private MusicChange _musicChange;
    private Coroutine _confirmationCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        _textLength = texts.Length;
        _papersLength = listOfPapers.Count;
        _counter = _papersLength;
        
        _failedPapers = new List<GameObject>();
        _passedPapers = new List<GameObject>();

        _soundPlayer = GetComponent<SoundPlayer>();
        
        if (letterLow != null) letterLow.OnTypingFinished += EnableButton;
        if (letterMedium != null) letterMedium.OnTypingFinished += EnableButton;
        if (letterHigh != null) letterHigh.OnTypingFinished += EnableButton;
        _musicChange = GetComponent<MusicChange>();
    }

    private void OnDestroy()
    {
        if (letterLow != null) letterLow.OnTypingFinished -= EnableButton;
        if (letterMedium != null) letterMedium.OnTypingFinished -= EnableButton;
        if (letterHigh != null) letterHigh.OnTypingFinished -= EnableButton;
        if (_confirmationCoroutine != null)
        {
            StopCoroutine(_confirmationCoroutine);
            _confirmationCoroutine = null;
        }
    }

    private void Start()
    {
        StyleConfirmationPopup();
        ResetGame();
    }

    public void ResetGame()
    {
        _counter = _papersLength = (listOfPapers != null && listOfPapers.Count > 0) ? listOfPapers.Count : 10;
        _currentItemIndex = 0;
        _passedCount = 0;

        if (_passedPapers == null) _passedPapers = new List<GameObject>();
        else _passedPapers.Clear();

        if (_failedPapers == null) _failedPapers = new List<GameObject>();
        else _failedPapers.Clear();

        ClearAllStamps();

        if (counterFtw != null)
        {
            counterFtw.SetText(_papersLength + " / " + _counter);
            counterFtw.StartTyping();
        }

        if (listOfPapers != null)
        {
            foreach (GameObject paper in listOfPapers)
            {
                if (paper != null)
                {
                    paper.SetActive(false);
                    paper.transform.localRotation = Quaternion.identity;
                }
            }

            if (listOfPapers.Count > 0 && listOfPapers[0] != null)
            {
                listOfPapers[0].SetActive(true);
                listOfPapers[0].transform.localRotation = Quaternion.Euler(0, 0, -10);
            }
            if (listOfPapers.Count > 1 && listOfPapers[1] != null)
            {
                listOfPapers[1].SetActive(true);
                listOfPapers[1].transform.localRotation = Quaternion.Euler(0, 0, -5);
            }
            if (listOfPapers.Count > 2 && listOfPapers[2] != null)
            {
                listOfPapers[2].SetActive(true);
                listOfPapers[2].transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
        }

        if (lastLetterLow != null) lastLetterLow.gameObject.SetActive(false);
        if (lastLetterMedium != null) lastLetterMedium.gameObject.SetActive(false);
        if (lastLetterHigh != null) lastLetterHigh.gameObject.SetActive(false);
        chosen = null;
        chosenButton = null;
    }

    private void Update()
    {
        if (chosen != null && chosen.isTyping)
        {
            bool skip = false;
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                if (UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame ||
                    UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame)
                {
                    skip = true;
                }
            }
            if (!skip && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || InputReader.IsAnyClickDown()))
            {
                skip = true;
            }

            if (skip)
            {
                chosen.SkipTyping();
                EnableButton();
            }
        }
    }

    public void StyleConfirmationPopup()
    {
        if (dialogueBox == null) return;

        // Ensure Canvas overrides sorting and has a high sortingOrder so it always renders on top of all papers
        Canvas c = dialogueBox.GetComponentInParent<Canvas>();
        if (c != null)
        {
            c.overrideSorting = true;
            c.sortingOrder = 100;
        }

        Sprite cardPanel = Resources.Load<Sprite>("UI/dialogue_card_panel");
        Sprite btnNormal = Resources.Load<Sprite>("UI/dialogue_button_normal");
        Sprite btnHover = Resources.Load<Sprite>("UI/dialogue_button_hover");
        Sprite btnPressed = Resources.Load<Sprite>("UI/dialogue_button_pressed");

        Image panelImg = dialogueBox.GetComponent<Image>();
        if (panelImg != null && cardPanel != null)
        {
            BaseProfessors.StyleCardPanel(panelImg, cardPanel);
        }

        Button[] btns = dialogueBox.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < btns.Length; i++)
        {
            if (btns[i] == null) continue;
            string bName = btns[i].gameObject.name.ToLower();
            bool isYes = bName.Contains("1") || bName.Contains("yes") || i == 0;
            string txt = isYes ? "بله" : "خیر";
            BaseProfessors.StyleChoiceButton(btns[i], btnNormal, btnHover, btnPressed, txt);
            btns[i].gameObject.SetActive(true);
            btns[i].interactable = true;
        }

        if (texts != null)
        {
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i] != null)
                {
                    BaseProfessors.StyleText(texts[i]);
                }
            }
        }
    }

    public void ShowConfirmation(DraggableItem item, DropZone zone)
    {
        _currentItem = item;
        _currentZone = zone;
        
        StyleConfirmationPopup();

        if (dialogueBox != null)
        {
            dialogueBox.gameObject.SetActive(true);
            UIAnimationManager.Instance.ShowWindow(dialogueBox, 0.35f);
        }

        if (_confirmationCoroutine != null)
        {
            StopCoroutine(_confirmationCoroutine);
        }
        _confirmationCoroutine = StartCoroutine(ConfirmationTextRoutine());
    }
    
    private IEnumerator ConfirmationTextRoutine()
    {
        // 1. Immediately ensure Yes (texts[1]) and No (texts[2]) buttons are visible with high contrast
        if (texts != null && texts.Length > 1 && texts[1] != null)
        {
            var ftw1 = texts[1].GetComponent<FarsiTypewriter>();
            if (ftw1 != null) ftw1.typeWriterEffect = false;
            texts[1].color = Color.white;
            texts[1].text = LocalizationManager.IsRTL ? ArabicFixer.Fix("بله", true, true) : LocalizationManager.Translate("بله");
        }
        if (texts != null && texts.Length > 2 && texts[2] != null)
        {
            var ftw2 = texts[2].GetComponent<FarsiTypewriter>();
            if (ftw2 != null) ftw2.typeWriterEffect = false;
            texts[2].color = Color.white;
            texts[2].text = LocalizationManager.IsRTL ? ArabicFixer.Fix("خیر", true, true) : LocalizationManager.Translate("خیر");
        }

        // 2. Type out question on texts[0] ("از این تصمیم مطمئنی؟")
        if (texts != null && texts.Length > 0 && texts[0] != null)
        {
            texts[0].color = Color.white;
            string question = LocalizationManager.Get("از این تصمیم مطمئنی؟", "از این تصمیم مطمئنی؟");
            FarsiTypewriter ftw = texts[0].GetComponent<FarsiTypewriter>();
            if (ftw != null)
            {
                ftw.SetText(question);
                while (ftw.isTyping)
                {
                    if (InputReader.IsAnyClickDown())
                    {
                        ftw.SkipTyping();
                        break;
                    }
                    yield return null;
                }
            }
            else
            {
                texts[0].text = LocalizationManager.IsRTL ? ArabicFixer.Fix(question, true, true) : question;
            }
        }

        _confirmationCoroutine = null;
    }

    public void OnYesClicked()
    {
        if (_confirmationCoroutine != null)
        {
            StopCoroutine(_confirmationCoroutine);
            _confirmationCoroutine = null;
        }

        if (_currentItem != null)
        {
            bool isPass = _currentZone != null && _currentZone.isPassSlot;
            GameObject paperObj = _currentItem.gameObject;

            _currentItem.CompleteDrop();
            _counter--;
            
            UpdateDropZone(_currentItem);
            ApplyStamp(paperObj, isPass);
            StartCoroutine(UpdatePaperStackRoutine(rotationDuration));
            
            _currentItem = null;
            _currentZone = null;

            UIAnimationManager.Instance.HideWindow(dialogueBox, 0.5f);
        }
    }

    public void ApplyStamp(GameObject paper, bool isPass)
    {
        if (paper == null) return;

        // Clean up any old stamp on this paper
        Transform existing = paper.transform.Find("PaperStamp");
        if (existing != null)
        {
            Destroy(existing.gameObject);
        }

        GameObject stampObj = new GameObject("PaperStamp", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        stampObj.transform.SetParent(paper.transform, false);
        stampObj.transform.SetAsLastSibling();

        RectTransform rt = stampObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        // Account for paper's localScale so the stamp has consistent screen-pixel dimensions
        float scaleX = Mathf.Abs(paper.transform.localScale.x);
        float scaleY = Mathf.Abs(paper.transform.localScale.y);
        if (scaleX < 0.001f) scaleX = 1f;
        if (scaleY < 0.001f) scaleY = 1f;
        Vector3 targetScale = new Vector3(1f / scaleX, 1f / scaleY, 1f);

        // Stamp size tuned cleanly for the paper aspect ratio
        rt.sizeDelta = new Vector2(215f, 93f);

        float angle = Random.Range(-11f, -5f);
        rt.localRotation = Quaternion.Euler(0f, 0f, angle);

        // Paper dimensions & rotated bounds to ensure stamp NEVER spills outside paper edges
        RectTransform paperRt = paper.GetComponent<RectTransform>();
        float paperHalfW = (paperRt != null && paperRt.rect.width > 0) ? paperRt.rect.width * 0.5f : 24.5f;
        float paperHalfH = (paperRt != null && paperRt.rect.height > 0) ? paperRt.rect.height * 0.5f : 32.0f;

        float stampW = rt.sizeDelta.x * targetScale.x;
        float stampH = rt.sizeDelta.y * targetScale.y;

        float rad = Mathf.Abs(angle) * Mathf.Deg2Rad;
        float halfRotW = (stampW * Mathf.Cos(rad) + stampH * Mathf.Sin(rad)) * 0.5f;
        float halfRotH = (stampW * Mathf.Sin(rad) + stampH * Mathf.Cos(rad)) * 0.5f;

        // Safety margins inside the paper borders
        float marginX = 3.5f;
        float marginY = 4.0f;

        float maxAllowedX = Mathf.Max(0f, paperHalfW - halfRotW - marginX);
        float maxAllowedY = paperHalfH - halfRotH - marginY;
        float minAllowedY = -paperHalfH + halfRotH + marginY;

        // Keep stamp positioned nicely in upper-center area with guaranteed margins
        float minPreferredY = Mathf.Clamp(3f, minAllowedY, maxAllowedY);
        float maxPreferredY = Mathf.Clamp(13f, minPreferredY, maxAllowedY);

        float posX = Random.Range(-Mathf.Min(maxAllowedX, 3.5f), Mathf.Min(maxAllowedX, 3.5f));
        float posY = Random.Range(minPreferredY, maxPreferredY);

        rt.anchoredPosition = new Vector2(posX, posY);

        Image img = stampObj.GetComponent<Image>();
        img.raycastTarget = false;
        img.preserveAspect = true;

        Sprite sprite = isPass ? passStampSprite : failStampSprite;
        if (sprite == null)
        {
            sprite = Resources.Load<Sprite>(isPass ? "UI/stamp_pass" : "UI/stamp_fail");
        }

        if (sprite != null)
        {
            img.sprite = sprite;
            img.color = Color.white;
        }
        else
        {
            // Fallback color tint
            img.color = isPass ? new Color(0.1f, 0.65f, 0.25f, 0.95f) : new Color(0.8f, 0.15f, 0.15f, 0.95f);
        }

        StartCoroutine(AnimateStampRoutine(rt, targetScale, paper.GetComponent<RectTransform>()));
    }

    private IEnumerator AnimateStampRoutine(RectTransform stampRt, Vector3 targetScale, RectTransform paperRt)
    {
        if (stampRt == null) yield break;

        Image img = stampRt.GetComponent<Image>();
        Color baseColor = img != null ? img.color : Color.white;

        // Phase 1: Slam down from 2.8x scale in 0.12s
        float slamDuration = 0.12f;
        float elapsed = 0f;
        Vector3 startScale = targetScale * 2.8f;
        Vector3 impactScale = targetScale * 0.92f;

        while (elapsed < slamDuration)
        {
            if (stampRt == null) yield break;
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / slamDuration);
            float easeT = t * t; // Accelerating downward slam

            stampRt.localScale = Vector3.Lerp(startScale, impactScale, easeT);
            if (img != null)
            {
                img.color = new Color(baseColor.r, baseColor.g, baseColor.b, Mathf.Lerp(0.1f, 1f, t));
            }
            yield return null;
        }

        if (stampRt == null) yield break;
        stampRt.localScale = impactScale;
        if (img != null) img.color = baseColor;

        PlayStampSound();

        // Phase 2: Impact bounce & settle
        Vector2 origPaperPos = paperRt != null ? paperRt.anchoredPosition : Vector2.zero;
        if (paperRt != null)
        {
            paperRt.anchoredPosition = origPaperPos + new Vector2(0, -3f);
        }

        float reboundDuration = 0.08f;
        elapsed = 0f;
        while (elapsed < reboundDuration)
        {
            if (stampRt == null) yield break;
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / reboundDuration);

            stampRt.localScale = Vector3.Lerp(impactScale, targetScale, t);
            if (paperRt != null)
            {
                paperRt.anchoredPosition = Vector2.Lerp(origPaperPos + new Vector2(0, -3f), origPaperPos, t);
            }
            yield return null;
        }

        if (stampRt != null) stampRt.localScale = targetScale;
        if (paperRt != null) paperRt.anchoredPosition = origPaperPos;
    }

    private void PlayStampSound()
    {
        try
        {
            if (stampAudioClip != null)
            {
                AudioSource src = GetComponent<AudioSource>();
                if (src == null) src = gameObject.AddComponent<AudioSource>();
                src.PlayOneShot(stampAudioClip);
                return;
            }

            if (_soundPlayer != null)
            {
                _soundPlayer.Play("Stamp");
            }
        }
        catch {}
    }

    public void ClearAllStamps()
    {
        void RemoveFromList(List<GameObject> list)
        {
            if (list == null) return;
            foreach (var item in list)
            {
                if (item == null) continue;
                Transform stamp = item.transform.Find("PaperStamp");
                if (stamp != null)
                {
                    Destroy(stamp.gameObject);
                }
            }
        }

        RemoveFromList(listOfPapers);
        RemoveFromList(_passedPapers);
        RemoveFromList(_failedPapers);
    }

    private void UpdateDropZone(DraggableItem currentItem)
    {
        bool isPass = _currentZone != null && _currentZone.isPassSlot;
        List<GameObject> targetList = isPass ? _passedPapers : _failedPapers;

        if (targetList.Count > 0)
        {
            GameObject previousPaper = targetList[^1];
            if (previousPaper != null)
            {
                previousPaper.SetActive(false);
            }
        }
        
        targetList.Add(currentItem.gameObject);
        _passedCount = _passedPapers.Count;
    }

    private IEnumerator UpdatePaperStackRoutine(float duration)
    {
        Transform paperToRotate1 = null;
        Transform paperToRotate2 = null;
        
        _soundPlayer.Play("Paper");

        if (_currentItemIndex + 1 < listOfPapers.Count && listOfPapers[_currentItemIndex + 1] != null)
        {
            paperToRotate1 = listOfPapers[_currentItemIndex + 1].transform;
        }
        if (_currentItemIndex + 2 < listOfPapers.Count && listOfPapers[_currentItemIndex + 2] != null)
        {
            paperToRotate2 = listOfPapers[_currentItemIndex + 2].transform;
        }

        Quaternion startRot1 = (paperToRotate1 != null) ? paperToRotate1.localRotation : Quaternion.identity;
        Quaternion targetRot1 = startRot1 * Quaternion.Euler(0, 0, -5);

        Quaternion startRot2 = (paperToRotate2 != null) ? paperToRotate2.localRotation : Quaternion.identity;
        Quaternion targetRot2 = startRot2 * Quaternion.Euler(0, 0, -5);

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration); 

            if (paperToRotate1 != null)
            {
                paperToRotate1.localRotation = Quaternion.Lerp(startRot1, targetRot1, t);
            }
            if (paperToRotate2 != null)
            {
                paperToRotate2.localRotation = Quaternion.Lerp(startRot2, targetRot2, t);
            }

            yield return null;
        }

        if (paperToRotate1 != null)
        {
            paperToRotate1.localRotation = targetRot1;
        }
        if (paperToRotate2 != null)
        {
            paperToRotate2.localRotation = targetRot2;
        }

        if (_currentItemIndex + 3 < listOfPapers.Count && listOfPapers[_currentItemIndex + 3] != null)
        {
            listOfPapers[_currentItemIndex + 3].SetActive(true);
            listOfPapers[_currentItemIndex + 3].transform.localRotation = Quaternion.Euler(0, 0, 0);
        }

        _currentItemIndex++;
        
        if (counterFtw != null)
        {
            counterFtw.SetText(_papersLength + " / " + _counter);
            counterFtw.StartTyping();
        }
        
        if (_counter == 0)
        {
            if (TaskManager.Instance != null && prof != null)
            {
                TaskManager.Instance.CompleteTask(prof.TaskOrderNumber);
            }
            
            SelectFinalLetter(_passedPapers != null ? _passedPapers.Count : _passedCount);
            
            if (chosenButton != null)
            {
                var rt = chosenButton.GetComponent<RectTransform>();
                if (rt != null) UIAnimationManager.Instance.ShowWindow(rt, 0.5f);
            }
            DisableButton();
        }
    }

    private void SelectFinalLetter(int passedCount)
    {
        int lowThreshold = (counts != null && counts.Length > 0) ? counts[0] : 3;
        int medThreshold = (counts != null && counts.Length > 1) ? counts[1] : 6;

        int totalPapers = (_passedPapers != null && _failedPapers != null) ? (_passedPapers.Count + _failedPapers.Count) : _papersLength;
        int failedCount = totalPapers - passedCount;

        Debug.Log($"[PaperManager] Evaluating final result. Passed: {passedCount}, Failed: {failedCount}. Thresholds: <= {lowThreshold} (Failed too many), <= {medThreshold} (Balanced), > {medThreshold} (Passed too many)");

        // Map candidates dynamically based on their text content:
        // - Text containing "انداختید" -> The letter for failing too many students
        // - Text containing "خوب" -> The letter for a balanced pass/fail job
        // - Text containing "پاس" -> The letter for passing too many students
        (FarsiTypewriter ftw, Button btn) failedTooManyLetter = (letterHigh, lastLetterHigh);
        (FarsiTypewriter ftw, Button btn) balancedLetter = (letterMedium, lastLetterMedium);
        (FarsiTypewriter ftw, Button btn) passedTooManyLetter = (letterLow, lastLetterLow);

        var allPairs = new (FarsiTypewriter ftw, Button btn)[]
        {
            (letterLow, lastLetterLow),
            (letterMedium, lastLetterMedium),
            (letterHigh, lastLetterHigh)
        };

        foreach (var pair in allPairs)
        {
            if (pair.ftw == null) continue;
            Text t = pair.ftw.GetComponent<Text>();
            string text = t != null ? t.text : "";

            if (text.Contains("انداختید"))
            {
                failedTooManyLetter = pair;
            }
            else if (text.Contains("خوب"))
            {
                balancedLetter = pair;
            }
            else if (text.Contains("پاس"))
            {
                passedTooManyLetter = pair;
            }
        }

        if (passedCount <= lowThreshold)
        {
            // Low number of passes (e.g. 0 to 3) => The player failed too many students!
            chosen = failedTooManyLetter.ftw;
            chosenButton = failedTooManyLetter.btn;
        }
        else if (passedCount <= medThreshold)
        {
            // Balanced pass rate (e.g. 4 to 6) => Good job!
            chosen = balancedLetter.ftw;
            chosenButton = balancedLetter.btn;
        }
        else
        {
            // High number of passes (e.g. 7 to 10) => The player passed too many students!
            chosen = passedTooManyLetter.ftw;
            chosenButton = passedTooManyLetter.btn;
        }

        Debug.Log($"[PaperManager] Selected letter: {chosen?.gameObject?.name} (Button: {chosenButton?.gameObject?.name})");
    }

    public void HandleEnding()
    {
        if (task != null) UIAnimationManager.Instance.HideWindow(task, 0.5f);
        if (prof != null)
        {
            prof.CompleteMission();
        }
        else
        {
            if (_musicChange != null)
            {
                _musicChange.ToggleMusic();
                _musicChange.PlayMainMusic();
            }
            if (TaskManager.Instance != null)
            {
                TaskManager.Instance.CompleteCurrentTask();
                TaskUI.SetTopUIVisible(true);
            }
        }
    }

    public void OnNoClicked()
    {
        if (_confirmationCoroutine != null)
        {
            StopCoroutine(_confirmationCoroutine);
            _confirmationCoroutine = null;
        }

        if (_currentItem != null)
        {
            _currentItem.CancelDrop();
        }

        _currentItem = null;
        _currentZone = null;
        
        UIAnimationManager.Instance.HideWindow(dialogueBox, 0.5f);
    }

    private void DisableButton()
    {
        if (chosenButton != null)
        {
            chosenButton.interactable = false;
        }
        
        chosen.StartTyping();
    }
    
    private void EnableButton()
    {
        if (chosenButton != null)
        {
            chosenButton.interactable = true;
        }
    }
}
