using TMPro;
using UnityEngine;

// Manages navigation within news pages
public class NewsPageController : ListDetailPageController<NewsArticleData, NewsListItemUI>
{
    [Header("Detail View Fields")]
    [SerializeField] private TMP_InputField detailHeadlineText;
    [SerializeField] private TMP_InputField detailDateText;
    [SerializeField] private TMP_InputField detailContentText;

    [Header("Clue")]
    [SerializeField] private ClueRecordButton clueRecordButton;

    public void ShowArticleDetail(NewsArticleData article) => ShowDetail(article);

    protected override void PopulateDetail(NewsArticleData article)
    {
        detailHeadlineText.text = article.Headline;
        detailDateText.text = article.Date;
        detailContentText.text = article.Content;
        detailContentText.verticalScrollbar.value = 0f; // Scroll to top
    }

    protected override void OnDetailShown(NewsArticleData article)
    {
        clueRecordButton.SetSource(article);
    }

    protected override void BindListItem(NewsListItemUI listItemUI, NewsArticleData article)
    {
        listItemUI.Bind(article, this);
    }
}