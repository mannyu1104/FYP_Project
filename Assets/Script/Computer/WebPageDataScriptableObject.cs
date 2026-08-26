using UnityEngine;

public enum PageLayoutType
{
    Main,
    News,
    SocialMedia,
    Announcement,
    Tutorial,
}

[CreateAssetMenu(fileName = "NewWebPage", menuName = "ScriptableObject/Web Page Data")]
public class WebPageDataScriptableObject : ScriptableObject, IBrowserPage
{
    [Header("Tab Info (shown in the tab bar)")]
    [SerializeField] private string pageId;
    [SerializeField] private string tabTitle;
    [SerializeField] private Sprite tabIcon;

    [Header("Page Content (shown in the content area)")]
    [SerializeField] private PageLayoutType layoutType;
    [SerializeField][TextArea(5, 20)] private string bodyText;

    // Later: search keywords for the address bar, comment list, etc.
    // can be added here without touching anything else in the tab system.

    public string PageId => pageId;
    public string TabTitle => tabTitle;
    public Sprite TabIcon => tabIcon;
    public PageLayoutType LayoutType => layoutType;
    public string BodyText => bodyText;
}