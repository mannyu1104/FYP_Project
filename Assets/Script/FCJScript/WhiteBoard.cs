using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WhiteBoard : MonoBehaviour, IPointerClickHandler
{
    private static bool isAnyWhiteBoardOpen;

    public static bool IsAnyWhiteBoardOpen => isAnyWhiteBoardOpen;

    [Header("Whiteboard Panel")]
    [SerializeField] private GameObject whiteBoardPanel;
    [SerializeField] private Button closeButton;
    [SerializeField] private string whiteBoardPanelName = "WhiteBoardPanel";
    [SerializeField] private bool hidePanelOnStart = true;

    [Header("Cursor")]
    [SerializeField] private bool configureCursorTarget = true;
    [SerializeField] private string cursorPresetName = "View";
    [SerializeField] private string closeButtonCursorPresetName = "Back";

    [Header("Interaction")]
    [SerializeField] private bool pauseLookWhenOpen = true;
    [SerializeField] private bool closeWithEscape = true;

    private bool isOpen;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        isAnyWhiteBoardOpen = false;
    }

    private void Awake()
    {
        SetupCursorTarget();
        ResolveReferences();
        SetupCloseButton();
        if (whiteBoardPanel != null)
            foreach (Transform child in whiteBoardPanel.transform)
                if (child.GetComponent<Image>() != null && child.GetComponent<Button>() == null)
                {
                    if (child.GetComponent<WhiteBoardSurface>() == null) child.gameObject.AddComponent<WhiteBoardSurface>();
                    break;
                }

        if (hidePanelOnStart)
        {
            SetWhiteBoardVisible(false);
        }
        else
        {
            isOpen = whiteBoardPanel != null && whiteBoardPanel.activeSelf;
        }
    }

    private void Update()
    {
        var menu = FindAnyObjectByType<MainMenuController>();
        if (SaveSlotPanel.IsOpen || (menu != null && menu.IsSettingsVisible)) return;
        if (isOpen && closeWithEscape && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseWhiteBoard();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isAnyWhiteBoardOpen)
        {
            return;
        }

        OpenWhiteBoard();
    }

    private void OnMouseDown()
    {
        if (isAnyWhiteBoardOpen)
        {
            return;
        }

        OpenWhiteBoard();
    }

    public void OpenWhiteBoard()
    {
        SetWhiteBoardVisible(true);
    }

    public void CloseWhiteBoard()
    {
        SetWhiteBoardVisible(false);
    }

    private void SetWhiteBoardVisible(bool visible)
    {
        if (whiteBoardPanel == null)
        {
            Debug.LogWarning("WhiteBoard: WhiteBoardPanel is not assigned or found.", this);
            return;
        }

        whiteBoardPanel.SetActive(visible);
        // Keep the board below inventory (2001) and settings (4001), regardless of sibling order.
        var canvas = whiteBoardPanel.GetComponent<Canvas>();
        if (canvas == null) canvas = whiteBoardPanel.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 1000;
        if (whiteBoardPanel.GetComponent<GraphicRaycaster>() == null)
            whiteBoardPanel.AddComponent<GraphicRaycaster>();
        isOpen = visible;
        isAnyWhiteBoardOpen = visible;

        if (pauseLookWhenOpen)
        {
            LookController lookController = FindAnyObjectByType<LookController>();
            if (lookController != null)
            {
                lookController.SetPaused(visible);
            }
        }
    }

    private void SetupCursorTarget()
    {
        if (!configureCursorTarget)
        {
            return;
        }

        CursorInteractionTarget target = GetComponent<CursorInteractionTarget>();
        if (target == null)
        {
            target = gameObject.AddComponent<CursorInteractionTarget>();
        }

        target.enableInspectDialogue = false;
        target.cursorPresetName = cursorPresetName;
    }

    private void ResolveReferences()
    {
        if (whiteBoardPanel == null)
        {
            whiteBoardPanel = FindInactiveGameObjectByName(whiteBoardPanelName);
        }

        if (closeButton == null && whiteBoardPanel != null)
        {
            closeButton = whiteBoardPanel.GetComponentInChildren<Button>(true);
        }
    }

    private void SetupCloseButton()
    {
        if (closeButton == null)
        {
            Debug.LogWarning("WhiteBoard: close button is not assigned or found.", this);
            return;
        }

        CursorInteractionTarget closeTarget = closeButton.GetComponent<CursorInteractionTarget>();
        if (closeTarget == null)
        {
            closeTarget = closeButton.gameObject.AddComponent<CursorInteractionTarget>();
        }

        closeTarget.enableInspectDialogue = false;
        closeTarget.cursorPresetName = closeButtonCursorPresetName;

        foreach (var label in closeButton.GetComponentsInChildren<TMPro.TMP_Text>(true)) label.gameObject.SetActive(false);
        foreach (var label in closeButton.GetComponentsInChildren<Text>(true)) label.gameObject.SetActive(false);
        if (closeButton.targetGraphic is Image background)
        {
            background.sprite = null;
            background.color = Color.clear;
        }
        for (int i = 0; i < 2; i++)
        {
            var stroke = new GameObject("Close cross " + i, typeof(RectTransform), typeof(Image));
            stroke.transform.SetParent(closeButton.transform, false);
            var rect = (RectTransform)stroke.transform;
            rect.sizeDelta = new Vector2(58, 7);
            rect.localRotation = Quaternion.Euler(0, 0, i == 0 ? 45 : -45);
            stroke.GetComponent<Image>().color = new Color(1f, .55f, .55f);
            stroke.GetComponent<Image>().raycastTarget = false;
        }
        closeButton.onClick.RemoveListener(CloseWhiteBoard);
        closeButton.onClick.AddListener(CloseWhiteBoard);
    }

    private GameObject FindInactiveGameObjectByName(string objectName)
    {
        if (string.IsNullOrWhiteSpace(objectName))
        {
            return null;
        }

        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform current = transforms[i];
            if (current != null && current.gameObject.scene.IsValid() && current.name == objectName)
            {
                return current.gameObject;
            }
        }

        return null;
    }
}


