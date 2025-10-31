using System.Collections;
using UnityEngine;

public class TeacherController : BaseProfessors
{
    public Animator animator;
    public bool isFacingPlayer = false;

    [Header("Random Rotation Settings")]
    public float minWait = 1.5f;
    public float maxWait = 4f;

    private bool _isActive = false; 
    private Coroutine lookRoutine;

    private void Start()
    {
        animator.Play("FacingBack");
    }

    public void StartLooking()
    {
        if (_isActive) return;
        _isActive = true;
        lookRoutine = StartCoroutine(RandomLookRoutine());
    }

    private IEnumerator RandomLookRoutine()
    {
        while (_isActive)
        {
            yield return new WaitForSeconds(Random.Range(minWait, maxWait));
        
            if (isFacingPlayer)
            {
                // Player is safe immediately
                isFacingPlayer = false; 
                animator.SetTrigger("FacingBack");
            }
            else
            {
                // Just trigger the animation.
                // The Animation Event will call OnFacingFrontComplete() when it's done.
                animator.SetTrigger("FacingFront");
            }
        }
    }
    
    public void OnFacingFrontComplete()
    {
        isFacingPlayer = true;
    }

    public void StopLooking()
    {
        _isActive = false;
        if (lookRoutine != null)
            StopCoroutine(lookRoutine);
        animator.Play("FaocingBack");
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        //ignore the base method!
    }
}