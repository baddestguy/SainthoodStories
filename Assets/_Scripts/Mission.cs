using UnityEngine;

public class Mission
{
    //public MissionType MissionType;
    public int TargetNumber;
    public Energy StartingEnergy;
    public int StartingTimer;
    public MapTile StartingTile;

    public Mission(int targetNumber, int startingEnergy, int startingTimer, MapTile startingTile)
    {
        TargetNumber = targetNumber;
        StartingEnergy = new Energy(startingEnergy);
        StartingTimer = startingTimer;
        StartingTile = startingTile;
    }
}
