using System.Linq;

public enum MissionDifficulty
{
    EASY = 0,
    NORMAL,
    HARD
}

public enum Season
{
    SPRING = 0,
    SUMMER,
    FALL,
    WINTER
}

public class Mission
{
    public Energy StartingEnergy;
    public double StartingClock;
    public int StartingFaithPoints;
    public int FaithPointsPool;
    public int StartingCharityPoints;
    public int CharityPointsPool;
    public int TotalDays;
    public int CurrentWeek;

    public Season Season { 
        get 
        {
            var currentMission = MissionManager.Instance.CurrentObjective;
            return currentMission?.Season ?? Season.WINTER;
        } 
    }

    public void OverrideSeason(Season season)
    {
        switch (season)
        {
            case Season.SUMMER: CurrentWeek = 1; break;
            case Season.FALL: CurrentWeek = 2; break;
            case Season.WINTER: CurrentWeek = 3; break;
        }
    }

    public string SeasonLevel
    {
        get {
            switch (Season)
            {
                case Season.SPRING: return "SpringLevel";
                case Season.SUMMER: return "SummerLevel Large";
                case Season.FALL: return "FallLevel Large";
                case Season.WINTER: return "WinterLevel Large";
            }

            return "ERROR Retrieving Season text!";
        }
    }

    public SceneID SeasonSceneId
    {
        get {
            switch (Season)
            {
                case Season.SPRING: return SceneID.Spring;
                case Season.SUMMER: return SceneID.Summer;
                case Season.FALL: return SceneID.Fall;
                case Season.WINTER: return SceneID.Winter;
            }

            return SceneID.Spring;
        }
    }

    public Mission(int faithPoints, int faithPointsPool, int charityPoints, int charityPointsPool, int startingEnergy, double startTime, int days, int week)
    {
        StartingFaithPoints = faithPoints;
        StartingCharityPoints = charityPoints;
        StartingEnergy = new Energy(startingEnergy);
        StartingClock = startTime;
        TotalDays = days;
        FaithPointsPool = faithPointsPool;
        CharityPointsPool = charityPointsPool;
        CurrentWeek = week;
    }
}
