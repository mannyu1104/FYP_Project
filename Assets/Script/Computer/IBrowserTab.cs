using UnityEngine;

public interface IBrowserPage
{
    /// Id used to detect "is this page already open in a tab".
    string PageId { get; }

    /// Text shown on the tab.
    string TabTitle { get; }

    /// Icon shown on the tab.
    Sprite TabIcon { get; }
}