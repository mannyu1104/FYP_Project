using UnityEngine;

public class InventoryManagerINFO : MonoBehaviour
{
    public InventorySlot[] inventorySlots;

    public void AddItem(GameObject item)
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            InventorySlot slot = inventorySlots[i];
            DragableItem iteminslot = slot.GetComponentInChildren<DragableItem>(true);
            DragableItemSave mapItemInSlot = slot.GetComponentInChildren<DragableItemSave>(true);
            if (iteminslot == null && mapItemInSlot == null)
            {
                SetNewItem(item, slot);
                return;
            }
        }
    }

    void SetNewItem(GameObject item, InventorySlot slot)
    {
        DragableItem dragableitem = item.GetComponent<DragableItem>();
        DragableItemSave mapItem = item.GetComponent<DragableItemSave>();
        if (dragableitem != null)
        {
            dragableitem.thisUsed = true;
            dragableitem.parentAfterDrag = slot.transform;
        }
        else if (mapItem != null)
        {
            mapItem.thisUsed = true;
            mapItem.parentAfterDrag = slot.transform;
        }
        else return;
        item.transform.SetParent(slot.transform, false);
    }
}
