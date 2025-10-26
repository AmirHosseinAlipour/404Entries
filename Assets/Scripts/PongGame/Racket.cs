using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Racket : MonoBehaviour
{
    public Rigidbody2D rb;
    public float moveSpeed;
    public string AxesName;
    public int Score { get; private set; }
    public Text scoreText;
    public int[] scoreMilestones;
    public PongManager fadeActivator;
    private int currentSpriteIndex = 0;
    protected Ball ball;
    [Header("WinOption")]
    public int targetScore = 10;
    public GameObject popUpObject; 
    public float scaleSpeed = 5f;  
    public float maxScale = 10f;
    private void Start()
    {
        ball = GameObject.Find("Ball").GetComponent<Ball>();
    }
    void FixedUpdate()
    {
        Movement();

        #region AnotherMovementOption
        //if (Input.GetKey("w") || Input.GetKey(KeyCode.UpArrow))
        //{
        //    rb.transform.position += Vector3.up;
        //}

        //if (Input.GetKey("s") || Input.GetKey(KeyCode.DownArrow))
        //{
        //    rb.transform.position += Vector3.down;
        //}
        #endregion

    }

    protected abstract void Movement();

    public void GetScore(){
        Score++;
        scoreText.text = Score.ToString();
        CheckForBallSpriteChange();
        Debug.Log(Score);
        if (Score >= targetScore)
        {
            if (popUpObject != null)
                StartCoroutine(ScalePopUp());            fadeActivator.FadeOutAndDeactivate();
            PlayerController _player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
            _player.isUIActive = false;
        }
    }
    private void CheckForBallSpriteChange()
    {
        if (currentSpriteIndex < scoreMilestones.Length &&
            Score >= scoreMilestones[currentSpriteIndex])
        {
            ball.ChangeSprite(currentSpriteIndex);
            currentSpriteIndex++;
        }
    }
    private IEnumerator ScalePopUp()
    {
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

        popUpObject.SetActive(false);
    }


}
