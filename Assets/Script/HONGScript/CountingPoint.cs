using UnityEngine;
using System.Collections.Generic;
using System;
using TMPro;

public class CountingPoint : MonoBehaviour
{
    public static event Action ScoreShown;
    public static bool HasScoreBeenShown { get; private set; }

    [SerializeField] public List<int> CorrectIDList = new List<int>();
    [SerializeField] public List<int> WrongIDList = new List<int>();
    [SerializeField] private TMP_Text ScoreShow;
    [SerializeField] private TMP_Text AfterShow;

    private float Score;
    private int ScoreShowin;

    private void Awake()
    {
        var button = GetComponent<UnityEngine.UI.Button>();
        if (button == null) return;
        // Gate the entire persistent click, including the score panel's SetActive call.
        var originalClick = button.onClick;
        button.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
        button.onClick.AddListener(() => { if (HasCollectedClues()) originalClick.Invoke(); });
    }

    private bool HasCollectedClues()
    {
        foreach (var item in FindObjectsByType<DragableItem>(FindObjectsInactive.Include))
        {
            item.EnsureInitialized();
            if ((CorrectIDList.Contains(item.thisID) || WrongIDList.Contains(item.thisID)) &&
                (item.thisGet || item.thisUsed)) return true;
        }
        return false;
    }

    private void Start()
    {
        Score = 0;
    }

    public static void ResetTutorialCompletion()
    {
        HasScoreBeenShown = false;
    }

    public void CountingPoints()
    {
        if (!HasCollectedClues()) return;
        DragableItem[] items = FindObjectsByType<DragableItem>();
        Score = 0;

        foreach (DragableItem item in items)
        {
            if (!CorrectIDList.Contains(item.thisID) && !WrongIDList.Contains(item.thisID)) continue;
            if (CorrectIDList.Contains(item.thisID))
            {
                if (item.thisUsed == true)
                {
                    Score += 100f / (CorrectIDList.Count + WrongIDList.Count);
                }
                else if (item.thisUsed == false)
                {
                    Score -= 100f / ((CorrectIDList.Count + WrongIDList.Count) * 2f);
                }
            }
            else if (WrongIDList.Contains(item.thisID))
            {
                if (item.thisUsed == false)
                {
                    Score += 100f / (CorrectIDList.Count + WrongIDList.Count);
                }
                else if (item.thisUsed == true)
                {
                    Score -= 100f / ((CorrectIDList.Count + WrongIDList.Count) * 2f);
                }
            }
            item.thisTuto = true;
            Debug.Log(Score);
        }

        ScoreShowin = (int)Score;

        if (ScoreShowin < 0)
        {
            ScoreShowin = 0;
        }

        ScoreShow.text = "Score: " + ScoreShowin;
        HasScoreBeenShown = true;
        ScoreShown?.Invoke();
    }

    public void AfterShowCongrats()
    {
        AfterShow.text = "Congratulations !!!!" + ", " + "You Have Done Tutorial" + ", and your score is:" + ScoreShowin;
    }

}


