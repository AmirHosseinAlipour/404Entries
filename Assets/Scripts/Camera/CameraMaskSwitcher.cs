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
        // Start with normal mask
        mainCamera.cullingMask = normalMask;
    }

    private void Update()
    {
        if (roomVcam != null) // Null safety
        {
            // If room camera is currently the active vcam
            if (roomVcam.Priority > 10)
                mainCamera.cullingMask = roomMask;
            else
                mainCamera.cullingMask = normalMask;
        }
    }
}