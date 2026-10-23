using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AsgharManager : MonoBehaviour
{
    public TeacherController teacher;
    public PlayerController player;
    public GameObject fadePanel; 
    public float fadeDuration = 1f;
    public GameObject RespawnPoint;
    private CanvasGroup canvasGroup;
    

    public bool _win = false;

    private void Awake()
    {
        if (fadePanel != null)
        {
            canvasGroup = fadePanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = fadePanel.AddComponent<CanvasGroup>();
        }
        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) player = playerObj.GetComponent<PlayerController>();
    }

    private bool _isRestarting = false;

    private void Update()
    {
        if (teacher != null && player != null && teacher.isFacingPlayer && player.IsMoving() && !_win && !_isRestarting)
        {
            StartCoroutine(RestartGame());
        }
    }

    public void WinGame()
    {
        _win = true;
        StartCoroutine(WinRoutine());
    }

    private IEnumerator RestartGame()
    {
        _isRestarting = true;
        if (player != null)
        {
            player.isUIActive = true;
        }

        yield return StartCoroutine(FadeIn(fadeDuration));

        if (player != null && RespawnPoint != null)
        {
            player.Respawn(RespawnPoint.transform.position);
        }

        yield return StartCoroutine(FadeOut(fadeDuration));

        if (player != null)
        {
            player.isUIActive = false;
        }

        _isRestarting = false;
    }

    private IEnumerator WinRoutine()
    {
        if (fadePanel != null) fadePanel.SetActive(true);
        if (teacher != null) teacher.isFacingPlayer = false; 
        yield return new WaitForSeconds(fadeDuration);
        Debug.Log("You escaped the class!");
    }
    public IEnumerator FadeIn(float duration)
    {
        if (canvasGroup == null) yield break;
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, time / duration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }
    public IEnumerator FadeOut(float duration)
    {
        if (canvasGroup == null) yield break;
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, time / duration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
    }

}