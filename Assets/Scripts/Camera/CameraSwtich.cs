using System;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class CameraSwitch : MonoBehaviour
{
    public CinemachineCamera roomCamera;
    public bool isEnteringRoom;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (isEnteringRoom)
            {
                ScreenFader.Instance.FadeOut();
            }
            else
            {
                ScreenFader.Instance.FadeIn();
            }
            
            // If player enters the room switch to the room camera otherwise switch back to plaer VCam
            roomCamera.Priority = isEnteringRoom ? 20 : 0;
            
            // Set the right calling mask to make the void effect
            CameraMaskSwitcher.Instance.roomVcam = roomCamera;
        }
    }
}
