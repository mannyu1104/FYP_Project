using UnityEngine;

public class UsingEvent : MonoBehaviour
{
    public InventoryManagerINFO inventoryUsing;
    //[SerializeField] int CorrectID;

    public GameObject currentTarget;

    private void OnEnable() => ResolveInventory();

    private void ResolveInventory()
    {
        if (inventoryUsing != null) return;
        // The computer prefab cannot serialize a reference to the separate notebook prefab.
        foreach (var save in FindObjectsByType<SaveSystem>(FindObjectsInactive.Include))
            if (save.gameObject.scene == gameObject.scene && save.inventoryUsing != null)
            {
                inventoryUsing = save.inventoryUsing;
                return;
            }
    }

    private void OnDisable() => currentTarget = null;

    public bool TryDrop(DragableItem item, Vector2 screenPosition, Camera camera)
    {
        if (!isActiveAndEnabled || item == null || !item.thisGet || item.thisUsed || item.thisTuto || item.thisType != "Tutorial") return false;
        foreach (var group in GetComponentsInParent<CanvasGroup>())
            if (group.alpha <= .01f) return false;
        var rect = transform as RectTransform;
        if (rect == null || !RectTransformUtility.RectangleContainsScreenPoint(rect, screenPosition, camera)) return false;
        ResolveInventory();
        if (inventoryUsing == null) return false;
        inventoryUsing.AddItem(item.gameObject);
        currentTarget = null;
        return item.thisUsed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null || collision.gameObject == null)
        {
            return;
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Item"))
        {
            DragableItem item = collision.gameObject.GetComponent<DragableItem>();
            if (item == null)
            {
                return;
            }

            if (item.thisGet == true && item.thisType == "Tutorial" && item.thisUsed == false)
            {
                currentTarget = collision.gameObject;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision != null && collision.gameObject == currentTarget)
        {
            currentTarget = null;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            ResolveInventory();
            if (currentTarget != null && inventoryUsing != null)
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
