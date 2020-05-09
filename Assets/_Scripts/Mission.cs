using UnityEngine;

public class Mission
{
    //public MissionType MissionType;
    public int TargetNumber;
    public Energy StartingEnergy;
    public int StartingClock;
    public int StartingFaithPoints;
    public int StartingCharityPoints;

    public Mission(int faithPoints, int charityPoints, int startingEnergy, int startTime)
    {
        StartingFaithPoints = faithPoints;
        StartingCharityPoints = charityPoints;
        StartingEnergy = new Energy(startingEnergy);
        StartingClock = startTime;
    }
}
