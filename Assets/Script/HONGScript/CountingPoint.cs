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

    private float Score;
    private int ScoreShowin;

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
        DragableItem[] items = FindObjectsByType<DragableItem>();
        Score = 0;

        foreach (DragableItem item in items)
        {
            if (CorrectIDList.Contains(item.thisID))
            {
                if (item.thisUsed == true)
                {
                    Score += 100f / (CorrectIDList.Count + WrongIDList.Count);
                }
                else if (item.thisUsed == false) 
                {
                    Score -= 100f / ((CorrectIDList.Count + WrongIDList.Count)* 2f);
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
}
