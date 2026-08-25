using UnityEngine;

[CreateAssetMenu(fileName = "WebPageDataScriptableObject", menuName = "ScriptableObject/WebPageData")]
public class WebPageDataScriptableObject : ScriptableObject, IBrowserPage
{
    [SerializeField] private string pageId;
    [SerializeField] private string tabTitle;
    [SerializeField] private Sprite tabIcon;

    public string PageId => pageId;
    public string TabTitle => tabTitle;
    public Sprite TabIcon => tabIcon;
}

