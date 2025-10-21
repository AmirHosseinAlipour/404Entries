using System.Collections;
using UnityEngine;

public class TeacherController : MonoBehaviour
{
    public Animator animator;
    public bool isFacingPlayer = false; 

    [Header("Random Rotation Settings")]
    public float minWait = 2f;
    public float maxWait = 5f;

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

            
            isFacingPlayer = !isFacingPlayer;

            if (isFacingPlayer)
                animator.SetTrigger("FacingFront");
            else
                animator.SetTrigger("FacingBack");  
        }
    }

    public void StopLooking()
    {
        _isActive = false;
        if (lookRoutine != null)
            StopCoroutine(lookRoutine);
        animator.Play("FaocingBack");
    }
}