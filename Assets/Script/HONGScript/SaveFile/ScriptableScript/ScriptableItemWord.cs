using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObject/Item")]
public class Item : ScriptableObject
{
    public TutorialClueData TutorialClueDataTest;
    
    public string ItemShowName;
    public string Description;
    public string TypeofItem;

    public bool Get;
    public bool Used;
    //public bool Show;
    public int ItemID;

}
