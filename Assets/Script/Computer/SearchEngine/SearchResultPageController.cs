using System.Collections.Generic;
using TMPro;
using Unity.Pipeline.Editor.Commands.Navigation;
using UnityEngine;
using UnityEngine.UI;

// Displays the results list for one search-results tab.
public class SearchResultsPageController : MonoBehaviour
{
    [Header("List Setup")]
    [SerializeField] private Transform resultsListContainer;
    [SerializeField] private SearchResultItemUI resultItemPrefab;

    [Header("Empty State")]
    [SerializeField] private GameObject noResultsPanel;

    [Header("Query Display")]
    [SerializeField] private TMP_Text queryLabel;

    private readonly List<SearchResultItemUI> spawnedItems = new List<SearchResultItemUI>();

    /// <summary>
    /// Called by WebPageContentController right after this tab becomes active.
    /// </summary>
    public void DisplayResults(string query, List<SearchResultEntryData> results)
    {
        if (queryLabel != null)
        {
            queryLabel.text = $"搜索结果：\"{query}\"";
        }

        ClearSpawnedItems();

        bool hasResults = results != null && results.Count > 0;
        if (noResultsPanel != null)
        {
            noResultsPanel.SetActive(!hasResults);
        }

        if (hasResults)
        {
            foreach (SearchResultEntryData entry in results)
            {
                SearchResultItemUI item = Instantiate(resultItemPrefab, resultsListContainer);
                item.Setup(entry, OnResultClicked);
                spawnedItems.Add(item);
            }
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(resultsListContainer.GetComponent<RectTransform>());
    }

    private void ClearSpawnedItems()
    {
        foreach (SearchResultItemUI item in spawnedItems)
        {
            if (item != null) Destroy(item.gameObject);
        }
        spawnedItems.Clear();
    }

    /// <summary>
    /// Routes a clicked search result to its destination. Which field is populated
    /// on the entry IS the type - checked in a fixed priority order. (OnValidate on
    /// SearchResultEntryData warns at edit-time if more than one or none are set,
    /// so in practice exactly one of these three will ever be true.)
    /// </summary>
    private void OnResultClicked(SearchResultEntryData entry)
    {
        if (entry == null) return;

        if (entry.pageData != null)
        {
            // Opened exactly the same way a BrowserAppButton opens an app.
            BrowserTabManager.Instance.OpenPage(entry.pageData);
        }
        else if (entry.newsArticleRef != null)
        {
            WebPageContentController.Instance.OpenNewsArticle(entry.newsArticleRef);
        }
        else if (entry.socialProfileRef != null)
        {
            WebPageContentController.Instance.OpenSocialProfile(entry.socialProfileRef);
        }
        else
        {
            Debug.LogError($"[SearchResultsPageController] '{entry.resultTitle}' has no destination assigned.");
        }
    }
}