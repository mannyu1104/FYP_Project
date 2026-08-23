using UnityEngine;

public class UsingEvent : MonoBehaviour
{
    public InventoryManagerINFO inventoryUsing;
    [SerializeField] int CorrectID;

    public GameObject currentTarget;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("GOTIN");
        if (collision.gameObject.layer == LayerMask.NameToLayer("Item"))
        {
            Debug.Log("IN");
            currentTarget = collision.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Item"))
        {
            Debug.Log("OUT");
            currentTarget = null;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (currentTarget != null)
            {
                Debug.Log("OnIT");
                DragableItem dragableItem = currentTarget.GetComponent<DragableItem>();
                if (dragableItem.thisID == CorrectID)
                {
                    inventoryUsing.AddItem(currentTarget);
                    Debug.Log("Used");
                    gameObject.SetActive(false);
                }
            }
        }
    }

    //public void OnDrop(PointerEventData eventData)
    //{
    //    Debug.Log("OnIT");
    //    GameObject dropped = eventData.pointerDrag;
    //    DragableItem dragableItem = dropped.GetComponent<DragableItem>();
    //    if (dragableItem.thisID == CorrectID)
    //    {
    //        inventoryUsing.AddItem(dropped);
    //        Debug.Log("Used");
    //    }
    //    else
    //    {
    //        return;
    //    }
    //}
}
