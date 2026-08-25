using System.Collections.Generic;
using UnityEngine;

public class TabBarController : MonoBehaviour
{
    [SerializeField] private BrowserButton tabButtonPrefab;
    [SerializeField] private Transform tabButtonContainer; 

    private readonly Dictionary<BrowserTab, BrowserButton> spawnedButtons = new Dictionary<BrowserTab, BrowserButton>();

    private void OnEnable()
    {
        BrowserTabManager.Instance.OnTabsChanged += RebuildTabButtons;
        BrowserTabManager.Instance.OnActiveTabChanged += RefreshActiveVisuals;
        RebuildTabButtons();
    }

    private void OnDisable()
    {
        BrowserTabManager.Instance.OnTabsChanged -= RebuildTabButtons;
        BrowserTabManager.Instance.OnActiveTabChanged -= RefreshActiveVisuals;
    }

    private void RebuildTabButtons()
    {
        foreach (Transform child in tabButtonContainer)
            Destroy(child.gameObject);
        spawnedButtons.Clear();

        foreach (BrowserTab tab in BrowserTabManager.Instance.OpenTabs)
        {
            BrowserButton button = Instantiate(tabButtonPrefab, tabButtonContainer);
            button.Bind(tab);
            spawnedButtons[tab] = button;
        }

        RefreshActiveVisuals(BrowserTabManager.Instance.ActiveTab);
    }

    private void RefreshActiveVisuals(BrowserTab activeTab)
    {
        foreach (KeyValuePair<BrowserTab, BrowserButton> pair in spawnedButtons)
        {
            pair.Value.SetActiveVisual(pair.Key == activeTab);
        }
    }
}