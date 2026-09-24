using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Wire the "Submit" button's OnClick() to Submit(). Put this on the Clue
// Board panel alongside a Text for the outcome.
public class ClueSubmission : MonoBehaviour
{
    public static event System.Action<ClueSubmission> AssessmentClosed;
    public static event System.Action<ClueSubmission> AssessmentScored;
    [SerializeField] private ClueBoardUI clueBoard;
    [SerializeField] private Transform judgmentContent; // 判定区's content container
    public Transform JudgmentContent => judgmentContent;
    public ClueBoardUI Board => clueBoard;
    [SerializeField] private Button submitButton;

    [Header("Score Panel")]
    [SerializeField] private CanvasGroup scorePanelCanvasGroup;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Button closeScorePanelButton;

    [Header("Scoring")]
    [SerializeField] private int pointsPerCorrectPlacement = 10;
    [SerializeField] private int penaltyPerMissedCredible = 10;
    [SerializeField] private int penaltyPerWrongPlacement = 15;

    // A clue counts as "the player marked it credible" if it currently sits
    // inside the judgment zone, regardless of how it got dragged there.
    //   in judgment zone + actually credible     -> +pointsPerCorrectSelection
    //   in judgment zone + actually not credible -> -penaltyPerWrongSelection
    //   credible but still unsorted              -> missed-credible penalty
    // Only recorded clues participate in this assessment.

    // Completion depends on the submitted assessment, not a collection or score threshold.
    public bool HasScoredSubmission { get; private set; }

    private bool HasRecordedClues()
    {
        if (clueBoard == null) return false;
        foreach (var entry in clueBoard.Entries)
            if (entry != null) return true;
        return false;
    }

    private void Awake()
    {
        if (submitButton != null)
        {
            submitButton.onClick.AddListener(Submit);
        }
    }

    public void Submit()
    {
        HasScoredSubmission = false;
        if (clueBoard != null) clueBoard.RefreshEntries();
        if (!HasRecordedClues()) return;
        int score = 0;
        int maximumScore = 0;
        int correctlyPlaced = 0;
        int missedCredible = 0;
        int wronglyPlaced = 0;

        foreach (ClueBoardEntryUI entry in clueBoard.Entries)
        {
            if (entry == null) continue;
            bool inJudgmentZone = entry.transform.parent == judgmentContent;

            switch (entry.Credibility)
            {
                case ClueCredibility.Credible:
                    maximumScore += pointsPerCorrectPlacement;
                    if (inJudgmentZone)
                    {
                        score += pointsPerCorrectPlacement;
                        correctlyPlaced++;
                    }
                    else
                    {
                        score -= penaltyPerMissedCredible;
                        missedCredible++;
                    }
                    break;

                case ClueCredibility.NotCredible:
                    if (inJudgmentZone)
                    {
                        score -= penaltyPerWrongPlacement;
                        wronglyPlaced++;
                    }
                    break;

                case ClueCredibility.Neutral:
                default:
                    break; // never affects score, placed or not
            }
        }

        score = maximumScore > 0
            ? Mathf.Clamp(Mathf.RoundToInt(100f * score / maximumScore), 0, 100)
            : 0;

        //resultText.text = $"{score} / 100";
        HasScoredSubmission = true;

        ShowScorePanel(score);
        AssessmentScored?.Invoke(this);
    }

    private void ShowScorePanel(int score)
    {
        scorePanelCanvasGroup.alpha = 1f;
        scorePanelCanvasGroup.interactable = true;
        scorePanelCanvasGroup.blocksRaycasts = true;

        closeScorePanelButton.onClick.RemoveAllListeners();
        closeScorePanelButton.onClick.AddListener(HideScorePanel);
    }

    private void HideScorePanel()
    {
        scorePanelCanvasGroup.alpha = 0f;
        scorePanelCanvasGroup.interactable = false;
        scorePanelCanvasGroup.blocksRaycasts = false;
        if (HasScoredSubmission) AssessmentClosed?.Invoke(this);
        HasScoredSubmission = false;
    }
}
