using System.Collections.Generic;
using UnityEngine;

public class TabBarController : MonoBehaviour
{
    [SerializeField] private BrowserTabButton tabButtonPrefab;
    [SerializeField] private Transform tabButtonContainer; 

    private readonly Dictionary<BrowserTab, BrowserTabButton> spawnedButtons = new Dictionary<BrowserTab, BrowserTabButton>();

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
            BrowserTabButton button = Instantiate(tabButtonPrefab, tabButtonContainer);
            button.Bind(tab);
            spawnedButtons[tab] = button;
        }

        RefreshActiveVisuals(BrowserTabManager.Instance.ActiveTab);
    }

    private void RefreshActiveVisuals(BrowserTab activeTab)
    {
        foreach (KeyValuePair<BrowserTab, BrowserTabButton> pair in spawnedButtons)
        {
            pair.Value.SetActiveVisual(pair.Key == activeTab);
        }
    }
}