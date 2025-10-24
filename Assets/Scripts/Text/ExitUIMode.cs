using System;
using UnityEngine;

public class ExitUIMode : MonoBehaviour
{
    private PlayerController _player;

    private void Start()
    {
        _player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
    }

    public void PlayerExitUIMode()
    {
        _player.isUIActive = false;
    }
}
