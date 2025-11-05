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
    private SoundPlayer soundPlayer;

    protected override void Awake()
    {
        base.Awake();
        soundPlayer = GetComponent<SoundPlayer>();
    }

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
                //Green light
                isFacingPlayer = false; 
                soundPlayer.Play("GreenLight");
                // Player is safe immediately
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
        soundPlayer.Play("RedLight");
        //
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