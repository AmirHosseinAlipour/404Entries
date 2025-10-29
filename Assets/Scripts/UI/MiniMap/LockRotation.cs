using System;
using UnityEngine;

public class LockRotation : MonoBehaviour
{
    private void LateUpdate()
    {
        transform.rotation = Quaternion.identity;
    }
}
