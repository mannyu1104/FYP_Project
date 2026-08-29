using TMPro;
using UnityEngine;

// News list item prefab, one row in the list of articles. Clicking it will show the article's detail view.
[RequireComponent(typeof(CustomButtonUi))]
public class OrphanageAnnouncementListItemUi : MonoBehaviour
{
    [SerializeField] private CustomButtonUi clickable;
    [SerializeField] private TMP_Text announcementTitleText;

    private void Reset()
    {
        clickable = GetComponent<CustomButtonUi>();
    }

    // Bind this list item to a specific article, and set up the click callback to show that article's detail view.
    public void Bind(OrphanageAnnouncementData announcement, OrphanageHPPageController owner)
    {
        announcementTitleText.text = announcement.Title;

        clickable.onLeftClick.RemoveAllListeners();
        clickable.onLeftClick.AddListener(() => owner.ShowAnnouncementDetail(announcement));
    }
}
