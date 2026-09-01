using UnityEngine;

[CreateAssetMenu(fileName = "MapItems", menuName = "ScriptableObject/MapItems")]
public class MapItem : ScriptableObject
{
    public string NameShow;
    public string ObjType;

    public bool Get;
    public bool Used;
    public bool Show;
    public int ItemID;
}
