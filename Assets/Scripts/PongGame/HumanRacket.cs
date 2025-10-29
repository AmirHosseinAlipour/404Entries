using System.Collections;
using UnityEngine;

public class HumanRacket : Racket
{
    [Header("WinOption")]
    public int targetScore = 10;
    public GameObject popUpObject; 
    public float scaleSpeed = 5f;  
    public float maxScale = 10f;
    private bool isScaling = false;

    protected override void Movement()
    {
        float moveAxesValue = 0f;

        
        if (Input.GetKey("w") || Input.GetKey(KeyCode.UpArrow))
            moveAxesValue = 1f;
        else if (Input.GetKey("s") || Input.GetKey(KeyCode.DownArrow))
            moveAxesValue = -1f;

        // کنترل با تاچ برای موبایل
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // نصف بالایی صفحه → بالا
            if (touch.position.y > Screen.height / 2)
                moveAxesValue = 1f;
            // نصف پایینی صفحه → پایین
            else
                moveAxesValue = -1f;
        }

        rb.linearVelocity = new Vector2(0f, moveAxesValue * moveSpeed);
    }

    public override void GetScore()
    {
        base.GetScore();
        CheckForBallSpriteChange();
        Debug.Log(Score);
        if (Score >= targetScore)
        {
            if (popUpObject != null) StartCoroutine(ScalePopUp());    
            
        }
    }
    private IEnumerator ScalePopUp()
    {
        if (isScaling) yield break; 
        isScaling = true;

        popUpObject.SetActive(true);

        Vector3 startScale = Vector3.zero;
        Vector3 targetScale = Vector3.one * maxScale;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * scaleSpeed;
            popUpObject.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * scaleSpeed;
            popUpObject.transform.localScale = Vector3.Lerp(targetScale, startScale, t);
            yield return null;
        }

        fadeActivator.FadeOutAndDeactivate();
        PlayerController _player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        _player.isUIActive = false;

        isScaling = false; 
    }
}