using UnityEngine;
#if UNITY_EDITOR
using UnityEditor; // This namespace allows us to interact with the Editor
#endif

public class PLayerCollisionDetector : MonoBehaviour
{
    // This works for "solid" collisions
    private void OnCollisionEnter(Collision collision)
    {
        // Log the name to the console
        Debug.Log("Collided with: " + collision.gameObject.name);

        // Highlight the object in the Hierarchy
#if UNITY_EDITOR
        Selection.activeGameObject = collision.gameObject;
        EditorGUIUtility.PingObject(collision.gameObject);
#endif
    }

    // This works if one of the colliders is set to "Is Trigger"
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Triggered by: " + other.gameObject.name);

#if UNITY_EDITOR
        Selection.activeGameObject = other.gameObject;
        EditorGUIUtility.PingObject(other.gameObject);
#endif
    }
}