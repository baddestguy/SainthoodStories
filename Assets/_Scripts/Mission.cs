using UnityEngine;

public class Mission
{
    //public MissionType MissionType;
    public int TargetNumber;
    public int StartingEnergy;
    public int StartingTimer;
    public MapTile StartingTile;

    public Mission(int targetNumber, int startingEnergy, int startingTimer, MapTile startingTile)
    {
        TargetNumber = targetNumber;
        StartingEnergy = startingEnergy;
        StartingTimer = startingTimer;
        StartingTile = startingTile;
    }
}
