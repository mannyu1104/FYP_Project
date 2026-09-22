using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class ClueSubmission : MonoBehaviour
{
    [SerializeField] private ClueBoardUI clueBoard;
    [SerializeField] private Transform judgmentContent; // 判定区's content container
    [SerializeField] private Button submitButton;

    [Header("Score Panel")]
    [SerializeField] private CanvasGroup scorePanelCanvasGroup;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Button closeScorePanelButton;

    [Header("Scoring")]
    [SerializeField] private int pointsPerCorrectPlacement = 10;
    [SerializeField] private int penaltyPerMissedCredible = 10;
    [SerializeField] private int penaltyPerWrongPlacement = 15;

    [SerializeField] private CountingPoint countingPoint;


    private void Awake()
    {
        if (submitButton != null)
        {
            submitButton.onClick.AddListener(Submit);
        }

    }

    private void OnEnable()
    {
        CountingPoint.ScoreShown += Submit;
    }

    public void Submit()
    {
        //int score = 0;
        //int correctlyPlaced = 0;
        //int missedCredible = 0;
        //int wronglyPlaced = 0;

        //foreach (ClueBoardEntryUI entry in clueBoard.Entries)
        //{
        //    bool inJudgmentZone = entry.transform.parent == judgmentContent;

        //    switch (entry.Credibility)
        //    {
        //        case ClueCredibility.Credible:
        //            if (inJudgmentZone)
        //            {
        //                score += pointsPerCorrectPlacement;
        //                correctlyPlaced++;
        //            }
        //            else
        //            {
        //                score -= penaltyPerMissedCredible;
        //                missedCredible++;
        //            }
        //            break;

        //        case ClueCredibility.NotCredible:
        //            if (inJudgmentZone)
        //            {
        //                score -= penaltyPerWrongPlacement;
        //                wronglyPlaced++;
        //            }
        //            break;

        //        case ClueCredibility.Neutral:
        //        default:
        //            break; // never affects score, placed or not
        //    }
        //}

        //score = Mathf.Max(score, 0);

        //resultText.text = $"{score} / 100";

        countingPoint.CountingPoints();
        //resultText.text = countingPoint.CountingPoints();

        ShowScorePanel();
    }

    private void ShowScorePanel()
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
    }
}