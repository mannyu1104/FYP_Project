using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEditor.Networking.PlayerConnection;
using Unity.VisualScripting;
using TMPro;

public class ScissorPaperStone : MonoBehaviour
{
    private int winningcount;
    private int playingcount;
    public int playerselection;
    private float animationtime = 2f;
    private float playedtime;
    public bool playing;
    private int ChildSelection;
    private bool started;
    public bool UIback;

    [SerializeField] public Image OpponentSelection;
    [SerializeField] public Sprite ScissorUI;
    [SerializeField] public Sprite PaperUI;
    [SerializeField] public Sprite StoneUI;

    [SerializeField] public Sprite Win;
    [SerializeField] public Sprite Lose;
    public Image[] WinningUISlots;
    public GameObject[] SelectionUI;

    List<int> TheSelection = new List<int>() {1, 2, 3};
    List<Sprite> OpponentUI = new List<Sprite>();

    // 1 = scissor
    // 2 = paper
    // 3 = stone

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        started = false;
        winningcount = 0;
        playingcount = 0;
        playedtime = 0;
        playerselection = 0;
        ChildSelection = 0;
        playing = false;
        UIback = true;

        if (OpponentUI.Count == 0)
        {
            OpponentUI.Add(ScissorUI);
            OpponentUI.Add(PaperUI);
            OpponentUI.Add(StoneUI);
        }

        foreach (Image image in WinningUISlots)
        {
            image.sprite = null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (playing)
        {
            playedtime += Time.deltaTime;
            if (playedtime <= animationtime)
            {
                Sprite UIimage = OpponentUI[Random.Range(0, OpponentUI.Count)];
                OpponentSelection.sprite = UIimage;
            }
            else
            {
                started = true;
                playing = false;
            }
        }

        if (!playing && started)
        {
            OpponentSelection.sprite = OpponentUI[ChildSelection - 1];

            if (ChildSelection == playerselection)
            {
                Debug.Log("Draw");
                started = false;
                return;
            }
            else if (ChildSelection != playerselection)
            {
                if (ChildSelection == 1)
                {
                    if (playerselection == 2)
                    {
                        WinningUISlots[playingcount].sprite = Lose;
                        started = false;
                    }
                    else if (playerselection == 3)
                    {
                        WinningUISlots[playingcount].sprite = Win;
                        started = false;
                        winningcount += 1;
                    }
                }
                else if (ChildSelection == 2)
                {
                    if (playerselection == 1)
                    {
                        WinningUISlots[playingcount].sprite = Win;
                        winningcount += 1;
                        started = false;
                    }
                    else if (playerselection == 3)
                    {
                        WinningUISlots[playingcount].sprite = Lose;
                        started = false;
                    }
                }
                else if (ChildSelection == 3)
                {
                    if (playerselection == 1)
                    {
                        WinningUISlots[playingcount].sprite = Lose;
                        started = false;
                    }
                    else if (playerselection == 2)
                    {
                        WinningUISlots[playingcount].sprite = Win;
                        winningcount += 1;
                        started = false;
                    }
                }
                playingcount += 1;
            }
        }

        if (playing && UIback == true)
        {
            foreach (GameObject selection in SelectionUI)
            {
                if (selection != SelectionUI[playerselection - 1])
                {
                    selection.SetActive(false);
                }
            }
        }

        if (!playing && UIback == true)
        {
            foreach (GameObject selection in SelectionUI)
            {
                selection.SetActive(true);
                Debug.Log("SetUI");
            }
            UIback = false;
        }

        if (playingcount > 2)
        {
            ShowResult();
        }
    }

    void RandomSelection()
    {
        int ComponentSelection  = TheSelection[Random.Range(0, TheSelection.Count)];
        Debug.Log(ComponentSelection);

        ChildSelection = ComponentSelection;

        UIback = true;
        playing = true;
        playedtime = 0;
    }

    void ShowResult()
    {
        if (winningcount >= 2)
        {
            Debug.Log("YouWIN");
            // giveItem
        }
        else
        {
            Debug.Log("YouLose");
            // Set dialogue to child required candy
        }
    }

    public void Paper()
    {
        playerselection = 2;

        RandomSelection();
    }

    public void Stone()
    {
        playerselection = 3;
 
        RandomSelection();
    }

    public void Scissor()
    {
        playerselection = 1;

        RandomSelection();
    }
}
