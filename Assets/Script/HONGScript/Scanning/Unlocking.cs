using UnityEngine;

public class Unlocking : MonoBehaviour
{
    private DragableItemSave ItemState;
    public GameObject UnlockingTarget;

    public void Unlocked()
    {
        ItemState = UnlockingTarget.GetComponent<DragableItemSave>();
        ItemState.thisShow = true;
        ItemState.thisShow = true;
    }
}
