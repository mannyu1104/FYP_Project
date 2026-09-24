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
    [Tooltip("Optional navigator used to reset the orphanage to its main area when selected from the map.")]
    [SerializeField] private LocationNavigator orphanageNavigator;

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
        if (panelIndex >= 0 && panelIndex < locationPanels.Count && panelIndex != SavedLocation)
            GameAudioManager.Instance?.PlayLocationFootsteps();

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

        if (clampedIndex == OrphanageIndex && orphanageNavigator != null)
        {
            orphanageNavigator.ShowOrphanageMainFromMap();
        }

        CloseMapIfNeeded();
        UnpauseLookIfNeeded();
    }

    public GameObject LocationRoot(int index) => index >= 0 && index < locationPanels.Count ? locationPanels[index] : null;
    public int SavedLocation => locationPanels.FindIndex(p => p != null && p.activeSelf);
    public void RestoreLocation(int index) => OpenPanelImmediately(index);

    public void ResetToHome() => OpenPanelImmediately(HomeIndex);

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
        if (!closeMapAfterSelection) return;
        // Location buttons resolve the current map prefab. The navigator's older
        // serialized panel may be missing or still point to the legacy map.
        foreach (var button in FindObjectsByType<MapButton>(FindObjectsInactive.Include))
            if (button.gameObject.scene == gameObject.scene && button.mapPanel != null)
                button.CloseMap();
        if (mapPanel != null) mapPanel.SetActive(false);
        MapButton.SyncMapState(false);
    }

    private void UnpauseLookIfNeeded()
    {
        if (!unpauseLookAfterSelection)
        {
            return;
        }

        LookController lookController = FindAnyObjectByType<LookController>();
        if (lookController != null)
        {
            lookController.SetPaused(false);
        }
    }

    private void ResolveReferences()
    {
        if (screenTransitionController == null)
        {
            screenTransitionController = FindAnyObjectByType<ScreenTransitionController>();
        }

        if (orphanageNavigator == null)
        {
            orphanageNavigator = FindAnyObjectByType<LocationNavigator>();
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
