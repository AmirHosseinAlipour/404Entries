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
        if (cam != null) cam.Priority = 20;
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
            if (gameObjectsToHide != null) gameObjectsToHide.SetActive(false);
            if (gameObjectsToShow != null) gameObjectsToShow.SetActive(false);
            if (cam != null) cam.Priority = 0;
            bool done = FadeTo(0f);
            if (done)
            {
                isFadingOut = false;
                if (TaskManager.Instance != null && m != null)
                {
                    TaskManager.Instance.CompleteTask(m.TaskOrderNumber);
                }
                if (MiniMap != null) MiniMap.SetActive(true);
                if (m != null) m.HandleEnding();
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
            if (oneShotAudioPrefab != null)
            {
                GameObject XP = Instantiate(oneShotAudioPrefab, transform.position, Quaternion.identity);
                var oss = XP.GetComponent<OneShotSound>();
                if (oss != null) oss.Play(WindowsXp);
            }
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
        if (renderers == null || renderers.Length == 0) return true;
        bool allDone = true;
        foreach (var rend in renderers)
        {
            if (rend == null || rend.material == null) continue;
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
        if (renderers == null) return;
        foreach (var rend in renderers)
        {
            if (rend == null || rend.material == null) continue;
            Color c = rend.material.color;
            c.a = a;
            rend.material.color = c;
        }
    }

    public void ShowErorwindow(int i)
    {
        if (ErorWindow != null && i >= 0 && i < ErorWindow.Length && ErorWindow[i] != null)
        {
            UIAnimationManager.Instance.ShowWindow(ErorWindow[i], 0.5f);
        }
    }

    public void HideErorwindow(int i)
    {
        if (ErorWindow != null && i >= 0 && i < ErorWindow.Length && ErorWindow[i] != null)
        {
            UIAnimationManager.Instance.HideWindow(ErorWindow[i], 0.5f);
        }
    }
    
    private void ScaleSpriteToCamera()
    {
        if (cam == null || background == null || background.sprite == null) return;
        float cameraHeight = cam.Lens.OrthographicSize * 2f;
        float cameraWidth = cameraHeight * cam.Lens.Aspect;

        background.gameObject.SetActive(true);
    
        float spriteHeight = background.sprite.bounds.size.y;
        float spriteWidth = background.sprite.bounds.size.x;

        if (spriteHeight <= 0.001f || spriteWidth <= 0.001f) return;

        float scaleX = cameraWidth / spriteWidth;
        float scaleY = cameraHeight / spriteHeight;

        // Apply the scales independently to stretch the sprite
        background.transform.localScale = new Vector3(scaleX, scaleY, 1f);
    }
}
