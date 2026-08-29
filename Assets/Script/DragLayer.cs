using UnityEngine;

// Put this on a dedicated Canvas whose Sort Order is higher than every other
// Canvas in the scene (e.g. 999), with no other content on it. Any item being
// dragged gets temporarily reparented here, so it always renders above
// whichever Canvas it's currently hovering over - regardless of which Canvas
// it started in or which Canvas the drop zone belongs to.
public class DragLayer : MonoBehaviour
{
    public static Transform Transform { get; private set; }

    private void Awake()
    {
        Transform = transform;
    }
}