using System;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class ShowCollider : MonoBehaviour
{

    public CinemachineCamera cam;
    private void OnDrawGizmos()
    {
        if (cam == null) return;

        Gizmos.color = Color.green;

        // Only works for orthographic (2D) camera
        float height = cam.Lens.OrthographicSize * 2f;
        float width = height * cam.Lens.Aspect;
        Vector3 camPos = cam.transform.position;

        // Draw rectangle at camera position
        Gizmos.DrawWireCube(camPos, new Vector3(width, height, 0f));
    }
}
