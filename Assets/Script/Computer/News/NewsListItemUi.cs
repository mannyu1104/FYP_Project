using UnityEngine;
using TMPro;

// News list item prefab, one row in the list of articles. Clicking it will show the article's detail view.
[RequireComponent(typeof(CustomButtonUi))]
public class NewsListItemUI : MonoBehaviour
{
    [SerializeField] private CustomButtonUi clickable;
    [SerializeField] private TMP_Text headlineText;

    private NewsArticleData articleData;

    private void Reset()
    {
        clickable = GetComponent<CustomButtonUi>();
    }

    // Bind this list item to a specific article, and set up the click callback to show that article's detail view.
    public void Bind(NewsArticleData article, NewsPageController owner)
    {
        UnsubscribeFromLocalization();
        articleData = article;

        SubscribeToLocalization();

        clickable.onLeftClick.RemoveAllListeners();
        clickable.onLeftClick.AddListener(() => owner.ShowArticleDetail(article));
    }

    private void SubscribeToLocalization()
    {
        if (articleData == null) return;
        articleData.Headline.StringChanged += UpdateHeadlineText;
    }

    private void UnsubscribeFromLocalization()
    {
        if (articleData == null) return;
        articleData.Headline.StringChanged -= UpdateHeadlineText;
    }

    private void UpdateHeadlineText(string value)
    {
        if (headlineText != null) headlineText.text = value;
    }

    private void OnDestroy()
    {
        UnsubscribeFromLocalization();
    }
}