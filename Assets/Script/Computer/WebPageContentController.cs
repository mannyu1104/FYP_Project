using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WebPageContentController : MonoBehaviour
{
    [Header("States (keep these GameObjects active - hide via CanvasGroup only)")]
    [SerializeField] private CanvasGroup emptyStateView;       // no tabs open at all
    [SerializeField] private CanvasGroup mainStateView;        // PageLayoutType.Main
    [SerializeField] private CanvasGroup newsStateView;        // PageLayoutType.News
    [SerializeField] private CanvasGroup socialMediaStateView; // PageLayoutType.SocialMedia
    [SerializeField] private CanvasGroup tutorialStateView;    // PageLayoutType.Tutorial

    [Header("Main Page View References")]
    [SerializeField] private TMP_Text pageTitleText;
    [SerializeField] private TMP_Text pageBodyText;

    [Header("Sub-Controllers (reset to their default internal view on reopen)")]
    [SerializeField] private NewsPageController newsPageController;
    [SerializeField] private SocialMediaPageController socialMediaPageController;

    private void OnEnable()
    {
        BrowserTabManager.Instance.OnActiveTabChanged += DisplayTab;
        // In case a tab is already active when this panel is enabled
        // (e.g. player re-opens the browser after switching to Desktop).
        DisplayTab(BrowserTabManager.Instance.ActiveTab);
    }

    private void OnDisable()
    {
        BrowserTabManager.Instance.OnActiveTabChanged -= DisplayTab;
    }

    private void DisplayTab(BrowserTab tab)
    {
        HideAllStates();

        if (tab == null)
        {
            Show(emptyStateView);
            return;
        }

        if (tab.Page is not WebPageDataScriptableObject webPage)
        {
            // Fallback for anything implementing IBrowserPage without real
            // content yet (e.g. a PlaceholderPage used during early testing).
            Show(mainStateView);
            pageTitleText.text = tab.Page.TabTitle;
            pageBodyText.text = "(no content on this page yet)";
            return;
        }

        switch (webPage.LayoutType)
        {
            case PageLayoutType.Main:
                ShowMainPage(webPage);
                break;
            case PageLayoutType.News:
                ShowNewsPage(webPage);
                break;
            case PageLayoutType.SocialMedia:
                ShowSocialMediaPage(webPage);
                break;
            case PageLayoutType.Tutorial:
                ShowTutorialPage(webPage);
                break;
        }
    }

    public void HideAllStates()
    {
        Hide(emptyStateView);
        Hide(mainStateView);
        Hide(newsStateView);
        Hide(socialMediaStateView);
        Hide(tutorialStateView);
    }

    private void ShowMainPage(WebPageDataScriptableObject webPage)
    {
        Show(mainStateView);
        pageTitleText.text = webPage.TabTitle;
        pageBodyText.text = webPage.BodyText;
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

    private void ShowTutorialPage(WebPageDataScriptableObject webPage)
    {
        Show(tutorialStateView);

        // TODO: tutorial pages might be fully static (nothing to populate
        // from webPage at all), or might pull step text from it - depends
        // on how you plan to author tutorial content.
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