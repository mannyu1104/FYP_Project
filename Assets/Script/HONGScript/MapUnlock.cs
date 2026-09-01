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
            if (item != null)
            {
                if (item.thisGet == true)
                {
                    currentTarget = collision.gameObject;
                }
            }
            else
            {
                return;
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
                    Destroy(item.gameObject);
                }
                else if (item.thisID == 1)
                {
                    Map2.Unlocking();
                    item.thisUsed = true;
                    item.DeleteOther();
                    Destroy(item.gameObject);
                }
                else if (item.thisID == 2)
                {
                    Map3.Unlocking();
                    item.thisUsed = true;
                    item.DeleteOther();
                    Destroy(item.gameObject);
                }
                else if (item.thisID == 3)
                {
                    Map4.Unlocking();
                    item.thisUsed = true;
                    item.DeleteOther();
                    Destroy(item.gameObject);
                }
                else if (item.thisID == 4)
                {
                    Map5.Unlocking();
                    item.thisUsed = true;
                    item.DeleteOther();
                    Destroy(item.gameObject);
                }
                else
                {
                    Debug.Log("InvalidItem");
                    return;
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
