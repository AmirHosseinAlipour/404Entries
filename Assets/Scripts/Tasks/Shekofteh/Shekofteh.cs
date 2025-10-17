using System.Collections;
using UnityEngine;

public class Shekofteh : BaseProfessors
{
    [Header("Character")]
    public RectTransform UICharacterRectTransform;
    public float UICharacterMoveSpeed = 800f;
    
    [Header("Positions")]
    public RectTransform endPos1;
    public RectTransform endPos2;
    
    [Header("Skip Button")]
    public SkipButton SkipButton;
    public CircleTimerAnimation buttonBackground;

    // To make it visible after movement
    public GameObject counter;
    
    private Animator _UIAnimator;

    protected override void Start()
    {
        base.Start();
        
        _UIAnimator = UICharacterRectTransform.GetComponent<Animator>();
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
        SkipButton.gameObject.SetActive(true);
        buttonBackground.gameObject.SetActive(true);
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
}