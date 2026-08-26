using UnityEngine;
using TMPro;

// News list item prefab, one row in the list of articles. Clicking it will show the article's detail view.
[RequireComponent(typeof(CustomButtonUi))]
public class NewsListItemUI : MonoBehaviour
{
    [SerializeField] private CustomButtonUi clickable;
    [SerializeField] private TMP_Text headlineText;

    private void Reset()
    {
        clickable = GetComponent<CustomButtonUi>();
    }

    // Bind this list item to a specific article, and set up the click callback to show that article's detail view.
    public void Bind(NewsArticleData article, NewsPageController owner)
    {
        headlineText.text = article.Headline;

        clickable.onLeftClick.RemoveAllListeners();
        clickable.onLeftClick.AddListener(() => owner.ShowArticleDetail(article));
    }
}