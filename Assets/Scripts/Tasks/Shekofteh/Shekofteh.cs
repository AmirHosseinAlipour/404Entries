using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Shekofteh : BaseProfessors
{
    public RectTransform UICharacterRectTransform;
    public float UICharacterMoveSpeed = 800f;
    public SkipButton SkipButton;
    
    // Private fields
    private Animator _UIAnimator;
    private Vector2 _targetStage1;
    private Vector2 _targetStage2;

    protected override void Start()
    {
        base.Start();
        
        // UI Shekofteh + Initial movement
        _UIAnimator = UICharacterRectTransform.GetComponent<Animator>();

        RectTransform parentRect = UICharacterRectTransform.parent.GetComponent<RectTransform>();
        Vector2 parentSize = parentRect.rect.size;

        float halfParentWidth = parentSize.x / 2f;
        float halfParentHeight = parentSize.y / 4f;

        float playerWidth = UICharacterRectTransform.sizeDelta.x * UICharacterRectTransform.localScale.x;

        float targetX = -halfParentWidth + (playerWidth / 2f);
        float targetY = halfParentHeight; 

        _targetStage1 = new Vector2(targetX, UICharacterRectTransform.anchoredPosition.y); 
        _targetStage2 = new Vector2(targetX, targetY);
    }

    public void StartMission()
    {
        StartCoroutine(ExecuteMovement());
        
    }
    
    // Move Shekofteh methods
    private IEnumerator ExecuteMovement()
    {
        yield return StartCoroutine(MoveObject(_targetStage1));
        yield return StartCoroutine(MoveObject(_targetStage2));

        // For starting the the skip teleportation
        yield return new WaitForSeconds(1f);
        SkipButton.gameObject.SetActive(true);
        SkipButton.StartTeleportMovement(); 
    }
    
    private IEnumerator MoveObject(Vector2 targetPosition)
    {
        const float threshold = 1f; 

        while (Vector2.Distance(UICharacterRectTransform.anchoredPosition, targetPosition) > threshold)
        {
            Vector2 newPosition = Vector2.MoveTowards(
                UICharacterRectTransform.anchoredPosition, 
                targetPosition, 
                UICharacterMoveSpeed * Time.deltaTime
            );

            UICharacterRectTransform.anchoredPosition = newPosition;
            yield return null;
        }

        UICharacterRectTransform.anchoredPosition = targetPosition;
    }
}
