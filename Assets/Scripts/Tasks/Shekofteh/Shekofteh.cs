using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class Shekofteh : BaseProfessors
{
    public override ProfessorUIType UIType => ProfessorUIType.ScreenSpaceUI;

    [Header("Character")]
    public RectTransform UICharacterRectTransform;
    public float UICharacterMoveSpeed = 800f;
    
    [Header("Positions")]
    public RectTransform endPos1;
    public RectTransform endPos2;

    [Header("Mission")]
    public GameObject textPanel;
    public SkipButton SkipButton;
    public CircleTimerAnimation buttonBackground;
    public SpeechTypeWriterShekofteh mission;
    public float nextLevelWaitDuration = 1.2f;
    
    private bool _isNextLevelRunning = false;
    private bool _isMissionCompleted = false;

    [Header("Others")]
    public GameObject counter;
    public VideoPlayer videoPlayer;
    public GameObject videoRnderer;
    
    private Animator _UIAnimator;
    private Vector3 _characterStartPosition;
    
    private SoundPlayer _soundPlayer;
    private MusicChange _musicChange;

    protected override void Awake()
    {
        base.Awake();

        if (UICharacterRectTransform != null)
            _characterStartPosition = UICharacterRectTransform.position;
        _soundPlayer = GetComponent<SoundPlayer>();
        
        if (mission != null)
            mission.OnTypingFinished += MissionFailed;
        _musicChange = GetComponent<MusicChange>();

        if (videoRnderer != null)
        {
            RawImage r = videoRnderer.GetComponent<RawImage>();
            if (r != null)
            {
                r.raycastTarget = false;
                Color c = r.color;
                c.a = 0f;
                r.color = c;
            }
        }
    }
    
    protected override void Start()
    {
        base.Start();
        
        if (UICharacterRectTransform != null)
            _UIAnimator = UICharacterRectTransform.GetComponent<Animator>();
        
        if (SkipButton != null)
        {
            SkipButton.OnLevelGoalReached += HandleLevelGoalReached;
            SkipButton.OnMissionComplete += HandleEnding;
        }

        HideMinigameUI();
    }

    public override void HideMinigameUI()
    {
        base.HideMinigameUI();
        if (counter != null) counter.SetActive(false);
        if (textPanel != null)
        {
            if (textPanel.transform.parent != null) textPanel.transform.parent.gameObject.SetActive(false);
            textPanel.SetActive(false);
        }
        if (SkipButton != null) SkipButton.gameObject.SetActive(false);
        if (buttonBackground != null) buttonBackground.gameObject.SetActive(false);
    }

    public override void AcceptChallenge()
    {
        StartMission();
    }

    private void OnDestroy()
    {
        if (mission != null)
            mission.OnTypingFinished -= MissionFailed;

        if (SkipButton != null)
        {
            SkipButton.OnLevelGoalReached -= HandleLevelGoalReached;
            SkipButton.OnMissionComplete -= HandleEnding;
        }
    }

    private void HandleLevelGoalReached()
    {
        if (!_isNextLevelRunning && !_isMissionCompleted)
        {
            StartCoroutine(NextLevel());
        }
    }

    private void HandleEnding()
    {
        _isMissionCompleted = true;
        _isNextLevelRunning = false;
        StopAllCoroutines();

        if (SkipButton != null)
        {
            SkipButton.StopTeleportMovement();
            SkipButton.gameObject.SetActive(false);
        }

        if (buttonBackground != null)
        {
            buttonBackground.StopCountdown();
            buttonBackground.gameObject.SetActive(false);
        }

        if (textPanel != null)
        {
            var st = textPanel.GetComponent<SpeechTypeWriterShekofteh>();
            if (st != null) st.StopTyping();
            if (textPanel.transform.parent != null) textPanel.transform.parent.gameObject.SetActive(false);
        }

        if (counter != null) counter.SetActive(false);
        if (videoPlayer != null) videoPlayer.Stop();

        if (videoRnderer != null)
        {
            RawImage r = videoRnderer.GetComponent<RawImage>();
            if (r != null)
            {
                r.raycastTarget = false;
                Color c = r.color;
                c.a = 0f;
                r.color = c;
            }
        }

        if (StartPanel != null)
        {
            UIAnimationManager.Instance.HideWindow(StartPanel, 0.5f);
        }

        CompleteMission();
    }

    public void StartMission()
    {
        StartMinigame();
        _isMissionCompleted = false;
        _isNextLevelRunning = false;
        StartCoroutine(ExecuteSequence());
    }
    
    private IEnumerator ExecuteSequence()
    {
        if (StartPanel != null)
        {
            StartPanel.gameObject.SetActive(true);
            UIUtils.SetAlpha(StartPanel.gameObject, 1f);
        }

        if (counter != null)
        {
            counter.SetActive(true);
            UIUtils.SetAlpha(counter, 1f);
        }

        if (textPanel != null)
        {
            if (textPanel.transform.parent != null)
            {
                textPanel.transform.parent.gameObject.SetActive(true);
                UIUtils.SetAlpha(textPanel.transform.parent.gameObject, 1f);
            }
            textPanel.SetActive(true);
            UIUtils.SetAlpha(textPanel, 1f);
        }

        if (SkipButton != null)
        {
            SkipButton.gameObject.SetActive(true);
            UIUtils.SetAlpha(SkipButton.gameObject, 1f);
            SkipButton.ResetState();
        }

        if (buttonBackground != null)
        {
            buttonBackground.gameObject.SetActive(true);
            UIUtils.SetAlpha(buttonBackground.gameObject, 1f);
        }

        Canvas.ForceUpdateCanvases();

        yield return new WaitForSeconds(0.6f);

        if (_musicChange != null)
        {
            _musicChange.ToggleMusic();
            _musicChange.PlayThemeMusic();
        }

        if (StartPanel != null)
        {
            StartPanel.gameObject.SetActive(true);
            UIUtils.SetAlpha(StartPanel.gameObject, 1f);
        }

        if (textPanel != null)
        {
            if (textPanel.transform.parent != null)
            {
                textPanel.transform.parent.gameObject.SetActive(true);
                UIUtils.SetAlpha(textPanel.transform.parent.gameObject, 1f);
            }
            textPanel.SetActive(true);
            UIUtils.SetAlpha(textPanel, 1f);

            Canvas.ForceUpdateCanvases();

            var typewriter = textPanel.GetComponent<SpeechTypeWriterShekofteh>();
            if (typewriter != null)
            {
                typewriter.StartTyping();
            }
        }
        
        if (SkipButton != null)
        {
            SkipButton.gameObject.SetActive(true);
            SkipButton.StartTeleportMovement();
        }
    }

    private IEnumerator NextLevel()
    {
        _isNextLevelRunning = true;

        if (SkipButton != null)
        {
            SkipButton.StopTeleportMovement();
            UIUtils.SetAlpha(SkipButton.gameObject, 0f);
        }
        if (buttonBackground != null)
        {
            buttonBackground.StopCountdown();
            UIUtils.SetAlpha(buttonBackground.gameObject, 0f);
        }

        float originalSpeed = (mission != null) ? mission.typeCharTime : 0.05f;
        if (mission != null)
        {
            mission.typeCharTime = Mathf.Max(0.015f, originalSpeed / 3f);
        }

        if (_soundPlayer != null) _soundPlayer.Play("Skip");

        float waitDuration = (nextLevelWaitDuration > 0.1f) ? Mathf.Min(nextLevelWaitDuration, 1.2f) : 1.2f;

        if (videoRnderer != null && videoPlayer != null)
        {
            RawImage r = videoRnderer.GetComponent<RawImage>();
            if (r != null)
            {
                r.raycastTarget = false;
                Color c = r.color;
                c.a = 0.5f;
                r.color = c;
                videoPlayer.Play();
                yield return new WaitForSecondsRealtime(waitDuration);
                c.a = 0f;
                r.color = c;
            }
            else
            {
                yield return new WaitForSecondsRealtime(waitDuration);
            }
        }
        else
        {
            yield return new WaitForSecondsRealtime(waitDuration);
        }

        if (mission != null)
        {
            mission.typeCharTime = originalSpeed;
        }

        if (!_isMissionCompleted)
        {
            if (SkipButton != null)
            {
                SkipButton.NextLevel();
                if (!SkipButton.isCompleted)
                {
                    SkipButton.gameObject.SetActive(true);
                    UIUtils.SetAlpha(SkipButton.gameObject, 1f);
                    if (buttonBackground != null)
                    {
                        buttonBackground.gameObject.SetActive(true);
                        UIUtils.SetAlpha(buttonBackground.gameObject, 1f);
                    }
                    SkipButton.StartTeleportMovement();
                }
            }
        }

        _isNextLevelRunning = false;
    }

    private void MissionFailed()
    {
        if (_isMissionCompleted || _isNextLevelRunning) return;

        StopAllCoroutines();
    
        if (mission != null) mission.StopTyping();
        if (videoPlayer != null) videoPlayer.Stop();
        
        if (textPanel != null && textPanel.transform.parent != null)
            textPanel.transform.parent.gameObject.SetActive(false);

        if (SkipButton != null)
        {
            SkipButton.StopTeleportMovement();
            SkipButton.gameObject.SetActive(false);
        }

        if (buttonBackground != null)
        {
            buttonBackground.StopCountdown();
            buttonBackground.gameObject.SetActive(false);
        }

        if (counter != null) counter.SetActive(false);

        if (videoRnderer != null)
        {
            RawImage r = videoRnderer.GetComponent<RawImage>();
            if (r != null)
            {
                r.raycastTarget = false;
                Color c = r.color;
                c.a = 0f;
                r.color = c;
            }
        }
        
        if (UICharacterRectTransform != null)
            UICharacterRectTransform.position = _characterStartPosition;

        _isNextLevelRunning = false;

        HandleLoose();
    }
    
    public override void ApplyModernVisualStyles()
    {
        base.ApplyModernVisualStyles();

        Sprite cardPanel = Resources.Load<Sprite>("UI/dialogue_card_panel");
        if (textPanel != null && cardPanel != null)
        {
            Image panelImg = textPanel.GetComponent<Image>();
            if (panelImg != null)
            {
                panelImg.sprite = cardPanel;
                panelImg.type = Image.Type.Sliced;
                panelImg.color = Color.white;
            }
        }
    }

    private void HandleLoose()
    {
        if (StartPanel != null) UIAnimationManager.Instance.HideWindow(StartPanel, 0.5f);
        if (acceptRect != null) UIAnimationManager.Instance.ShowWindow(acceptRect, 0.01f);
        FailMission();
    }
}