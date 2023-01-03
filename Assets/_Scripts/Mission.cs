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
    public int TotalDays;
    public int CurrentWeek;

    public Season Season { 
        get 
        {
            var summerArray = new int[] { 1,4,7,10,13,16 };
            var fallArray = new int[] { 2,5,8,11,14,17 };
            var winterArray = new int[] {3,6,9,12,15,18 };

            if(summerArray.Contains(CurrentWeek)) return Season.SUMMER;
            if(fallArray.Contains(CurrentWeek)) return Season.FALL;
            if(winterArray.Contains(CurrentWeek)) return Season.WINTER;

            return Season.SUMMER;
        } 
    }

    public string SeasonLevel
    {
        get {
            switch (Season)
            {
                case Season.SPRING: return "SpringLevel";
                case Season.SUMMER: return "SummerLevel";
                case Season.FALL: return "FallLevel";
                case Season.WINTER: return "WinterLevel";
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

    public Mission(int faithPoints, int faithPointsPool, int charityPoints, int startingEnergy, double startTime, int days, int week)
    {
        StartingFaithPoints = faithPoints;
        StartingCharityPoints = charityPoints;
        StartingEnergy = new Energy(startingEnergy);
        StartingClock = startTime;
        TotalDays = days;
        FaithPointsPool = faithPointsPool;
        CurrentWeek = week;
    }
}
