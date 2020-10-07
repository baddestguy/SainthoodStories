using System.Collections.Generic;
using UnityEngine;

public class TutorialMapTileGroups : MonoBehaviour
{
    public static TutorialMapTileGroups Instance { get; private set; }

    public List<MapTile> Group1;
    public List<MapTile> Group2;
    public List<MapTile> Group3;
    public List<MapTile> Group4;
    public List<MapTile> Group5;

    public MapTile ChurchMapTile;

    private void Awake()
    {
        Instance = this;
    }
}
