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
        whiteBoardPanel.transform.SetAsLastSibling();
        isOpen = visible;
        isAnyWhiteBoardOpen = visible;

        if (pauseLookWhenOpen)
        {
            LookController lookController = FindFirstObjectByType<LookController>();
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
