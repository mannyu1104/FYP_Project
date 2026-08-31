using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "NewOrphanageAnnouncement", menuName = "ScriptableObject/Orphanage Announcement")]
public class OrphanageAnnouncementData : ClueSourceData
{
    [Header("Orphanage Announcement Details")]
    [SerializeField] private LocalizedString title;
    [SerializeField] private LocalizedString date;
    [SerializeField] private LocalizedString content;

    public LocalizedString Title => title;
    public LocalizedString Date => date;
    public LocalizedString Content => content;
}
