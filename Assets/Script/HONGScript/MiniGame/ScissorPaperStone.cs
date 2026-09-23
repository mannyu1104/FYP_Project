using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScissorPaperStone : MonoBehaviour
{
    public int playerselection;
    public bool playing, UIback;
    public Image OpponentSelection;
    public Sprite ScissorUI, PaperUI, StoneUI, Win, Lose;
    public Image[] WinningUISlots;
    public GameObject[] SelectionUI;
    public event Action<int, int> RoundResolved;
    public event Action<bool> MatchFinished;
    private int rounds, wins, resultMask;
    private bool finished;

    public void RestoreMatch(int played, int won, int mask)
    {
        StopAllCoroutines();
        rounds = Mathf.Clamp(played, 0, 3); wins = Mathf.Clamp(won, 0, rounds);
        resultMask = mask; finished = rounds >= 3; playing = false;
        for (int i = 0; i < WinningUISlots.Length; i++)
            if (WinningUISlots[i] != null) WinningUISlots[i].sprite = i < rounds ? ((mask & (1 << i)) != 0 ? Win : Lose) : null;
        ShowChoices(!finished);
    }
    private void ShowChoices(bool show)
    {
        foreach (GameObject choice in SelectionUI) if (choice != null) choice.SetActive(show);
    }
    public void Paper() => Choose(2);
    public void Stone() => Choose(3);
    public void Scissor() => Choose(1);
    private void Choose(int choice)
    {
        if (playing || finished) return;
        playerselection = choice; playing = true; ShowChoices(false);
        StartCoroutine(ResolveRound(choice));
    }
    private IEnumerator ResolveRound(int choice)
    {
        Sprite[] sprites = { ScissorUI, PaperUI, StoneUI };
        int opponent = UnityEngine.Random.Range(1, 4);
        for (float elapsed = 0; elapsed < 1.2f; elapsed += Time.unscaledDeltaTime)
        {
            if (OpponentSelection != null) OpponentSelection.sprite = sprites[UnityEngine.Random.Range(0, 3)];
            yield return null;
        }
        if (OpponentSelection != null) OpponentSelection.sprite = sprites[opponent - 1];
        playing = false;
        if (choice != opponent)
        {
            bool won = (choice == 1 && opponent == 2) || (choice == 2 && opponent == 3) || (choice == 3 && opponent == 1);
            if (won) { wins++; resultMask |= 1 << rounds; }
            if (rounds < WinningUISlots.Length && WinningUISlots[rounds] != null) WinningUISlots[rounds].sprite = won ? Win : Lose;
            rounds++;
            RoundResolved?.Invoke(rounds, resultMask);
        }
        finished = rounds >= 3;
        ShowChoices(!finished);
        if (finished) MatchFinished?.Invoke(wins >= 2);
    }
}
