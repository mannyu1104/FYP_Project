using UnityEngine;

public class MapUnlock : MonoBehaviour
{
    [SerializeField] private MapIcon Map1;
    [SerializeField] private MapIcon Map2;
    [SerializeField] private MapIcon Map3;
    [SerializeField] private MapIcon Map4;
    [SerializeField] private MapIcon Map5;

    //[SerializeField] int CorrectID;

    public GameObject currentTarget;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Item"))
        {
            DragableItemSave item = collision.gameObject.GetComponent<DragableItemSave>();
            if (item.thisGet == true)
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
                DragableItemSave item = currentTarget.GetComponent<DragableItemSave>();

                if (item.thisID == 0)
                {
                    Map1.Unlocking();
                    item.thisUsed = true;
                    item.DeleteOther();
                }


                //DragableItem dragableItem = currentTarget.GetComponent<DragableItem>();
                //if (dragableItem.thisID == CorrectID)
                //{
                //    inventoryUsing.AddItem(currentTarget);
                //    gameObject.SetActive(false);
                //}
                currentTarget = null;
                //gameObject.SetActive(false);
            }
        }
    }
}
