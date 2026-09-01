using TMPro;
using UnityEngine;

// News list item prefab, one row in the list of articles. Clicking it will show the article's detail view.
[RequireComponent(typeof(CustomButtonUi))]
public class OrphanageAnnouncementListItemUi : MonoBehaviour
{
    [SerializeField] private CustomButtonUi clickable;
    [SerializeField] private TMP_Text announcementTitleText;

    private OrphanageAnnouncementData announcementData;

    private void Reset()
    {
        clickable = GetComponent<CustomButtonUi>();
    }

    // Bind this list item to a specific article, and set up the click callback to show that article's detail view.
    public void Bind(OrphanageAnnouncementData announcement, OrphanageHPPageController owner)
    {
        UnsubscribeFromLocalization();
        announcementData = announcement;

        SubscribeToLocalization();

        clickable.onLeftClick.RemoveAllListeners();
        clickable.onLeftClick.AddListener(() => owner.ShowAnnouncementDetail(announcement));
    }

    private void SubscribeToLocalization()
    {
        if (announcementData == null) return;
        announcementData.Title.StringChanged += UpdateTitleText;
    }

    private void UnsubscribeFromLocalization()
    {
        if (announcementData == null) return;
        announcementData.Title.StringChanged -= UpdateTitleText;
    }

    private void UpdateTitleText(string value)
    {
        if (announcementTitleText != null) announcementTitleText.text = value;
    }

    private void OnDestroy()
    {
        UnsubscribeFromLocalization();
    }
}
