using UnityEngine;

public class Target : MonoBehaviour
{
    public void EnableTarget()
    {
        MinimapIndicator2D.AddTarget(this);
    }

    public void DisableTarget()
    {
        MinimapIndicator2D.RemoveTarget(this);
    }
}