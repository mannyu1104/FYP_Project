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
        if (item != null) InitialiseItem(item);
        BindLocalization();

        if (SumShowText != null) SumShowText.text = thisName;
    }

    private ClueSourceData subscribedSource;
    private static DragableItem inspectedItem;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetInspection() => inspectedItem = null;

    private void OnEnable() => BindLocalization();
    private void BindLocalization()
    {
        if (dragSourceData == null || subscribedSource == dragSourceData) return;
        UnbindLocalization();
        subscribedSource = dragSourceData;
        subscribedSource.ClueTitle.StringChanged += UpdateTitle;
        subscribedSource.ClueSummary.StringChanged += UpdateDescription;
    }
    private void OnDisable() => UnbindLocalization();
    private void UnbindLocalization()
    {
        if (subscribedSource == null) return;
        subscribedSource.ClueTitle.StringChanged -= UpdateTitle;
        subscribedSource.ClueSummary.StringChanged -= UpdateDescription;
        subscribedSource = null;
    }
    private void UpdateTitle(string value)
    {
        thisName = thisSave = value;
        if (SumShowText != null) SumShowText.text = value;
    }
    private void UpdateDescription(string value)
    {
        thisDescription = thisSaveDes = value;
        if (inspectedItem == this && Description != null && DesUI != null && DesUI.activeInHierarchy)
            Description.text = value;
    }

    public void InitialiseItem(Item newItem)
    {
        item = newItem;
        thisGet = newItem.Get;
        thisUsed = newItem.Used;
        thisID = newItem.ItemID;
        //thisShow = newItem.Show;
        if (dragSourceData != null)
        {
            thisName = dragSourceData.ClueTitle.GetLocalizedString();
            thisDescription = dragSourceData.ClueSummary.GetLocalizedString();
        }
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
        if (thisGet && !isdragging && DesUI != null && Description != null)
        {
            inspectedItem = this;
            DesUI.SetActive(true);
            Description.text = thisDescription;
            if (ButtonUI != null) ButtonUI.SetActive(false);
            if (ButtonUI2 != null) ButtonUI2.SetActive(false);
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
