using System;
using UnityEngine;

public class MiniMapCamera : MonoBehaviour
{
    private Transform _playerTransform;
    private float _initialZPosition; 

    private void Start()
    {
        _playerTransform = GameObject.FindWithTag("Player").transform;
        _initialZPosition = transform.position.z; 
        
        transform.rotation = Quaternion.identity; 
    }

    private void LateUpdate()
    {
        Vector3 newPos = _playerTransform.position;
        newPos.z = _initialZPosition; 

        transform.position = newPos;
    }
}