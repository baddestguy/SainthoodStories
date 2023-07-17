using System.Collections.Generic;
using UnityEngine;

public class DataClasses : MonoBehaviour
{

}

public enum SaintID
{
     NONE = 0
    ,ANDREW
    ,ANN
    ,BAKHITA
    ,CANUTE
    ,ELIZABETH
    ,FRANCOIS
    ,FREDERICK
    ,FREI
    ,IRMA
    ,JADWIGA
    ,JOHN
    ,JOSE
    ,JOSEPH
    ,KATERI
    ,KATHARINE
    ,LORENZO
    ,MAGDALENE
    ,MARIAM
    ,MARIE
    ,MONICA
    ,MOSES
    ,PEDRO
    ,ROQUE
    ,SANMARTIN
    ,VICTOR
}   

public enum CustomEventRewardType
{
    NONE = 0,
    COIN,
    FP,
    CP
}

public enum BuildingEventType
{
    NONE = 0,
    DELIVER_ITEM,
    BABY,
    VOLUNTEER
}

public enum BuildingActivityState
{
    NONE = 0,
    VOLUNTEERING,
    TEACHING,
    DELIVERING_BABY
}

public enum ThankYouType
{
    ITEM,
    VOLUNTEER,
    BABY,
    TEACH
}

public enum PlayerStatusEffect
{
    NONE = 0,
    FATIGUED, //Stat Debuff, more energy required for Actions
    SICK, //Damage over time, lose energy every hour
    VULNERABLE, //Setup for more effects
    MIGRAINE, //Lose all Energy in x hours
    FROZEN // Debilitating
}

public enum PlayerFacingDirection
{
    UP = 0,
    DOWN,
    LEFT,
    RIGHT
}

public enum WeatherType
{
    NONE = 0,
    PRESTORM,
    PREHEAT,
    RAIN,
    SNOW,
    HAIL,
    HEATWAVE
}

public enum DayNight
{
    DAY = 0,
    NIGHT
}

public enum TileType
{
    PLAYER = 0,
    ROAD,
    BARRIER,
    WATER,
    SNOW,
    ICE,
    TREE,
    SHELTER,
    CHURCH,
    MARKET,
    HOSPITAL,
    SCHOOL,
    CLOTHESBANK,
    ORPHANAGE,
    KITCHEN,
    BANDIT,
    CHILD
}

public enum ItemType
{
    NONE = 0,
    GROCERIES,
    CLOTHES,
    TOYS,
    STATIONERY,
    MEDS,
    MEAL
}

public enum Provision
{
    UMBRELLA,
    WINTER_CLOAK,
    SHADES,
 //   EXTRA_INVENTORY, //HIDE TEMPORARILY!
    ENERGY_DRINK,
    ROSARY,
    COOKING_UTENSILS,
    DISCOUNT_CARD,
    ALLOWANCE,
    REDUCE_SLEEP_TIME,
    CONSTRUCTION_TOOLS
    ,STURDY_BUILDING_MATERIALS
    ,CONSTRUCTION_TENTS
    ,CHAPEL_BLUEPRINT
    ,BUILDING_BLUEPRINT,
    HOSPITAL_RELATIONSHIP_BUILDER,
    SCHOOL_RELATIONSHIP_BUILDER,
    ORPHANAGE_RELATIONSHIP_BUILDER,
    KITCHEN_RELATIONSHIP_BUILDER,
    SHELTER_RELATIONSHIP_BUILDER
    ,AUTO_DELIVER
    ,SOFT_MATTRESS
    ,FASTING
    ,KOBOKO
    ,INCENSE
    ,SECURITY_GUARDS
    ,MAX_COUNT
}

public enum BuildingState
{
    RUBBLE = 0,
    NORMAL,
    HAZARDOUS
}

