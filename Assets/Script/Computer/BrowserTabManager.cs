using System;
using System.Collections.Generic;
using UnityEngine;

public class BrowserTabManager : MonoBehaviour
{
    public static BrowserTabManager Instance { get; private set; }

    public IReadOnlyList<BrowserTab> OpenTabs => openTabs;
    public BrowserTab ActiveTab { get; private set; }

    private readonly List<BrowserTab> openTabs = new List<BrowserTab>();

    // Fired whenever a tab is added or removed.
    public event Action OnTabsChanged;

    // Fired when the active tab changes.
    public event Action<BrowserTab> OnActiveTabChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Open a page. If a tab with the same PageId already exists, switch to it.
    public BrowserTab OpenPage(IBrowserPage page)
    {
        BrowserTab existing = FindTabByPageId(page.PageId);
        if (existing != null)
        {
            SetActiveTab(existing);
            return existing;
        }

        BrowserTab newTab = new BrowserTab(page);
        openTabs.Add(newTab);
        OnTabsChanged?.Invoke();
        SetActiveTab(newTab);
        return newTab;
    }

    public void CloseTab(BrowserTab tab)
    {
        Debug.Log($"Closing tab: {tab.Page.TabTitle} (ID: {tab.Page.PageId})");

        int index = openTabs.IndexOf(tab);
        if (index < 0) return;

        bool wasActive = ActiveTab == tab;
        openTabs.RemoveAt(index);
        OnTabsChanged?.Invoke();

        if (!wasActive) return;

        if (openTabs.Count == 0)
        {
            SetActiveTab(null);
            return;
        }

        // Falls back to the tab that was to the left of the closed one.
        int fallbackIndex = Mathf.Clamp(index - 1, 0, openTabs.Count - 1);
        SetActiveTab(openTabs[fallbackIndex]);
        Debug.Log($"Switched to tab: {ActiveTab.Page.TabTitle} (ID: {ActiveTab.Page.PageId})");
    }

    public void SwitchTab(BrowserTab tab)
    {
        if (!openTabs.Contains(tab)) return;
        SetActiveTab(tab);
        Debug.Log($"Switched to tab: {tab.Page.TabTitle} (ID: {tab.Page.PageId})");
    }

    public void ResetActiveTab()
    {
        SetActiveTab(null);
        Debug.Log("Active tab reset.");
    }

    private void SetActiveTab(BrowserTab tab)
    {
        if (ActiveTab == tab) return;
        ActiveTab = tab;
        OnActiveTabChanged?.Invoke(tab);
    }

    private BrowserTab FindTabByPageId(string pageId)
    {
        foreach (BrowserTab tab in openTabs)
        {
            if (tab.Page.PageId == pageId)
                return tab;
        }
        return null;
    }
}