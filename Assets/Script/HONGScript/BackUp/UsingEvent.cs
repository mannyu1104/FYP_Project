using UnityEngine;

public class UsingEvent : MonoBehaviour
{
    public InventoryManagerINFO inventoryUsing;
    //[SerializeField] int CorrectID;

    public GameObject currentTarget;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Item"))
        {
            DragableItem item = collision.gameObject.GetComponent<DragableItem>();
            if (item.thisGet == true && item.thisType == "Tutorial" && item.thisUsed == false)
            {
                currentTarget = collision.gameObject;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Item"))
        {
            currentTarget = null;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (currentTarget != null)
            {
                //DragableItem dragableItem = currentTarget.GetComponent<DragableItem>();
                //if (dragableItem.thisID == CorrectID)
                //{
                //    inventoryUsing.AddItem(currentTarget);
                //    gameObject.SetActive(false);
                //}
                inventoryUsing.AddItem(currentTarget);
                currentTarget = null;
                //gameObject.SetActive(false);
            }
        }
    }
}
