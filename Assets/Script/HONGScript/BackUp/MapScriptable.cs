using UnityEngine;

[CreateAssetMenu(fileName = "Map", menuName = "ScriptableObject/Map")]
public class Map : ScriptableObject
{
    public Sprite ImageLocked;
    public Sprite ImageUnlocked;
    public string PlaceName;

    public int MapID;
    public bool Unlocked;
}