// Cards are references to evidence; placing one does not consume the inventory item.
public class WhiteBoardSurface : MonoBehaviour, IDropHandler
{
    [System.Serializable] public class Placement { public int id; public Vector2 position; }
    [System.Serializable] public class Layout { public System.Collections.Generic.List<Placement> cards = new System.Collections.Generic.List<Placement>(); }
    private readonly System.Collections.Generic.Dictionary<int, WhiteBoardCard> cards = new System.Collections.Generic.Dictionary<int, WhiteBoardCard>();
    private static string SavePath => System.IO.Path.Combine(Application.persistentDataPath, "whiteboard.json");

    public void OnDrop(PointerEventData e)
    {
        if (!WhiteBoard.IsAnyWhiteBoardOpen || e.pointerDrag == null) return;
        var item = e.pointerDrag.GetComponent<DragableItem>();
        if (item == null || !item.thisGet || item.thisUsed) return;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, e.position, e.pressEventCamera, out var position))
            Place(item, position);
    }

    private void Place(DragableItem item, Vector2 position)
    {
        if (!cards.TryGetValue(item.thisID, out var card))
        {
            var obj = new GameObject("Evidence " + item.thisID, typeof(RectTransform), typeof(Image), typeof(WhiteBoardCard));
            obj.transform.SetParent(transform, false);
            obj.GetComponent<Image>().color = new Color(1f, .95f, .78f);
            card = obj.GetComponent<WhiteBoardCard>(); card.ItemId = item.thisID;
            ((RectTransform)obj.transform).sizeDelta = new Vector2(190, 140);
            var iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(obj.transform, false);
            var icon = iconObj.GetComponent<Image>();
            icon.sprite = item.image != null ? item.image.sprite : null;
            icon.color = item.image != null ? item.image.color : Color.white;
            icon.preserveAspect = true; icon.raycastTarget = false;
            icon.rectTransform.sizeDelta = new Vector2(70, 65); icon.rectTransform.anchoredPosition = new Vector2(0, 28);
            iconObj.SetActive(icon.sprite != null);
            var labelObj = new GameObject("Title", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
            labelObj.transform.SetParent(obj.transform, false);
            var label = labelObj.GetComponent<TMPro.TextMeshProUGUI>();
            label.text = item.thisName;
            label.fontSize = 22; label.enableAutoSizing = true; label.fontSizeMin = 14; label.fontSizeMax = 22;
            label.color = new Color(.15f, .15f, .15f); label.alignment = TMPro.TextAlignmentOptions.Center;
            label.raycastTarget = false; label.rectTransform.sizeDelta = new Vector2(176, 60);
            label.rectTransform.anchoredPosition = new Vector2(0, -35);
            LocalizedFontController.Instance?.ApplyTo(label);
            if (item.dragSourceData != null)
            {
                var localized = labelObj.AddComponent<UnityEngine.Localization.Components.LocalizeStringEvent>();
                localized.StringReference = item.dragSourceData.ClueTitle;
                localized.OnUpdateString.AddListener(value => label.text = value);
                localized.RefreshString();
            }
            cards.Add(item.thisID, card);
        }
        card.MoveTo(position);
    }

    public void SaveLayout()
    {
        var layout = new Layout();
        foreach (var pair in cards) layout.cards.Add(new Placement { id = pair.Key, position = ((RectTransform)pair.Value.transform).anchoredPosition });
        System.IO.File.WriteAllText(SavePath, JsonUtility.ToJson(layout));
    }

    public void LoadLayout()
    {
        foreach (var card in cards.Values) Destroy(card.gameObject);
        cards.Clear();
        if (!System.IO.File.Exists(SavePath)) return;
        var layout = JsonUtility.FromJson<Layout>(System.IO.File.ReadAllText(SavePath));
        if (layout == null || layout.cards == null) return;
        var items = FindObjectsByType<DragableItem>(FindObjectsInactive.Include);
        foreach (var placement in layout.cards)
            foreach (var item in items)
                if (item.thisID == placement.id && item.thisGet && !item.thisUsed) { Place(item, placement.position); break; }
    }
}

public class WhiteBoardCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int ItemId;
    private Vector2 offset;
    public void OnBeginDrag(PointerEventData e)
    {
        transform.SetAsLastSibling();
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform.parent, e.position, e.pressEventCamera, out var pointer);
        offset = ((RectTransform)transform).anchoredPosition - pointer;
    }
    public void OnDrag(PointerEventData e)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform.parent, e.position, e.pressEventCamera, out var pointer)) MoveTo(pointer + offset);
    }
    public void OnEndDrag(PointerEventData e) { }
    public void MoveTo(Vector2 position)
    {
        var rect = (RectTransform)transform;
        var bounds = ((RectTransform)transform.parent).rect;
        var half = rect.sizeDelta * .5f;
        rect.anchoredPosition = new Vector2(Mathf.Clamp(position.x, bounds.xMin + half.x, bounds.xMax - half.x),
            Mathf.Clamp(position.y, bounds.yMin + half.y, bounds.yMax - half.y));
    }
}
