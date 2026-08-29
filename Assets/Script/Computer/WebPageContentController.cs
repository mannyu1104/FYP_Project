using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WebPageContentController : MonoBehaviour
{
    [Header("States (keep these GameObjects active - hide via CanvasGroup only)")]
    [SerializeField] private CanvasGroup newsStateView;        // PageLayoutType.News
    [SerializeField] private CanvasGroup socialMediaStateView; // PageLayoutType.SocialMedia


    [Header("Sub-Controllers (reset to their default internal view on reopen)")]
    [SerializeField] private NewsPageController newsPageController;
    [SerializeField] private SocialMediaPageController socialMediaPageController;

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

        if (tab.Page is not WebPageDataScriptableObject webPage) return;

        switch (webPage.LayoutType)
        {
            case PageLayoutType.News:
                ShowNewsPage(webPage);
                break;
            case PageLayoutType.SocialMedia:
                ShowSocialMediaPage(webPage);
                break;
        }
    }

    public void HideAllStates()
    {
        Hide(newsStateView);
        Hide(socialMediaStateView);
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