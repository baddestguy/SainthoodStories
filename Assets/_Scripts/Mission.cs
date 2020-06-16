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
    public double StartingClock;
    public int StartingFaithPoints;
    public int StartingCharityPoints;
    public int TotalDays;

    public Mission(int faithPoints, int charityPoints, int startingEnergy, double startTime, int days)
    {
        StartingFaithPoints = faithPoints;
        StartingCharityPoints = charityPoints;
        StartingEnergy = new Energy(startingEnergy);
        StartingClock = startTime;
        TotalDays = days;
    }
}
