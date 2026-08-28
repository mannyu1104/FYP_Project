using UnityEngine;
using UnityEngine.UI;

// Wire the "Submit" button's OnClick() to Submit(). Put this on the Clue
// Board panel alongside a Text for the outcome.
public class ClueSubmission : MonoBehaviour
{
    [SerializeField] private ClueBoardUI clueBoard;
    [SerializeField] private Transform judgmentContent; // 判定区's content container
    [SerializeField] private Text resultText; // swap for TMP_Text if needed

    [Header("Scoring")]
    [SerializeField] private int pointsPerCorrectSelection = 10;
    [SerializeField] private int penaltyPerWrongSelection = 15;

    // A clue counts as "the player marked it credible" if it currently sits
    // inside the judgment zone, regardless of how it got dragged there.
    //   in judgment zone + actually credible     -> +pointsPerCorrectSelection
    //   in judgment zone + actually not credible -> -penaltyPerWrongSelection
    //   still in the unsorted zone               -> not counted either way
    public void Submit()
    {
        int score = 0;
        int correct = 0;
        int wrong = 0;

        foreach (ClueBoardEntryUI entry in clueBoard.Entries)
        {
            bool inJudgmentZone = entry.transform.parent == judgmentContent;
            if (!inJudgmentZone)
            {
                continue;
            }

            if (entry.GroundTruthIsCredible)
            {
                score += pointsPerCorrectSelection;
                correct++;
            }
            else
            {
                score -= penaltyPerWrongSelection;
                wrong++;
            }
        }

        score = Mathf.Max(score, 0);

        resultText.text = $"得分：{score}\n选对的可信线索：{correct}\n选错的不可信线索：{wrong}";
    }
}