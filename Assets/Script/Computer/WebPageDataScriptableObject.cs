using UnityEngine;
using UnityEngine.Localization;

public enum PageLayoutType
{
    News,
    SocialMedia,
    OrphanageHP,
    BlueMorphoHP,
}

[CreateAssetMenu(fileName = "NewWebPage", menuName = "ScriptableObject/Web Page Data")]
public class WebPageDataScriptableObject : ScriptableObject, IBrowserPage
{
    [Header("Tab Info")]
    [SerializeField] private string pageId;
    [SerializeField] private LocalizedString tabTitle;
    [SerializeField] private Sprite tabIcon;

    [Header("Page Content")]
    [SerializeField] private PageLayoutType layoutType;


    public string PageId => pageId;
    public LocalizedString TabTitle => tabTitle;
    public Sprite TabIcon => tabIcon;
    public PageLayoutType LayoutType => layoutType;

}