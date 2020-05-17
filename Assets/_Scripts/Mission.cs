public enum MissionDifficulty
{
    EASY = 0,
    NORMAL,
    HARD
}

public class Mission
{
    public int TargetNumber;
    public Energy StartingEnergy;
    public int StartingClock;
    public int StartingFaithPoints;
    public int StartingCharityPoints;
    public int TotalDays;

    public Mission(int faithPoints, int charityPoints, int startingEnergy, int startTime, int days)
    {
        StartingFaithPoints = faithPoints;
        StartingCharityPoints = charityPoints;
        StartingEnergy = new Energy(startingEnergy);
        StartingClock = startTime;
        TotalDays = days;
    }
}
