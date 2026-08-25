using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public Item item;
    [HideInInspector] public Transform parentAfterDrag;
    public Image image;
    public bool thisUnlocked;
    public bool thisGet;
    public bool thisUsed;
    public int thisID;
    [SerializeField] GameObject Description;
    public GameObject DesUI;
    public GameObject InvenUI;

    public void Start()
    {
        InitialiseItem(item);
    }

    public void InitialiseItem(Item newItem)
    {
        item = newItem;
        image.sprite = newItem.Image;
        thisUnlocked = newItem.Unlocked;
        thisGet = newItem.Get;
        thisUsed = newItem.Used;
        thisID = newItem.ItemID;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!thisUsed && thisUnlocked == true)
        {
            parentAfterDrag = transform.parent;
            transform.SetParent(transform.root);
            transform.SetAsLastSibling();
            image.raycastTarget = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!thisUsed && thisUnlocked == true)
        {
            transform.position = Input.mousePosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!thisUsed && thisUnlocked == true)
        {
            transform.SetParent(parentAfterDrag);
            image.raycastTarget = true;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (thisUsed)
        {
            DesUI.SetActive(true);
            Description.SetActive(true);
            InvenUI.SetActive(false);
        }
    }

    public void LoadSetParent()
    {
        if (parentAfterDrag != null)
        {
            transform.SetParent(parentAfterDrag);
        }
        else
        {
            return;
        }
    }

}
