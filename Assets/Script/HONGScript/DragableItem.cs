using UnityEditor.Profiling;
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
    public bool isdragging;

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
        isdragging = false; 
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!thisUsed && thisUnlocked == true)
        {
            Debug.Log("StartDrag");
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
            Debug.Log("Dragging");
            transform.position = Input.mousePosition;
            isdragging = true;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!thisUsed && thisUnlocked == true)
        {
            Debug.Log("EndDrag");
            isdragging = false;
            transform.SetParent(parentAfterDrag);
            image.raycastTarget = true;
        }
        //else if (!thisUsed && thisGet == true)
        //{
        //    transform.SetParent(parentAfterDrag);
        //    transform.position = transform.parent.position;
        //    image.raycastTarget = true;
        //}
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
