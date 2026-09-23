using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Localization;

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
    public bool thisTuto;
    public int thisID;
    [SerializeField] private TMP_Text Description;
    public GameObject DesUI;
    public GameObject ButtonUI;
    public GameObject ButtonUI2;
    public bool isdragging;

    private bool initialized;
    private static DragableItem inspectedItem;

    // Runtime pickups have no ScriptableObject; their state must not be reset by Start.
    public void ConfigureCandy(int id, TMP_Text label, Image icon)
    {
        initialized = true;
        thisID = id;
        thisType = "Ingame";
        thisTuto = false;
        SumShowText = label;
        image = icon;
    }

    public void Start() => EnsureInitialized();

    private static string SafeLocalizedText(LocalizedString localizedString)
    {
        if (localizedString == null)
        {
            return string.Empty;
        }

        try
        {
            var initHandle = UnityEngine.Localization.Settings.LocalizationSettings.InitializationOperation;
            if (!initHandle.IsDone)
            {
                return string.Empty;
            }

            string value = localizedString.GetLocalizedString();
            return value ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    public void EnsureInitialized()
    {
        if (initialized) return;
        initialized = true;
        if (item != null) InitialiseItem(item);
        if (SumShowText != null) SumShowText.text = thisName;
    }

    void Update()
    {
        if (dragSourceData == null) return;
        if (dragSourceData.ClueTitle == null || dragSourceData.ClueSummary == null)
        {
            return;
        }

        //if (thisShow == true)
        //{
        //    image.sprite = item.Image;
        //}
        thisSave = SafeLocalizedText(dragSourceData.ClueTitle);
        thisSaveDes = SafeLocalizedText(dragSourceData.ClueSummary);
        if (thisName != thisSave)
        {
            thisName = thisSave;
            if (SumShowText != null) SumShowText.text = thisName;
        }
        if (thisSaveDes != thisDescription)
        {
            thisDescription = thisSaveDes;
            if (Description != null && inspectedItem == this) Description.text = thisDescription;
        }
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
            thisName = SafeLocalizedText(dragSourceData.ClueTitle);
            thisDescription = SafeLocalizedText(dragSourceData.ClueSummary);
        }
        //thisName = item.TutorialClueDataTest.TutorialClueName.GetLocalizedString();
        thisType = newItem.TypeofItem;
        isdragging = false;
        
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (thisGet == true && thisTuto == false)
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
        if (thisGet == true && thisTuto == false)
        {
            Debug.Log("Dragging");
            transform.position = Input.mousePosition;
            isdragging = true;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (thisGet == true && thisTuto == false)
        {
            Debug.Log("EndDrag");
            foreach (var receiver in FindObjectsByType<UsingEvent>())
                if (receiver.TryDrop(this, eventData.position, eventData.pressEventCamera)) break;
            isdragging = false;
            transform.SetParent(parentAfterDrag, false);
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;
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
        if (thisID == 9103) { WelfareInteractionController.Instance?.InspectParkKey(); return; }
        if (thisID >= 9100 && thisID <= 9102)
        { WelfareInteractionController.Instance?.InspectCandy(thisID - 9100); return; }
        if (thisGet && isdragging == false)
        {
            inspectedItem = this;
            if (DesUI != null) DesUI.SetActive(true);
            if (Description != null && inspectedItem == this) Description.text = thisDescription;
            if (ButtonUI != null) ButtonUI.SetActive(false);
            if (ButtonUI2 != null) ButtonUI2.SetActive(false);
        }
    }

    public void LoadSetParent()
    {
        if (parentAfterDrag != null)
        {
            transform.SetParent(parentAfterDrag, false);
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;
        }
        else
        {
            return;
        }
    }

}
