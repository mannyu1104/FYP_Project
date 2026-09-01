using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class SearchBarController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField searchInputField;
    [SerializeField] private Button searchButton;

    [Header("Tab Display")]
    [Tooltip("Icon shown on the tab for search-results tabs.")]
    [SerializeField] private Sprite searchTabIcon;
    [SerializeField] private LocalizedString tabTitleFormat;

    private void Awake()
    {
        if (searchButton != null)
        {
            searchButton.onClick.AddListener(OnSearchSubmitted);
        }

        if (searchInputField != null)
        {
            // Allow pressing Enter to trigger the search as well.
            searchInputField.onSubmit.AddListener(_ => OnSearchSubmitted());
        }
    }

    private void OnSearchSubmitted()
    {
        if (searchInputField == null) return;

        string query = searchInputField.text;
        if (string.IsNullOrWhiteSpace(query)) return;

        var results = SearchManager.Instance.PerformSearch(query);

        SearchResultsTabPage searchPage = new SearchResultsTabPage(query, results, searchTabIcon, tabTitleFormat);
        BrowserTabManager.Instance.OpenPage(searchPage);

        searchInputField.text = string.Empty;
    }
}