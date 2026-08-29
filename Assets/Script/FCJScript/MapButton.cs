using System.Collections.Generic;
using UnityEngine;

public class MapButton : MonoBehaviour
{
    private static bool isAnyMapOpen;
    private static readonly HashSet<GameObject> initializedPanels = new HashSet<GameObject>();

    public static bool IsAnyMapOpen => isAnyMapOpen;

    [Header("Map Panel")]
    public GameObject mapPanel;
    public bool startClosed = true;

    [Header("Map Interaction")]
    public bool pauseLookWhenOpen = true;

    [Header("Button Cursor Behavior")]
    [Tooltip("Only the open-map button should enable the custom map cursor on hover. The close button should stay at glow-only.")]
    public bool useMapCursorOnHover = false;

    private CursorInteractionTarget cursorTarget;
    private bool isOpen;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        isAnyMapOpen = false;
        initializedPanels.Clear();
    }

    private void Awake()
    {
        SetupCursorTarget();
        InitializeMapPanelOnce();

        isOpen = mapPanel != null && mapPanel.activeSelf;
        isAnyMapOpen = isOpen;
        RefreshCursorState();
    }

    private void OnEnable()
    {
        isOpen = mapPanel != null && mapPanel.activeSelf;
        isAnyMapOpen = isOpen;
        RefreshCursorState();
    }

    private void SetupCursorTarget()
    {
        if (cursorTarget == null)
        {
            cursorTarget = GetComponent<CursorInteractionTarget>();
        }

        if (cursorTarget == null)
        {
            cursorTarget = gameObject.AddComponent<CursorInteractionTarget>();
        }

        cursorTarget.enableInspectDialogue = false;
    }

    private void InitializeMapPanelOnce()
    {
        if (mapPanel == null || initializedPanels.Contains(mapPanel))
        {
            return;
        }

        initializedPanels.Add(mapPanel);
        mapPanel.SetActive(!startClosed);
    }

    private void RefreshCursorState()
    {
        if (cursorTarget == null)
        {
            return;
        }

        bool shouldUseMapCursor = useMapCursorOnHover && !isAnyMapOpen;
        cursorTarget.cursorPresetName = shouldUseMapCursor ? "Map" : string.Empty;
    }

    public void ToggleMap()
    {
        if (mapPanel == null)
        {
            Debug.LogWarning("MapButton: mapPanel is not assigned.");
            return;
        }

        SetMapVisible(!isAnyMapOpen);
    }

    public void OpenMap()
    {
        SetMapVisible(true);
    }

    public void CloseMap()
    {
        SetMapVisible(false);
    }

    public void SetMapVisible(bool visible)
    {
        if (mapPanel == null)
        {
            Debug.LogWarning("MapButton: mapPanel is not assigned.");
            return;
        }

        if (visible == isAnyMapOpen)
        {
            isOpen = visible;
            RefreshCursorState();
            return;
        }

        isAnyMapOpen = visible;
        isOpen = visible;

        MapButton[] allButtons = FindObjectsByType<MapButton>(FindObjectsSortMode.None);
        for (int i = 0; i < allButtons.Length; i++)
        {
            allButtons[i].isOpen = visible;
            allButtons[i].RefreshCursorState();
        }

        mapPanel.SetActive(visible);

        if (pauseLookWhenOpen)
        {
            LookController lookController = FindFirstObjectByType<LookController>();
            if (lookController != null)
            {
                lookController.SetPaused(visible);
            }
        }
    }
}
