using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "NewTutorialClue", menuName = "ScriptableObject/Tutorial Clue")]
public class TutorialClueData : ClueSourceData
{
    [SerializeField] private LocalizedString tutorialClueName;
    [SerializeField] private LocalizedString tutorialClueDescription;
    [SerializeField] private Sprite clueImage; // Optional image for the clue, can be null if not needed
    [SerializeField] private Sprite clueAvatar;

    public LocalizedString TutorialClueName => tutorialClueName;
    public LocalizedString TutorialClueDescription => tutorialClueDescription;
    public Sprite ClueImage => clueImage;
    public Sprite ClueAvatar => clueAvatar;
}