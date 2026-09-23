using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

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
        foreach (var panels in FindObjectsByType<OpenCanvasButton>(FindObjectsInactive.Include))
        {
            panels.AvaliableNotebookCanva();
            panels.DisablingInGameCanva();
            panels.DisablingTutorialCanva();
        }
        maxpage = 1;
        LoadAllData();
        if (Notes.Count == 0)
        {
            Notes.Add("");
        }

        pagenum = 1;
        maxpage = Mathf.Max(1, Notes.Count);
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

        if (Notes.Count == 0)
        {
            Notes.Add("");
        }

        int currentIndex = Mathf.Clamp(pagenum - 1, 0, Notes.Count - 1);
        SaveNotes(currentIndex);

        pagenum += 1;
        if (pagenum > Notes.Count)
        {
            Notes.Add("");
        }

        maxpage = Mathf.Max(maxpage, Notes.Count);
        int nextIndex = Mathf.Clamp(pagenum - 1, 0, Notes.Count - 1);
        LoadNotes(nextIndex);
        NumPagesShowing.text = "Page:" + pagenum + "/" + maxpage;
    }

    public void PrevPage()
    {
        if (pagenum <= 1)
        {
            return;
        }

        if (Notes.Count == 0)
        {
            Notes.Add("");
        }

        int currentIndex = Mathf.Clamp(pagenum - 1, 0, Notes.Count - 1);
        SaveNotes(currentIndex);

        pagenum -= 1;
        int prevIndex = Mathf.Clamp(pagenum - 1, 0, Notes.Count - 1);
        LoadNotes(prevIndex);
        NumPagesShowing.text = "Page:" + pagenum + "/" + maxpage;
    }

    public void SaveNotes(int num)
    {
        if (num < 0)
        {
            num = 0;
        }

        while (Notes.Count <= num)
        {
            Notes.Add("");
        }

        Notes[num] = noteInput.text;
    }

    public void LoadNotes(int num)
    {
        if (num < 0)
        {
            num = 0;
        }

        while (Notes.Count <= num)
        {
            Notes.Add("");
        }

        noteInput.text = Notes[num];
    }

    public void CloseNote()
    {
        if (pagenum > 0 && pagenum <= Notes.Count)
        {
            Notes[pagenum-1] = noteInput.text;
            SaveAllData();
        }
        Notes.Clear();
        foreach (var panels in FindObjectsByType<OpenCanvasButton>(FindObjectsInactive.Include)) panels.CloseAll();
    }

    public void FlushForSlot()
    {
        if (pagenum < 1 || Notes.Count == 0) return;
        Notes[pagenum - 1] = noteInput.text;
        SaveAllData();
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

        Notes.Clear();
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
