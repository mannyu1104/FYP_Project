using UnityEngine;

// Defines a case in the game.
[CreateAssetMenu(fileName = "NewCase", menuName = "ScriptableObject/Case Definition")]
public class CaseDefinition : ScriptableObject
{
    [SerializeField] private string caseName;
    public string CaseName => caseName;
}