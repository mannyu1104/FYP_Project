using UnityEngine;

public class Unlocking : MonoBehaviour
{
    private DragableItemSave ItemState;
    public GameObject UnlockingTarget;
    public InventoryManager inventory;

    public void Unlocked()
    {
        if (UnlockingTarget != null)
        {
            ItemState = UnlockingTarget.GetComponent<DragableItemSave>();
            ItemState.thisShow = true;
            inventory.AddItem(UnlockingTarget);
        }
        else
        {
            Debug.Log("NotFound");
            return;
        }
    }
}
