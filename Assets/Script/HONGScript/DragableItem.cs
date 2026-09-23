using UnityEditor.Profiling;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class DragableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public ClueSourceData dragSourceData;
    public Item item;
    public Image image;
    [HideInInspector] public Transform parentAfterDrag;
    [SerializeField] private TMP_Text SumShowText;

    public string thisType;
    public string thisName;
    public string thisSave;
    public string thisSaveDes;
    public string thisDescription;
    //public bool thisShow;
    public bool thisGet;
    public bool thisUsed;
    public int thisID;
    [SerializeField] private TMP_Text Description;
    public GameObject DesUI;
    public GameObject ButtonUI;
    public GameObject ButtonUI2;
    public bool isdragging;

    public void Start()
    {
        InitialiseItem(item);

        SumShowText.text = thisName;
    }

    void Update()
    {
        //if (thisShow == true)
        //{
        //    image.sprite = item.Image;
        //}
        thisSave = dragSourceData.ClueTitle.GetLocalizedString();
        thisSaveDes = dragSourceData.ClueSummary.GetLocalizedString();
        if (thisName != thisSave)
        {
            thisName = thisSave;
            SumShowText.text = thisName;
        }
        if (thisSaveDes != thisDescription)
        {
            thisDescription = thisSaveDes;
            Description.text = thisDescription;
        }
    }

    public void InitialiseItem(Item newItem)
    {
        item = newItem;
        thisGet = newItem.Get;
        thisUsed = newItem.Used;
        thisID = newItem.ItemID;
        //thisShow = newItem.Show;
        thisName = dragSourceData.ClueTitle.GetLocalizedString();
        //thisName = item.TutorialClueDataTest.TutorialClueName.GetLocalizedString();
        thisType = newItem.TypeofItem;
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
            SumShowText.raycastTarget = false;
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
            transform.position = transform.parent.position;
            image.raycastTarget = true;
            SumShowText.raycastTarget = true;
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
        if (thisGet && isdragging == false)
        {
            DesUI.SetActive(true);
            Description.text = thisDescription;
            ButtonUI.SetActive(false);
            ButtonUI2.SetActive(false);
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
