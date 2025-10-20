using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Shekofteh : BaseProfessors
{
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
    public FarsiTypewriter mission;

    public int initialDialogueCount;
    public int nextLevelWaitDuration;
    
    private bool _isNextLevelRunning = false;

    // To make it visible after movement
    public GameObject counter;

    [Header("Text Sequence")] 
    public Text[] listOfTexts;
    
    private Animator _UIAnimator;
    
    protected override void Start()
    {
        base.Start();
        
        _UIAnimator = UICharacterRectTransform.GetComponent<Animator>();
        
        SkipButton.OnLevelGoalReached += HandleLevelGoalReached;
    }
    
    private void HandleLevelGoalReached()
    {
        if (!_isNextLevelRunning)
        {
            StartCoroutine(NextLevel());
        }
    }

    public void StartMission()
    {
        StartCoroutine(ExecuteSequence());
    }
    
    private IEnumerator ExecuteSequence()
    {
        yield return StartCoroutine(MoveObject(endPos1.position));
        yield return StartCoroutine(MoveObject(endPos2.position));
        counter.SetActive(true);

        yield return new WaitForSeconds(1f);

        textPanel.transform.parent.gameObject.SetActive(true);
        textPanel.GetComponent<FarsiTypewriter>().StartTyping();
        
        SkipButton.gameObject.SetActive(true);
        buttonBackground.gameObject.SetActive(true);
        
        UIUtils.SetAlpha(SkipButton.buttonImage.gameObject, 0);
        UIUtils.SetAlpha(buttonBackground.gameObject, 0);
        
        SkipButton.StartTeleportMovement();
    }

    private IEnumerator MoveObject(Vector3 targetWorldPosition)
    {
        Vector3 direction = (targetWorldPosition - UICharacterRectTransform.position).normalized;

        if (_UIAnimator != null)
        {
            _UIAnimator.SetFloat("MoveX", direction.x);
            _UIAnimator.SetFloat("MoveY", direction.y);
            _UIAnimator.SetFloat("MoveMagnitude", 1f);
        }
        
        while (Vector3.Distance(UICharacterRectTransform.position, targetWorldPosition) > 1f)
        {
            UICharacterRectTransform.position = Vector3.MoveTowards(
                UICharacterRectTransform.position,
                targetWorldPosition,
                UICharacterMoveSpeed * Time.deltaTime
            );

            yield return null;
        }

        if (_UIAnimator != null)
        {
            _UIAnimator.SetFloat("MoveMagnitude", 0f);
        }

        UICharacterRectTransform.position = targetWorldPosition;
    }

    // Override this method do implement text sequence execution
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);

        StartCoroutine(TextSequence());
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            for (int i = 0; i < 3; i++)
            {
                // Reset the text so each time the text would be written in type writer effect!
                listOfTexts[i].text = "";
            }
        }
    }

    private IEnumerator TextSequence()
    {
        // First of all we write the first 3 text which is the accept message + yes/no option
        for (int i = 0; i < initialDialogueCount; i++)
        {
            FarsiTypewriter text = listOfTexts[i].gameObject.GetComponent<FarsiTypewriter>();
            text.StartTyping();
            yield return new WaitForSeconds(text.typeCharTime * text.len);
        }
    }

    private IEnumerator NextLevel()
    {
        _isNextLevelRunning = true;

        float originalSpeed = mission.typeCharTime;
        mission.typeCharTime /= 5;

        UIUtils.SetAlpha(SkipButton.gameObject, 0f);
        UIUtils.SetAlpha(buttonBackground.gameObject, 0f);
        
        yield return new WaitForSecondsRealtime(nextLevelWaitDuration);
        mission.typeCharTime = originalSpeed;
        UIUtils.SetAlpha(SkipButton.gameObject, 1f);
        UIUtils.SetAlpha(buttonBackground.gameObject, 1f);

        SkipButton.NextLevel();
        _isNextLevelRunning = false;
    }
}