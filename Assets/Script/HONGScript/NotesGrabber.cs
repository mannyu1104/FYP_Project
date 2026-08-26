using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEditor.Search;

[System.Serializable]
public class NotesSaveData
{
    public List<string> NotesAll = new List<string>();
}

public class NotesGrabber : MonoBehaviour
{
    public static NotesGrabber Instance;

    private int pagenum;
    private int maxpage;
    private int savenum;
    private int loadnum;

    private string path;

    [SerializeField] private TMP_Text NumPagesShowing;
    public TMP_InputField noteInput;
    public List<string> Notes = new List<string>();

    private void Awake()
    {
        Instance = this;

        path = Application.persistentDataPath + "/notes.json";
    }

    public void OpenNotes()
    {
        maxpage = 1;
        LoadAllData();
        if (Notes.Count == 0)
        {
            Notes.Add("");
        }
        pagenum = 1;
        LoadNotes(0);
        NumPagesShowing.text = "Page:" + pagenum + "/" + maxpage;
        Debug.Log(Notes.Count);
    }

    public void NextPage()
    {
        if (pagenum >= 20)
        {
            return;
        }
        else
        {
            savenum = pagenum - 1;
            pagenum += 1;
            loadnum = pagenum - 1;
            if (pagenum > Notes.Count)
            {
                Notes.Add("");
                maxpage = Notes.Count;
            }
            SaveNotes(savenum);
            LoadNotes(loadnum);
            NumPagesShowing.text = "Page:" + pagenum + "/" + maxpage;
        }
    }

    public void PrevPage()
    {
        if (pagenum <= 1)
        {
            return;
        }
        else
        {
            savenum = pagenum - 1;
            pagenum -= 1;
            loadnum = pagenum - 1;
            SaveNotes(savenum);
            LoadNotes(loadnum);
            NumPagesShowing.text = "Page:" + pagenum + "/" + maxpage;
        }
    }

    public void SaveNotes(int num)
    {
        Notes[num] = noteInput.text;
    }

    public void LoadNotes(int num)
    {
        noteInput.text = Notes[num];
    }

    public void CloseNote()
    {
        Notes[pagenum-1] = noteInput.text;
        SaveAllData();
        Notes.Clear();
    }

    public void SaveAllData()
    {
        NotesSaveData data = new NotesSaveData();

        foreach (string notesinpage in Notes)
        {
            data.NotesAll.Add(notesinpage);
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);

        Debug.Log("NoteSave");
    }

    public void LoadAllData()
    {
        if (!File.Exists(path))
        {
            Debug.Log("No Save File Found");
            return;
        }

        string json = File.ReadAllText(path);
        NotesSaveData data = JsonUtility.FromJson<NotesSaveData>(json);

        foreach (string notesinpage in data.NotesAll)
        {
            Notes.Add(notesinpage);
        }
        maxpage = Notes.Count;

        Debug.Log("NotesLoad");
    }
}
