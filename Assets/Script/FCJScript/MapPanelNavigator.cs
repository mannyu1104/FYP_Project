using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Opens one location panel from a numbered list.
/// </summary>
public class MapPanelNavigator : MonoBehaviour
{
    private const int HomeIndex = 0;
    private const int OrphanageIndex = 1;
    private const int WelfareIndex = 2;
    private const int ParkIndex = 3;

    [Header("Location Panels")]
    [Tooltip("Element 0 = Home, 1 = Orphanage, 2 = Welfare, 3 = Park.")]
    [SerializeField] private List<GameObject> locationPanels = new List<GameObject>();

    [Header("Map Panel")]
    [Tooltip("Optional map panel to close after choosing a location.")]
    [SerializeField] private GameObject mapPanel;
    [SerializeField] private bool closeMapAfterSelection = true;
    [SerializeField] private bool unpauseLookAfterSelection = true;

    [Header("Transition")]
    [SerializeField] private ScreenTransitionController screenTransitionController;
    [SerializeField] private bool useTransition = true;

    private void Awake()
    {
        ResolveReferences();
    }

    public void OpenPanel(int panelIndex)
    {
        ResolveReferences();

        if (useTransition && screenTransitionController != null)
        {
            screenTransitionController.PlayTransition(() => OpenPanelImmediately(panelIndex));
            return;
        }

        OpenPanelImmediately(panelIndex);
    }

    private void OpenPanelImmediately(int panelIndex)
    {
        if (locationPanels.Count == 0)
        {
            Debug.LogWarning("MapPanelNavigator: No location panels are assigned.", this);
            return;
        }

        int clampedIndex = Mathf.Clamp(panelIndex, 0, locationPanels.Count - 1);

        for (int i = 0; i < locationPanels.Count; i++)
        {
            SetGameObject(locationPanels[i], i == clampedIndex);
        }

        CloseMapIfNeeded();
        UnpauseLookIfNeeded();
    }

    public void OpenHome()
    {
        OpenPanel(HomeIndex);
    }

    public void OpenOrphanage()
    {
        OpenPanel(OrphanageIndex);
    }

    public void OpenWelfare()
    {
        OpenPanel(WelfareIndex);
    }

    public void OpenPark()
    {
        OpenPanel(ParkIndex);
    }

    private void CloseMapIfNeeded()
    {
        if (closeMapAfterSelection && mapPanel != null)
        {
            mapPanel.SetActive(false);
            MapButton.SyncMapState(false);
        }
    }

    private void UnpauseLookIfNeeded()
    {
        if (!unpauseLookAfterSelection)
        {
            return;
        }

        LookController lookController = FindFirstObjectByType<LookController>();
        if (lookController != null)
        {
            lookController.SetPaused(false);
        }
    }

    private void ResolveReferences()
    {
        if (screenTransitionController == null)
        {
            screenTransitionController = FindFirstObjectByType<ScreenTransitionController>();
        }
    }

    private static void SetGameObject(GameObject target, bool active)
    {
        if (target != null)
        {
            target.SetActive(active);
        }
    }
}
