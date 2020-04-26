using UnityEngine;

public class Mission
{
    //public MissionType MissionType;
    public int TargetNumber;
    public Energy StartingEnergy;
    public int StartingClock;

    public Mission(int targetNumber, int startingEnergy, int startTime)
    {
        TargetNumber = targetNumber;
        StartingEnergy = new Energy(startingEnergy);
        StartingClock = startTime;
    }
}
