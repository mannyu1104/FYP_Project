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
    public string thisDescription;
    //public bool thisShow;
    public bool thisGet;
    public bool thisUsed;
    public int thisID;
    [SerializeField] private TMP_Text Description;
    public GameObject DesUI;
    public GameObject InvenUI;
    public bool isdragging;

    private bool initialized;
    private Canvas boardDragCanvas;
    private bool createdBoardDragCanvas, previousOverrideSorting;
    private int previousSortingOrder;

    public void ConfigureCandy(int id, TMP_Text label, Image icon)
    {
        initialized = true; thisID = id; thisType = "Ingame";
        SumShowText = label; image = icon;
    }

    void Start() => EnsureInitialized();

    public void EnsureInitialized()
    {
        if (initialized) return;
        initialized = true;
        InitialiseItem(item);

        SumShowText.text = thisName;
    }

    void Update()
    {
        //if (thisShow == true)
        //{
        //    image.sprite = item.Image;
        //}
    }

    public void InitialiseItem(Item newItem)
    {
        item = newItem;
        thisGet = newItem.Get;
        thisUsed = newItem.Used;
        thisID = newItem.ItemID;
        //thisShow = newItem.Show;
        thisDescription = dragSourceData.ClueSummary.GetLocalizedString();
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
            if (WhiteBoard.IsAnyWhiteBoardOpen)
            {
                boardDragCanvas = GetComponent<Canvas>();
                createdBoardDragCanvas = boardDragCanvas == null;
                if (createdBoardDragCanvas) boardDragCanvas = gameObject.AddComponent<Canvas>();
                previousOverrideSorting = boardDragCanvas.overrideSorting;
                previousSortingOrder = boardDragCanvas.sortingOrder;
                boardDragCanvas.overrideSorting = true; boardDragCanvas.sortingOrder = 3000;
            }
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
            if (boardDragCanvas != null)
            {
                boardDragCanvas.overrideSorting = previousOverrideSorting;
                boardDragCanvas.sortingOrder = previousSortingOrder;
                if (createdBoardDragCanvas) Destroy(boardDragCanvas);
                boardDragCanvas = null;
            }
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
        if (thisID == 9103) { WelfareInteractionController.Instance?.InspectParkKey(); return; }
        if (thisID >= 9100 && thisID <= 9102)
        { WelfareInteractionController.Instance?.InspectCandy(thisID - 9100); return; }
        if (thisGet && isdragging == false)
        {
            DesUI.SetActive(true);
            Description.text = thisDescription;
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
