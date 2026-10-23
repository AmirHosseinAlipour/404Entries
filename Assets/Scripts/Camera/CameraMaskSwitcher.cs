using UnityEngine;
using Unity.Cinemachine;

public class CameraMaskSwitcher : MonoBehaviour
{
    public static CameraMaskSwitcher Instance {  get; private set; }
    
    public Camera mainCamera;
    public CinemachineCamera roomVcam;
    public LayerMask normalMask;
    public LayerMask roomMask;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera != null)
            mainCamera.cullingMask = normalMask;
    }

    private void Update()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        if (roomVcam != null)
        {
            LayerMask targetMask = (roomVcam.Priority > 10) ? roomMask : normalMask;
            if (mainCamera.cullingMask != targetMask)
            {
                mainCamera.cullingMask = targetMask;
            }
        }
    }
}