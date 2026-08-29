using UnityEngine;

[CreateAssetMenu(fileName = "NewOrphanageAnnouncement", menuName = "ScriptableObject/Orphanage Announcement")]
public class OrphanageAnnouncementData : ClueSourceData
{
    [SerializeField] private string title;
    [SerializeField] private string date;
    [SerializeField][TextArea(5, 20)] private string content;

    public string Title => title;
    public string Date => date;
    public string Content => content;
}
