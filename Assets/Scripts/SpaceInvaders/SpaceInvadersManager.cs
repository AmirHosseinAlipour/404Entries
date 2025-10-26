using UnityEngine;

public class SpaceInvadersManager : MonoBehaviour
{
    [Header("FadeSetting")]
    public GameObject gameObjectsToShow; 
    public RectTransform gameObjectsToHide;
    public float fadeSpeed = 2f;

    [Header("Enemy Setting")]
    public int totalEnemies = 10; 

    private Renderer[] renderers; 
    [SerializeField] private bool isFadingIn = false;
    private bool isFadingOut = false;
    [Header("Music")]
    public GameObject oneShotAudioPrefab;
    public AudioClip WindowsXp;
    void Awake()
    {
        if (gameObjectsToShow != null)
        {
            renderers = gameObjectsToShow.GetComponentsInChildren<Renderer>();

            
            gameObjectsToShow.SetActive(false);
        }
    }

    void Update()
    {
        if (isFadingIn)
        {
            bool done = FadeTo(1f);
            if (done) isFadingIn = false;
        }
        else if (isFadingOut)
        {
            UIAnimationManager.Instance.HideWindow(gameObjectsToHide , 0.5f );
            bool done = FadeTo(0f);
            if (done)
            {
                isFadingOut = false;
                gameObject.SetActive(false); 
            }
        }
    }

    
    public void PlayerEnteredTrigger()
    {
        if (gameObjectsToShow != null)
        {
            gameObjectsToShow.SetActive(true); 
            SetAlpha(0f);                      
            isFadingIn = true;   
            GameObject XP = Instantiate(oneShotAudioPrefab, transform.position, Quaternion.identity);
            XP.GetComponent<OneShotSound>().Play(WindowsXp);
        }
    }

    
    public void EnemyKilled()
    {
        totalEnemies--;
        if (totalEnemies <= 0)
        {
            isFadingOut = true;
        }
    }

    
    private bool FadeTo(float targetAlpha)
    {
        bool allDone = true;
        foreach (var rend in renderers)
        {
            Color c = rend.material.color;
            float alpha = Mathf.MoveTowards(c.a, targetAlpha, fadeSpeed * Time.deltaTime);
            if (!Mathf.Approximately(alpha, c.a))
                allDone = false;
            c.a = alpha;
            rend.material.color = c;
        }
        return allDone;
    }

    private void SetAlpha(float a)
    {
        foreach (var rend in renderers)
        {
            Color c = rend.material.color;
            c.a = a;
            rend.material.color = c;
        }
    }
}
