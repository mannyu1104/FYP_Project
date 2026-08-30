using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WebPageContentController : MonoBehaviour
{
    public static WebPageContentController Instance { get; private set; }

    [Header("States (keep these GameObjects active - hide via CanvasGroup only)")]
    [SerializeField] private CanvasGroup newsStateView;         // PageLayoutType.News
    [SerializeField] private CanvasGroup socialMediaStateView;  // PageLayoutType.SocialMedia
    [SerializeField] private CanvasGroup searchResultsStateView; // PageLayoutType.SearchResults
    [SerializeField] private CanvasGroup orphanageHPStateView; // PageLayoutType.OrphanageHP
    [SerializeField] private CanvasGroup blueMorphoHPStateView; // PageLayoutType.BlueMorphoHP

    [Header("Sub-Controllers (reset to their default internal view on reopen)")]
    [SerializeField] private NewsPageController newsPageController;
    [SerializeField] private SocialMediaPageController socialMediaPageController;
    [SerializeField] private SearchResultsPageController searchResultsPageController;
    [SerializeField] private OrphanageHPPageController orphanageHPPageController;


    [Header("App Page Data (same assets used by BrowserAppButton)")]
    [SerializeField] private WebPageDataScriptableObject newsAppPageData;
    [SerializeField] private WebPageDataScriptableObject socialAppPageData;
    [SerializeField] private WebPageDataScriptableObject orphanageHPAppPageData;
    [SerializeField] private WebPageDataScriptableObject blueMorphoHPAppPageData;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        BrowserTabManager.Instance.OnActiveTabChanged += DisplayTab;

        HideAllStates();
    }

    private void OnDisable()
    {
        BrowserTabManager.Instance.OnActiveTabChanged -= DisplayTab;
    }

    private void DisplayTab(BrowserTab tab)
    {
        HideAllStates();

        if (tab == null) return;

        // Search-results tabs are plain IBrowserPage instances (one per search),
        // not WebPageDataScriptableObject assets - handle them first.
        if (tab.Page is SearchResultsTabPage searchPage)
        {
            ShowSearchResultsPage(searchPage);
            return;
        }

        if (tab.Page is not WebPageDataScriptableObject webPage) return;

        switch (webPage.LayoutType)
        {
            case PageLayoutType.News:
                ShowNewsPage(webPage);
                break;
            case PageLayoutType.SocialMedia:
                ShowSocialMediaPage(webPage);
                break;
            case PageLayoutType.OrphanageHP:
                ShowOrphanageHPPage(webPage);
                break;
            case PageLayoutType.BlueMorphoHP:
                ShowBlueMorphoHPPage(webPage);
                break;
        }
    }

    public void HideAllStates()
    {
        Hide(newsStateView);
        Hide(socialMediaStateView);
        Hide(searchResultsStateView);
        Hide(orphanageHPStateView);
        Hide(blueMorphoHPStateView);
    }

    private void ShowNewsPage(WebPageDataScriptableObject webPage)
    {
        Show(newsStateView);
        // Explicit call instead of relying on NewsPageController's OnEnable -
        // its GameObject no longer goes inactive/active, so OnEnable would
        // otherwise only ever fire once, at scene start.
        newsPageController.ShowList();
    }

    private void ShowSocialMediaPage(WebPageDataScriptableObject webPage)
    {
        Show(socialMediaStateView);
        socialMediaPageController.ShowFeed();
    }

    private void ShowSearchResultsPage(SearchResultsTabPage searchPage)
    {
        Show(searchResultsStateView);
        searchResultsPageController.DisplayResults(searchPage.Query, searchPage.Results);
    }

    private void ShowOrphanageHPPage(WebPageDataScriptableObject webPage)
    {
        Show(orphanageHPStateView);
        orphanageHPPageController.ShowList();
    }

    private void ShowBlueMorphoHPPage(WebPageDataScriptableObject webPage)
    {
        Show(blueMorphoHPStateView);
    }

    /// <summary>
    /// Opens the News app tab (exactly like clicking its BrowserAppButton) and
    /// jumps straight to the given article's detail view. Called from search
    /// results whose destinationType is NewsArticle.
    /// </summary>
    public void OpenNewsArticle(NewsArticleData article)
    {
        // OpenPage synchronously fires OnActiveTabChanged -> DisplayTab -> ShowNewsPage,
        // which calls newsPageController.ShowList(). We then override it below to
        // jump straight to the article instead of landing on the list.
        BrowserTabManager.Instance.OpenPage(newsAppPageData);
        newsPageController.ShowArticleDetail(article);
    }

    /// <summary>
    /// Opens the Social Media app tab and jumps straight to the given account's
    /// profile view. Called from search results whose destinationType is SocialProfile.
    /// </summary>
    public void OpenSocialProfile(SocialAccountData account)
    {
        BrowserTabManager.Instance.OpenPage(socialAppPageData);
        socialMediaPageController.ShowProfile(account);
    }

    private void Show(CanvasGroup group)
    {
        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;
    }

    private void Hide(CanvasGroup group)
    {
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
    }

    public void ResetActiveTab()
    {
        BrowserTabManager.Instance.ResetActiveTab();
    }
}