public enum CustomEventType
{
    NONE = 0,
    MARKET_HOURS,
    RAIN,
    HEATWAVE,
    BLIZZARD,
    BABY_FEVER,
    MARKET_DISCOUNT,
    MARKET_MARKUP,
    WEEKDAY_MASS,
    SUNDAY_MASS,
    SCHOOL_CLOSED,
    SICK,
    HIGH_SPIRIT,
    LOW_SPIRIT,
    HIGH_MORALE,
    LOW_MORALE,
    ORPHANAGE_BONUS,
    HOSPITAL_BONUS,
    VANDALISM,
    VANDALISM_STOPPED,
    SPIRITUAL_RETREAT,
    PRAYER_REQUEST,
    TOWN_HELP,
    DONATION,
    ICON,
    SPIRITUALCRISIS,
    RIOTS,
    THANKYOU_ITEM_HOSPITAL,
    THANKYOU_ITEM_SCHOOL,
    THANKYOU_ITEM_ORPHANAGE,
    THANKYOU_ITEM_FOOD,
    THANKYOU_ITEM_CLOTHES,
    THANKYOU_MONEY,
    THANKYOU_BABY,
    THANKYOU_TEACH,
    THANKYOU_VOLUNTEER_HOSPITAL,
    THANKYOU_VOLUNTEER_ORPHANAGE,
    Tutorial_61,
    BIBLE_STUDY
    ,QUIET_TIME
    ,BLESSEDSACRAMENT_ROSARY
    ,SOCIALIZE_OLDFOLK
    ,LAUNDRY
    ,FLOORS
    ,PLAYTIME
    ,ARTS_CRAFT
    ,SPORTS
    ,AFTERSCHOOL_LESSON
    ,SUB_TEACHER
    ,DETENTION
    ,SUPERVISE_RECESS
    ,LIBRARY_ORGANIZATION
    ,WEDDING
    ,SHELTER_SURPLUS
    ,ORPHANAGE_PICNIC
    ,DISHWASHING
    ,SHELTER_SERVE
    ,SHELTER_BUILDEXTRA
    ,STOCK_SHELVES
    ,REGISTRATION
    ,ORPHANAGE_PRECOMPLETE
    ,ORPHANAGE_COMPLETE
    ,SCHOOL_PRECOMPLETE
    ,SCHOOL_COMPLETE
    ,KITCHEN_PRECOMPLETE
    ,KITCHEN_COMPLETE
    ,SHELTER_PRECOMPLETE
    ,SHELTER_COMPLETE
    ,CLOTHES_PRECOMPLETE
    ,CLOTHES_COMPLETE
    ,BABY_FAILED
    ,ENERGY_DEPLETED
    ,TRYHARDER_FAITH
    ,TRYHARDER_CHARITY
    ,SAVE_HOSPITAL
    ,SAVE_SCHOOL
    ,SAVE_ORPHANAGE
    ,SAVE_KITCHEN
    ,SAVE_SHELTER
    ,ENDGAME
    ,AUTO_DELIVER_BEGIN
    ,AUTO_DELIVER_COMPLETE

}

public enum EventPopupType
{
    OK = 0,
    YESNO
}

public enum EventGroup
{
    DAILY = 0,
    IMMEDIATE,
    ENDWEEK,
    THANKYOU,
    STORY,
    CHURCH,
    HOSPITAL,
    ORPHANAGE,
    SCHOOL,
    KITCHEN,
    SHELTER,
    CLOTHES
    ,SAVE_HOSPITAL
    ,SAVE_SCHOOL
    ,SAVE_ORPHANAGE
    ,SAVE_KITCHEN
    ,SAVE_SHELTER
}

public enum Language
{
    ENGLISH,
    FRENCH,
    BRPT,
    LATAMSPANISH,
    ITALIAN,
    GERMAN,
    FILIPINO
}

public enum QualityLevel
{
    QUALITY_SETTING_VERY_LOW = 0,
    QUALITY_SETTING_LOW,
    QUALITY_SETTING_MEDIUM,
    QUALITY_SETTING_HIGH,
    QUALITY_SETTING_VERY_HIGH,
    QUALITY_SETTING_ULTRA
}

public enum TooltipStatId
{
    NONE = 0,
    MOVE,
    BUILD,
    CONSTRUCT,
    PRAY,
    SLEEP,
    VOLUNTEER,
    BABY,
    TIME
}

[System.Serializable]
public class HouseSaveData
{
    public string HouseName;
    public int RelationshipPoints;
    public int RelationshipBonus;
    public BuildingState BuildingState;
    public int FPBonus;
    public int SturdyMaterials;
    public bool DeadlineSet;
    public int DeadlineCounter;
    public int RequiredItems;
    public double DeadlineTime;
    public int DeadlineDay;
    public bool DeliveryTimeSet;
    public double DeliveryTime;
    public int DeliveryDay;
    public int EnvironmentalHazardDestructionCountdown;
    public int HazardCounter;
}

public class TooltipStats
{
    public TooltipStatId Id;
    public double Ticks;
    public int FP;
    public int CP;
    public int Energy;
}

[System.Serializable]
public class WeatherData
{
    public int Week;
    public int Day;
    public double Time;
    public double StartTime;
    public double Duration;
}

