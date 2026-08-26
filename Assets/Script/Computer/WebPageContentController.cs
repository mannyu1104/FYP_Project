using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WebPageContentController : MonoBehaviour
{
    [Header("States")]
    [SerializeField] private GameObject emptyStateView;       // no tabs open at all
    [SerializeField] private GameObject mainStateView;        
    [SerializeField] private GameObject newsStateView;        
    [SerializeField] private GameObject socialMediaStateView; 
    [SerializeField] private GameObject tutorialStateView;    

    [Header("News Page View References")]
    [SerializeField] private TMP_Text newsPageTitleText;
    [SerializeField] private TMP_Text newsPageBodyText;

    [Header("Social Media Page View References")]
    [SerializeField] private TMP_Text socialMediaPageTitleText;
    [SerializeField] private TMP_Text socialMediaPageBodyText;

    [Header("Tutorial Page View References")]
    [SerializeField] private TMP_Text tutorialPageTitleText;
    [SerializeField] private TMP_Text tutorialPageBodyText;

    // News/SocialMedia/Tutorial reference fields go here as their layouts
    // are designed - see the TODOs in each Show...Page method below.

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
            //emptyStateView.SetActive(true);
            HideAllStates();
            return;
        }

        if (tab.Page is not WebPageDataScriptableObject webPage)
        {
            // Fallback for anything implementing IBrowserPage without real
            // content yet (e.g. a PlaceholderPage used during early testing).
            //mainStateView.SetActive(true);
            //pageTitleText.text = tab.Page.TabTitle;
            //pageBodyText.text = "(no content on this page yet)";
            HideAllStates();
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
        emptyStateView.SetActive(false);
        mainStateView.SetActive(false);
        newsStateView.SetActive(false);
        socialMediaStateView.SetActive(false);
        tutorialStateView.SetActive(false);

    }

    private void ShowMainPage(WebPageDataScriptableObject webPage)
    {
        mainStateView.SetActive(true);
        //pageTitleText.text = webPage.TabTitle;
        //pageBodyText.text = webPage.BodyText;
    }

    private void ShowNewsPage(WebPageDataScriptableObject webPage)
    {
        newsStateView.SetActive(true);

        newsPageTitleText.text = webPage.TabTitle;
        newsPageBodyText.text = webPage.BodyText;

        // TODO: once the news layout's own fields exist (headline, date,
        // author, comment list, etc.), add [SerializeField] references for
        // them above and populate them here the same way ShowMainPage does.
    }

    private void ShowSocialMediaPage(WebPageDataScriptableObject webPage)
    {
        socialMediaStateView.SetActive(true);

        socialMediaPageTitleText.text = webPage.TabTitle;
        socialMediaPageBodyText.text = webPage.BodyText;

        // TODO: social posts will likely need poster name/avatar, post text,
        // and a comment list (see the CommentData idea discussed earlier) -
        // add the reference fields and populate them here once that's built.
    }

    private void ShowTutorialPage(WebPageDataScriptableObject webPage)
    {
        tutorialStateView.SetActive(true);

        tutorialPageTitleText.text = webPage.TabTitle;
        tutorialPageBodyText.text = webPage.BodyText;

        // TODO: tutorial pages might be fully static (nothing to populate
        // from webPage at all), or might pull step text from it - depends
        // on how you plan to author tutorial content.
    }

    public void ResetActiveTab()
    {
        BrowserTabManager.Instance.ResetActiveTab();
    }
}