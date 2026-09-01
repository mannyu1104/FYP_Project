using TMPro;
using UnityEngine;

// Manages navigation within Orphanage Home Page
public class OrphanageHPPageController : ListDetailPageController<OrphanageAnnouncementData, OrphanageAnnouncementListItemUi>
{
    [Header("Detail View Fields")]
    [SerializeField] private TMP_InputField detailTitleText;
    [SerializeField] private TMP_InputField detailDateText;
    [SerializeField] private TMP_InputField detailContentText;

    [Header("Clue")]
    [SerializeField] private ClueRecordButton clueRecordButton;

    public void ShowAnnouncementDetail(OrphanageAnnouncementData announcement) => ShowDetail(announcement);

    private OrphanageAnnouncementData announcementData;

    protected override void PopulateDetail(OrphanageAnnouncementData announcement)
    {
        UnsubscribeFromLocalization();
        announcementData = announcement;

        SubscribeToLocalization();

        detailContentText.verticalScrollbar.value = 0f; // Scroll to top
    }

    protected override void OnDetailShown(OrphanageAnnouncementData announcement)
    {
        clueRecordButton.SetSource(announcement);
    }

    protected override void BindListItem(OrphanageAnnouncementListItemUi listItemUI, OrphanageAnnouncementData announcement)
    {
        listItemUI.Bind(announcement, this);
    }

    private void SubscribeToLocalization()
    {
        if (announcementData == null) return;
        announcementData.Title.StringChanged += UpdateTitleText;
        announcementData.Date.StringChanged += UpdateDateText;
        announcementData.Content.StringChanged += UpdateContentText;
    }

    private void UnsubscribeFromLocalization()
    {
        if (announcementData == null) return;
        announcementData.Title.StringChanged -= UpdateTitleText;
        announcementData.Date.StringChanged -= UpdateDateText;
        announcementData.Content.StringChanged -= UpdateContentText;
    }

    private void UpdateTitleText(string value)
    {
        if (detailTitleText != null) detailTitleText.text = value;
    }

    private void UpdateDateText(string value)
    {
        if (detailDateText != null) detailDateText.text = value;
    }

    private void UpdateContentText(string value)
    {
        if (detailContentText != null) detailContentText.text = value;
    }

    private void OnDestroy()
    {
        UnsubscribeFromLocalization();
    }
}