using UnityEngine;

// Base class for any ScriptableObject that can be recorded as important clue
public abstract class ClueSourceData : ScriptableObject
{
    [Header("Clue Details")]
    [SerializeField] private string clueTitle;
    [SerializeField][TextArea(6, 10)] private string clueSummary;
    [SerializeField] private bool isCredible;

    public string ClueTitle => clueTitle;
    public string ClueSummary => clueSummary;
    public bool IsCredible => isCredible;
}