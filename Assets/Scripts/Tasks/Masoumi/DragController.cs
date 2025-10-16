using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DragDropable : MonoBehaviour 
{
    [SerializeField] private InputAction press, screenPos;

    private Vector3 curScreenPos;

    Camera camera;
    private bool isDragging;

    private Vector3 WorldPos
    {
        get
        {
            float z = camera.WorldToScreenPoint(transform.position).z;
            return camera.ScreenToWorldPoint(curScreenPos + new Vector3(0, 0, z));
        }
    }
    private bool isClickedOn
    {
        get
        {
            Ray ray = camera.ScreenPointToRay(curScreenPos);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit))
            {
                return hit.transform == transform;
            }
            return false;
        }
    }
    private void Awake() 
    {
        camera = Camera.main;
        screenPos.Enable();
        press.Enable();
        screenPos.performed += context => { curScreenPos = context.ReadValue<Vector2>(); };
        press.performed += _ => { if(isClickedOn) StartCoroutine(Drag()); };
        press.canceled += _ => { isDragging = false; };

    }

    private IEnumerator Drag()
    {
        isDragging = true;
        Vector3 offset = transform.position - WorldPos;
        // grab
        GetComponent<Rigidbody>().useGravity = false;
        while(isDragging)
        {
            // dragging
            transform.position = WorldPos + offset;
            yield return null;
        }
        // drop
        GetComponent<Rigidbody>().useGravity = true;
    }
}