public enum MissionDifficulty
{
    EASY = 0,
    NORMAL,
    HARD
}

public class Mission
{
    public Energy StartingEnergy;
    public double StartingClock;
    public int StartingFaithPoints;
    public int StartingCharityPoints;
    public int TotalDays;
    public int CurrentWeek;

    public Mission(int faithPoints, int charityPoints, int startingEnergy, double startTime, int days, int week)
    {
        StartingFaithPoints = faithPoints;
        StartingCharityPoints = charityPoints;
        StartingEnergy = new Energy(startingEnergy);
        StartingClock = startTime;
        TotalDays = days;
        CurrentWeek = week;
    }
}
