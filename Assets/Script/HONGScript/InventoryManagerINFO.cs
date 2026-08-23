using UnityEngine;

public class InventoryManagerINFO : MonoBehaviour
{
    public InventorySlot[] inventorySlots;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //void Start()
    //{

    //}

    public void AddItem(GameObject item)
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            InventorySlot slot = inventorySlots[i];
            DragableItem iteminslot = slot.GetComponentInChildren<DragableItem>();
            if (iteminslot == null)
            {
                SetNewItem(item, slot);
                return;
            }
        }
    }

    void SetNewItem(GameObject item, InventorySlot slot)
    {
        DragableItem dragableitem = item.GetComponent<DragableItem>();
        dragableitem.thisUsed = true;
        dragableitem.transform.SetParent(slot.transform);
    }
}
