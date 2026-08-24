using UnityEngine;

public class BagCollider : MonoBehaviour
{
    public InventoryManager inventorymanager;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Enter");
        DragableItem itemBool = collision.gameObject.GetComponent<DragableItem>();
        if (itemBool.thisGet == false)
        {
            GameObject item = collision.gameObject;
            inventorymanager.AddItem(item);
            Debug.Log("Trigger");
        }
    }
}
