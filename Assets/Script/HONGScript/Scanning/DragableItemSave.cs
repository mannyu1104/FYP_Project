using UnityEditor.Profiling;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragableItemSave : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler //IPointerClickHandler
{
    public MapItem item;
    [HideInInspector] public Transform parentAfterDrag;
    public Image image;
    public bool thisShow;
    //public bool thisUnlocked;
    public string thisType;
    public bool thisGet;
    public bool thisUsed;
    public int thisID;
    //[SerializeField] GameObject Description;
    public GameObject DesUI;
    public GameObject InvenUI;
    public bool isdragging;

    public void Start()
    {
        InitialiseItem(item);

        if (thisShow == true)
        {
            image.sprite = item.Image;
        }
        if (thisUsed == true)
        {
            DeleteOther();
            Destroy(gameObject);
        }
    }

    //void Update()
    //{

    //}

    public void InitialiseItem(MapItem newItem)
    {
        item = newItem;
        //thisUnlocked = newItem.Unlocked;
        thisGet = newItem.Get;
        thisUsed = newItem.Used;
        thisID = newItem.ItemID;
        thisShow = newItem.Show;
        isdragging = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (thisGet == true)
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
        if (thisGet == true)
        {
            Debug.Log("Dragging");
            transform.position = Input.mousePosition;
            isdragging = true;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (thisGet == true)
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

    //public void OnPointerClick(PointerEventData eventData)
    //{
    //    if (thisUsed)
    //    {
    //        DesUI.SetActive(true);
    //        Description.SetActive(true);
    //        InvenUI.SetActive(false);
    //    }
    //}

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

    public void DeleteOther()
    {
        DragableItemSave[] duplicatecheck = FindObjectsByType<DragableItemSave>();

        foreach (DragableItemSave duplicate in duplicatecheck)
        {
            if (duplicate.thisID == thisID)
            {
                Destroy(duplicate.gameObject);
            }
        }
    }

}