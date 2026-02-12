using Unity.Cinemachine;
using UnityEngine;

public class SpaceInvadersManager : MonoBehaviour
{
    public GameObject MiniMap; 
    [Header("Camera Settings")] 
    public CinemachineCamera cam;
    public SpriteRenderer background;
    
    [Header("FadeSetting")]
    public GameObject gameObjectsToShow; 
    public GameObject gameObjectsToHide;
    public float fadeSpeed = 2f;
    public RectTransform[] ErorWindow;
    
    [Header("Enemy Setting")]
    public int totalEnemies = 10;

    [Header("Professor")] public Mazaheri m;
    
    
    private Renderer[] renderers; 
    [SerializeField] private bool isFadingIn = false;
    private bool isFadingOut = false;
    [Header("Music")]
    public GameObject oneShotAudioPrefab;
    public AudioClip WindowsXp;
    private MusicChange musicChange;
    
    public void InitialSettings()
    {
        cam.Priority = 20;
        ScaleSpriteToCamera();
    }
    void Awake()
    {
        if (gameObjectsToShow != null)
        {
            renderers = gameObjectsToShow.GetComponentsInChildren<Renderer>();
            
            gameObjectsToShow.SetActive(false);
        }

        musicChange = GetComponent<MusicChange>();
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
           
            gameObjectsToHide.SetActive(false);
            gameObjectsToShow.SetActive(false);
            cam.Priority = 0;
            bool done = FadeTo(0f);
            if (done)
            {
                isFadingOut = false;
            }
           

            TaskManager.Instance.CompleteTask(m.TaskOrderNumber);
            MiniMap.SetActive(true);
            m.HandleEnding();
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

    public void ShowErorwindow(int i )
    {
        UIAnimationManager.Instance.ShowWindow(ErorWindow[i] , 0.5f );
    }

    public void HideErorwindow(int i)
    {
        UIAnimationManager.Instance.HideWindow(ErorWindow[i] , 0.5f);
    }
    
    private void ScaleSpriteToCamera()
    {
        float cameraHeight = cam.Lens.OrthographicSize * 2f;
        float cameraWidth = cameraHeight * cam.Lens.Aspect;

        background.gameObject.SetActive(true);
    
        float spriteHeight = background.sprite.bounds.size.y;
        float spriteWidth = background.sprite.bounds.size.x;

        float scaleX = cameraWidth / spriteWidth;
        float scaleY = cameraHeight / spriteHeight;

        // Apply the scales independently to stretch the sprite
        background.transform.localScale = new Vector3(scaleX, scaleY, 1f);
    }
}