[System.Serializable]
public class BuildingMissionData
{
    public int Week;
    public int Day;
    public double Time;
    public int RequiredItems;
    public double DeadlineHours;
    public BuildingEventType Event;
    public string InteractableHouse;
}

[System.Serializable]
public class ShopItemData
{
    public ItemType Id;
    public double Price;
    public string NameKey;
    public string DescriptionKey;
}

[System.Serializable]
public class ProvisionData
{
    public Provision Id;
    public int Level;
    public int Value;
    public string NameKey;
    public string DescriptionKey;
    public string Tooltips;
}

[System.Serializable]
public class StatusEffectData
{
    public PlayerStatusEffect Id;
    public string NameKey;
    public string DescriptionKey;
    public string Tooltips;
}

[System.Serializable]
public class ConstantsData
{
    public string Id;
    public int IntValue;
    public float FloatValue;
}

[System.Serializable]
public class CustomEventData
{
    public CustomEventType Id;
    public EventPopupType EventPopupType;
    public EventGroup EventGroup;
    public int Weight;
    public float Cost;
    public float Gain;
    public CustomEventRewardType RewardType;
    public float RejectionCost;
    public bool IsOrderedSequence;
    public string TriggerWeekDay;
    public string ImagePath;
    public string LocalizationKey;
}

[System.Serializable]
public class StoryEventData
{
    public string Id;
    public EventGroup EventGroup = EventGroup.STORY;
    public int Week;
    public int Day;
    public double Time;
    public string ImagePath;
    public bool IsOrderedSequence;
    public int OrderBy;
}

[System.Serializable]
public class LocalizationData
{
    public string Key;
    public string English;
    public string French;
    public string BrPt;
    public string Filipino;
    public string LatAmSpanish;
    public string Italian;
    public string German;
}

[System.Serializable]
public class ConstructionAvailabilityData
{
    public string Id;
    public int Week;
    public int Day;
    public double Time;
}

[System.Serializable]
public class SaintData
{
    public SaintID Id;
    public string Name;
    public string Birthday;
    public string Death;
    public string FeastDay;
    public string PatronKey;
    public string BioKey;
    public string IconPath;
}

public class TileData
{
    public int Id { get; }
    public int TileSpriteId { get; }
    public TileType TileType { get; }

    public TileData(int id, int tileId, TileType tileType)
    {
        Id = id;
        TileSpriteId = tileId;
        TileType = tileType;
    }
}

public class MapData
{
    public int MapId { get; }
    public List<TileData> Tiles { get; }
    public int Rows { get; }
    public int Columns { get; }

    public MapData(int mapId, int rows, int columns, List<TileData> tiles)
    {
        MapId = mapId;
        Rows = rows;
        Columns = columns;
        Tiles = tiles;
    }
}


[System.Serializable]
public class SaveObject
{
    public int FP;
    public int FPPool;
    public int CP;
    public int Energy;
    public int Week;
    public int Day;
    public double Time;
    public int TutorialSteps;
    public double Money;
    public int RunAttempts;
    public HouseSaveData[] Houses;
    public SaintID[] Saints;
    public ItemType[] InventoryItems;
    public ProvisionData[] Provisions;
    public ProvisionData[] GeneratedProvisions;
    public CustomEventType DailyEvent;
    public int[] Maptiles;
    public string CurrentHouse;
    public PlayerStatusEffect[] StatusEffects;
    public CustomEventType HouseTriggeredEvent;
    public CustomEventData CurrentDailyEvent;
    public double WeatherStartTime;
    public int WeatherStartDay;
    public double WeatherEndTime;
    public int WeatherEndDay;
    public bool WeatherActivated;
}

[System.Serializable]
public class SaveSettingsData
{
    public bool fullscreen;
    public QualityLevel qualityLevel;
    public int resolutionWidth;
    public int resolutionHeight;
    public float brightnessPercent;
    public float gammaPercent;

    public bool sfxEnabled;
    public bool musicEnabled;
    public bool ambianceEnabled;
    public float globalVolume;
    public float sfxVolume;
    public float musicVolume;
    public float ambianceVolume;
    public bool tutorialEnabled;

    public Language language;
}

[System.Serializable]
public class KSBackers
{
    public string Name;
}

public enum ProvisionsPopupPhase
{
    ADD_UPGRADE,
    REPLACE
}

public enum ProvisionUIItemType
{
    NEW,
    UPGRADE
}