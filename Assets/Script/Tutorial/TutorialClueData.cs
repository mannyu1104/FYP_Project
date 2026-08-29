using UnityEngine;

[CreateAssetMenu(fileName = "NewTutorialClue", menuName = "ScriptableObject/Tutorial Clue")]
public class TutorialClueData : ClueSourceData
{
    [SerializeField] private string tutorialClueName;
    [SerializeField][TextArea(6, 10)] private string tutorialClueDescription;
    [SerializeField] private Sprite clueImage;

    public string TutorialClueName => tutorialClueName;
    public string TutorialClueDescription => tutorialClueDescription;
    public Sprite ClueImage => clueImage;
}