using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MiniMapCamera : MonoBehaviour
{
    public RenderTexture miniMapTexture; // Drag your Render Texture asset here

    private Transform _playerTransform;
    private float _initialZPosition;
    private Camera _camera;

    // Use Awake() to get components
    private void Awake()
    {
        _camera = GetComponent<Camera>();
        
        // --- THIS IS THE CRITICAL FIX ---
        // Manually set the camera's aspect ratio to match the Render Texture's.
        // We do this in Awake() AND LateUpdate() to ensure it's never overwritten.
        SetCameraAspect();
    }

    private void Start()
    {
        _playerTransform = GameObject.FindWithTag("Player").transform;
        _initialZPosition = transform.position.z;
        transform.rotation = Quaternion.identity;
    }

    // Use LateUpdate() to ensure this runs AFTER the player has moved
    private void LateUpdate()
    {
        // 1. Follow the player
        Vector3 newPos = _playerTransform.position;
        newPos.z = _initialZPosition;
        transform.position = newPos;

        // 2. --- THIS IS THE CRITICAL FIX ---
        // Re-apply the aspect ratio EVERY frame. This overrides Unity's
        // automatic (and incorrect) screen-matching behavior.
        SetCameraAspect();
    }

    void SetCameraAspect()
    {
        if (miniMapTexture != null)
        {
            // Calculate the aspect ratio from the Render Texture's dimensions
            float aspect = (float)miniMapTexture.width / (float)miniMapTexture.height;
            
            // Apply it to the camera
            _camera.aspect = aspect;
        }
        else if (_camera.targetTexture != null)
        {
            // Fallback in case you assigned it in the camera's TargetTexture slot
            // but not in the script's public 'miniMapTexture' slot
            float aspect = (float)_camera.targetTexture.width / (float)_camera.targetTexture.height;
            _camera.aspect = aspect;
        }
        else
        {
            Debug.LogError("MiniMapCamera: Render Texture is not assigned!");
        }
    }
}