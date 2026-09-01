using UnityEngine;

public class BackToToGet : MonoBehaviour
{
    public InventoryManager inventoryUsing;
    //[SerializeField] int CorrectID;

    public GameObject currentTarget;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Item"))
        {
            DragableItem item = collision.gameObject.GetComponent<DragableItem>();

            if (item.thisGet == true && item.thisType == "Tutorial")
            {
                currentTarget = collision.gameObject;
            }
        }
        else
        {
            Debug.Log("NotEqual");
            return;
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
                inventoryUsing.AddItem(currentTarget);
                currentTarget = null;
            }
        }
    }
}
