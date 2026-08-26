using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

// Manages navigation within news pages
public class NewsPageController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private List<NewsArticleData> articles;

    [Header("List View")]
    [SerializeField] private GameObject listView;
    [SerializeField] private Transform listContainer; // parent with a Vertical Layout Group
    [SerializeField] private NewsListItemUI listItemPrefab;

    [Header("Detail View")]
    [SerializeField] private GameObject detailView;
    [SerializeField] private TMP_Text detailHeadlineText;
    [SerializeField] private TMP_Text detailDateText;
    [SerializeField] private TMP_Text detailContentText;
    [SerializeField] private ScrollRect detailContentScrollRect;
    [SerializeField] private CustomButtonUi backButton; 

    private void Awake()
    {
        backButton.onLeftClick.AddListener(ShowList);
        BuildListOnce();
    }

    private void OnEnable()
    {
        // Start from list view every time the player opens the News tab
        ShowList();
    }

    public void ShowList()
    {
        detailView.SetActive(false);
        listView.SetActive(true);
    }

    public void ShowArticleDetail(NewsArticleData article)
    {
        listView.SetActive(false);
        detailView.SetActive(true);

        detailHeadlineText.text = article.Headline;
        detailDateText.text = article.Date;
        detailContentText.text = article.Content;
        detailContentScrollRect.verticalNormalizedPosition = 1f; // scroll to top
    }

    private void BuildListOnce()
    {
        foreach (NewsArticleData article in articles)
        {
            NewsListItemUI item = Instantiate(listItemPrefab, listContainer);
            item.Bind(article, this);
        }
    }
}