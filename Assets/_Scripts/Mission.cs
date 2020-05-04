using UnityEngine;

public class Mission
{
    //public MissionType MissionType;
    public int TargetNumber;
    public Energy StartingEnergy;
    public int StartingClock;
    public int StartingFaithPoints;
    public int StartingTownPoints;

    public Mission(int faithPoints, int townPoints, int startingEnergy, int startTime)
    {
        StartingFaithPoints = faithPoints;
        StartingTownPoints = townPoints;
        StartingEnergy = new Energy(startingEnergy);
        StartingClock = startTime;
    }
}
