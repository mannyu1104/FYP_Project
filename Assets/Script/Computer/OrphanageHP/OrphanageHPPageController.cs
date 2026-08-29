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

    protected override void PopulateDetail(OrphanageAnnouncementData announcement)
    {
        detailTitleText.text = announcement.Title;
        detailDateText.text = announcement.Date;
        detailContentText.text = announcement.Content;
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
}