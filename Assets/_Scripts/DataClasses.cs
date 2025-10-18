using System;
using System.Collections.Generic;
using CsvHelper.Configuration.Attributes;
using UnityEngine;

public class DataClasses : MonoBehaviour
{

}

public interface ICondition
{
    bool IsTrue();
}


public enum MinigameType
{
    CONSTRUCT,
    CPR,
    BIRTH
}

public enum SaintID
{
    NONE = 0
    , ANDREW
    , ANN
    , BAKHITA
    , CANUTE
    , ELIZABETH
    , FRANCOIS
    , FREDERICK
    , FREI
    , IRMA
    , JADWIGA
    , JOHN
    , JOSE
    , JOSEPH
    , KATERI
    , KATHARINE
    , LORENZO
    , MAGDALENE
    , MARIAM
    , MARIE
    , MONICA
    , MOSES
    , PEDRO
    , ROQUE
    , SANMARTIN
    , VICTOR
}

public enum WeatherId
{
    CLEAR = 6,
    FOG = 10,
    SANDSTORM = 22,
    RAIN = 24,
    BLIZZARD = 17
}

public enum SacredItemBehaviour
{
    HOVER = 0,
    BOUNCE,
    PATROL,
    RUNAWAY,
    DECOY,
    MIRROR_MOVEMENT,
    TELEPORT,
    ZIP,
    BURST,
    SPIRAL,
    CONCENTRATION,
    WEATHER_CHANGING,
    CHASE,
    WANDER,
    HARMLESS
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
    VOLUNTEER,
    CONSTRUCT,
    REPAIR,
    RETURN,
    MASS,
    SPECIAL_EVENT,
    DELIVER_MEAL,
    COOK,
    PRAY,
    URGENT,
    DELIVER_ITEM_URGENT,
    DELIVER_MEAL_URGENT,
    VOLUNTEER_URGENT,
    REPAIR_URGENT,
    CONSTRUCT_URGENT,
    COOK_URGENT,
    PRAY_URGENT,
    SPECIAL_EVENT_URGENT,
    RETURN_URGENT
}

public enum BuildingActivityState
{
    NONE = 0,
    VOLUNTEERING,
    DELIVERING_BABY
}

public enum ThankYouType
{
    ITEM,
    VOLUNTEER,
    BABY,
    TEACH,
    UPGRADE,
    IMMEDIATE_ASSISTANCE
}

public enum PlayerStatusEffect
{
    NONE = 0,
    VULNERABLE, //Setup for more effects
    FATIGUED, //Stat Debuff, more energy required for Actions
    SICK, //Damage over time, lose energy every hour
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
    CHILD,
    BUILDING
}

public enum ItemType
{
    NONE = 0,
    GROCERIES,
    KITCHEN_INGREDIENTS,
    CLOTHES,
    TOYS,
    STATIONERY,
    MEDS,
    MEAL,
    ENERGY_BOOST,
    DRUGS
}

public enum Provision
{
    UMBRELLA,
    WINTER_CLOAK,
    ENERGY_DRINK,
    ROSARY,
    COOKING_UTENSILS,
    DISCOUNT_CARD,
    ALLOWANCE,
    CONSTRUCTION_TOOLS
    , CONSTRUCTION_TENTS
    , CHAPEL_BLUEPRINT
    , BUILDING_BLUEPRINT,
    HOSPITAL_RELATIONSHIP_BUILDER,
    SCHOOL_RELATIONSHIP_BUILDER,
    ORPHANAGE_RELATIONSHIP_BUILDER,
    SHELTER_RELATIONSHIP_BUILDER
    , SOFT_MATTRESS
    , REDUCE_SLEEP_TIME
    , FASTING
    , KOBOKO
    , INCENSE
    , SECURITY_GUARDS
    , STURDY_BUILDING_MATERIALS
    , MAX_COUNT      //Anything below Max_Count will not be obtained during gameplay. Keep the broken provisions here!
    , SHADES
    , KITCHEN_RELATIONSHIP_BUILDER
    , EXTRA_INVENTORY
    , AUTO_DELIVER
}

public enum HouseType
{
    InteractableChurch = 0,
    InteractableHospital,
    InteractableKitchen,
    InteractableOrphanage,
    InteractableShelter,
    InteractableSchool,
    InteractableClothesBank
}

public enum BuildingState
{
    RUBBLE = 0,
    NORMAL,
    HAZARDOUS
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
    , SAVE_HOSPITAL
    , SAVE_SCHOOL
    , SAVE_ORPHANAGE
    , SAVE_KITCHEN
    , SAVE_SHELTER
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
    TIME,
    PURCHASE
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
    public bool DeadlineTriggeredForTheDay;
    public bool DeliveryTimeSet;
    public double DeliveryTime;
    public int DeliveryDay;
    public int EnvironmentalHazardDestructionCountdown;
    public int HazardCounter;
    public bool HasBeenDestroyed;
    public List<CustomEventType> MyStoryEvents;
    public int UpgradeLevel;
    public int CurrentMissionId;
    public int VolunteerProgress;
    public bool AllObjectivesComplete;
}

public class TooltipStats
{
    public TooltipStatId Id {get; set; }
    public double Ticks {get; set; }
    public int FP {get; set; }
    public int CP {get; set; }
    public int Energy {get; set; }
    [Optional]
    public int Spirits {get; set; }
    [Optional]
    public int Coin {get; set; }
    [Optional]
    public int RP {get; set; }
}

[System.Serializable]
public class WeatherData
{
    public int Week {get; set; }
    public int Day {get; set; }
    public double Time {get; set; }
    public double StartTime {get; set; }
    public double Duration {get; set; }
}

[System.Serializable]
public class CollectibleData : IComparable<CollectibleData>
{
    public string Id {get; set; }
    public string Name {get; set; }
    public string Description {get; set; }
    public int SortOrder {get; set; }

    public int CompareTo(CollectibleData compareCollectible)
    {
        if (compareCollectible == null)
            return 1;

        else
            return SortOrder.CompareTo(compareCollectible.SortOrder);
    }
}

[System.Serializable]
public class CollectibleObjectivesData
{
    public int Id {get; set; }
    public int Amount {get; set; }
    public CustomEventType OnComplete {get; set; }
}

[System.Serializable]
public class HouseObjectivesData
{
    public string House {get; set; }
    public int MissionId {get; set; }
    public BuildingEventType Event {get; set; }
    public int RequiredAmount {get; set; }
    public CustomEventType CustomEventId {get; set; }
    public CustomEventType ThankYouEvent {get; set; }
    public CustomEventType SpecialEventId {get; set; }
    public int Reward {get; set; }
    public string MissionDescription {get; set; }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        HouseObjectivesData other = (HouseObjectivesData)obj;
        return House == other.House && MissionId == other.MissionId && CustomEventId == other.CustomEventId
            && Event == other.Event && RequiredAmount == other.RequiredAmount && Reward == other.Reward;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + House.GetHashCode();
            hash = hash * 23 + MissionId.GetHashCode();
            hash = hash * 23 + (Event.GetHashCode());
            hash = hash * 23 + (RequiredAmount.GetHashCode());
            hash = hash * 23 + (CustomEventId.GetHashCode());
            hash = hash * 23 + Reward.GetHashCode();
            return hash;
        }
    }
}


[System.Serializable]
public class ObjectivesData
{
    public int Id {get; set; }
    public CustomEventType CustomEventId {get; set; }
    public CustomEventType DailyEvent {get; set; }
    public int WeatherId {get; set; }
    public Season Season {get; set; }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        ObjectivesData other = (ObjectivesData)obj;
        return Id == other.Id && CustomEventId == other.CustomEventId
                              && WeatherId == other.WeatherId && Season == other.Season;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + Id.GetHashCode();
            hash = hash * 23 + (CustomEventId.GetHashCode());
            hash = hash * 23 + WeatherId.GetHashCode();
            return hash;
        }
    }

}

[System.Serializable]
public class BuildingMissionData
{
    public int Week {get; set; }
    public int Day {get; set; }
    public double Time {get; set; }
    public int RequiredItems {get; set; }
    public double DeadlineHours {get; set; }
    public BuildingEventType Event {get; set; }
    public string InteractableHouse {get; set; }
}


[System.Serializable]
public class ShopItemData
{
    public ItemType Id {get; set; }
    public double Price {get; set; }
    public string NameKey {get; set; }
    public string DescriptionKey {get; set; }

    public static string HouseNameForItemType(ItemType item)
    {
        switch (item)
        {
            case ItemType.MEDS:
                return "InteractableHospital";
            case ItemType.TOYS:
                return "InteractableOrphanage";
            case ItemType.MEAL:
            case ItemType.KITCHEN_INGREDIENTS:
                return "InteractableKitchen";
            case ItemType.GROCERIES:
                return "InteractableShelter";
            case ItemType.STATIONERY:
                return "InteractableSchool";
        }
        return "";
    }
}

[System.Serializable]
public class ProvisionData
{
    public Provision Id {get; set; }
    public int Level {get; set; }
    public int Value {get; set; }
    public int Time {get; set; }
    public int Ticks {get; set; }
    public int Energy {get; set; }
    public int FP {get; set; }
    public int CP {get; set; }
    public int Coin {get; set; }
    public int Spirits {get; set; }
    public string NameKey {get; set; }
    public string DescriptionKey {get; set; }
    public string Tooltips {get; set; }
}

[System.Serializable]
public class StatusEffectData
{
    public PlayerStatusEffect Id {get; set; }
    public string NameKey {get; set; }
    public string DescriptionKey {get; set; }
    public string Tooltips {get; set; }
}

[System.Serializable]
public class ConstantsData
{
    public string Id {get; set; }
    public int IntValue {get; set; }
    public float FloatValue {get; set; }
}

[System.Serializable]
public class CustomEventData
{
    public CustomEventType Id { get; set; }
    public EventPopupType EventPopupType { get; set; }
    public EventGroup EventGroup { get; set; }
    public int Weight { get; set; }
    public float Cost { get; set; }
    public int EnergyCost { get; set; }
    public float Gain { get; set; }
    public CustomEventRewardType RewardType { get; set; }
    public float RejectionCost { get; set; }
    public bool IsOrderedSequence { get; set; }
    public string TriggerWeekDay { get; set; }
    public string ImagePath { get; set; }
    public string LocalizationKey { get; set; }
    public string LinkTo { get; set; }
}

[System.Serializable]
public class StoryEventData
{
    public string Id {get; set; }
    [Optional]
    public EventGroup EventGroup { get; set; } = EventGroup.STORY;
    public int Week {get; set; }
    public int Day {get; set; }
    public double Time {get; set; }
    public string ImagePath {get; set; }
    public bool IsOrderedSequence {get; set; }
    public int OrderBy {get; set; }
}

[System.Serializable]
public class LocalizationData
{
    public string Key {get; set; }
    public string English {get; set; }
    public string French {get; set; }
    public string BrPt {get; set; }
    public string Filipino {get; set; }
    public string LatAmSpanish {get; set; }
    public string Italian {get; set; }
    public string German {get; set; }
}


[System.Serializable]
public class ConstructionAvailabilityData
{
    public string Id {get; set; }
    public int Week {get; set; }
    public int Day {get; set; }
    public double Time {get; set; }
}


[System.Serializable]
public class SaintData
{
    public SaintID Id {get; set; }
    public string Name {get; set; }
    public string Birthday {get; set; }
    public string Death {get; set; }
    public string FeastDay {get; set; }
    public string PatronKey {get; set; }
    public string BioKey {get; set; }
    public string IconPath {get; set; }
}


[System.Serializable]
public class MinigameData
{
    public MinigameType Id {get; set; }
    public string IconPath {get; set; }
    public int Sequences {get; set; }
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
    public int CPPool;
    public int Energy;
    public int Week;
    public int Day;
    public double Time;
    public int TutorialSteps;
    public double Money;
    public double TemporaryMoneyToDonate;
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
    public int CurrentMissionId;
    public string[] Collectibles;
    public int WanderingSpirits;
    public int CurrentCollectibleMissionId;
    public int CurrentCollectibleCounter;
    public string[] WorldCollectibles;
    public CustomEventType[] MissionEvents;
    public bool HasChosenProvision;
    public int FaithPointsPermanentlyLost;
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
    public bool DEMO_MODE;
    public bool ShowGrid;
    public bool CustomEventsToggle;

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

public class Constants
{
    public const string PRAYER_MODE_PRODUCT_ID = "prayer_mode";
    public const string FULL_GAME_PRODUCT_ID = "full_game";
    public const double DayTimeStartTime = 10;
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
    WORST_ENDING,
    THANKYOU_ITEM_HOSPITAL,
    THANKYOU_ITEM_SCHOOL,
    THANKYOU_ITEM_ORPHANAGE,
    THANKYOU_ITEM_FOOD,
    THANKYOU_ITEM_CLOTHES,
    THANKYOU_UPGRADE_HOSPITAL,
    THANKYOU_UPGRADE_SCHOOL,
    THANKYOU_UPGRADE_ORPHANAGE,
    THANKYOU_UPGRADE_SHELTER,
    THANKYOU_UPGRADE_CLOTHES,
    THANKYOU_UPGRADE_CHURCH,
    THANKYOU_UPGRADE_KITCHEN,
    THANKYOU_MONEY,
    THANKYOU_BABY,
    THANKYOU_TEACH,
    THANKYOU_VOLUNTEER_HOSPITAL,
    THANKYOU_VOLUNTEER_ORPHANAGE,
    Tutorial_61,
    BIBLE_STUDY
    , QUIET_TIME
    , BLESSEDSACRAMENT_ROSARY
    , SOCIALIZE_OLDFOLK
    , LAUNDRY
    , FLOORS
    , PLAYTIME
    , ARTS_CRAFT
    , SPORTS
    , AFTERSCHOOL_LESSON
    , SUB_TEACHER
    , DETENTION
    , SUPERVISE_RECESS
    , LIBRARY_ORGANIZATION
    , WEDDING
    , SHELTER_SURPLUS
    , ORPHANAGE_PICNIC
    , DISHWASHING
    , SHELTER_SERVE
    , SHELTER_BUILDEXTRA
    , STOCK_SHELVES
    , REGISTRATION
    , HOSPITAL_ARRIVAL
    , HOSPITAL_COMPLETE
    , ORPHANAGE_PRECOMPLETE
    , ORPHANAGE_COMPLETE
    , SCHOOL_PRECOMPLETE
    , SCHOOL_COMPLETE
    , KITCHEN_PRECOMPLETE
    , KITCHEN_COMPLETE
    , SHELTER_PRECOMPLETE
    , SHELTER_COMPLETE
    , CLOTHES_PRECOMPLETE
    , CLOTHES_COMPLETE
    , BABY_FAILED
    , ENERGY_DEPLETED
    , TRYHARDER_FAITH
    , TRYHARDER_CHARITY
    , SAVE_HOSPITAL
    , SAVE_SCHOOL
    , SAVE_ORPHANAGE
    , SAVE_KITCHEN
    , SAVE_SHELTER
    , ENDGAME_DEMO
    , ENDGAME_MOBILE
    , ENDGAME_MOBILE_THANKS
    , ENDGAME
    , ENDGAME_BEST
    , ENDGAME_NORMAL
    , ENDGAME_HOSPITAL
    , ENDGAME_SHELTER
    , ENDGAME_SCHOOL
    , ENDGAME_CHURCH
    , ENDGAME_ORPHANAGE
    , ENDGAME_KITCHEN
    , AUTO_DELIVER_BEGIN
    , AUTO_DELIVER_COMPLETE
    , HOSPITAL_STORY_1
    , HOSPITAL_STORY_2
    , HOSPITAL_STORY_3
    , ORPHANAGE_STORY_1
    , ORPHANAGE_STORY_2
    , ORPHANAGE_STORY_3
    , SCHOOL_STORY_1
    , SCHOOL_STORY_2
    , SCHOOL_STORY_3
    , SHELTER_STORY_1
    , SHELTER_STORY_2
    , SHELTER_STORY_3
    , KITCHEN_STORY_1
    , KITCHEN_STORY_2
    , KITCHEN_STORY_3
    , CONVENT_STORY_1
    , CONVENT_STORY_2
    , CONVENT_STORY_3
    , COLLECTIBLE_MISSION_1
    , COLLECTIBLE_MISSION_2
    , COLLECTIBLE_MISSION_3
    , COLLECTIBLE_MISSION_4
    , COLLECTIBLE_MISSION_5
    , COLLECTIBLE_MISSION_6
    , COLLECTIBLE_MISSION_7
    , COLLECTIBLE_MISSION_8
    , COLLECTIBLE_MISSION_9
    , COLLECTIBLE_MISSION_10
    , COLLECTIBLE_MISSION_11
    , COLLECTIBLE_MISSION_12
    , INTRO
    , MISSION_1
    , MISSION_2
    , MISSION_3
    , MISSION_4
    , MISSION_5
    , MISSION_6
    , MISSION_7
    , MISSION_8
    , MISSION_9
    , MISSION_10
    , MISSION_11
    , MISSION_12
    , MISSION_13
    , MISSION_14
    , MISSION_15
    , MISSION_16
    , MISSION_17
    , MISSION_18
    , MISSION_19
    , MISSION_20
    , MISSION_21
    , MISSION_22
    , MISSION_23
    , MISSION_24
    , MISSION_25
    , MISSION_26
    , MISSION_27
    , MISSION_28
    , MISSION_29
    , MISSION_30
    , MISSION_31
    , MISSION_32
    , MISSION_33
    , MISSION_34
    , MISSION_35
    , MISSION_36
    , MISSION_37
    , MISSION_38
    , MISSION_39
    , MISSION_40
    , HOSPITAL_MISSION_1
    , HOSPITAL_MISSION_2
    , HOSPITAL_MISSION_3
    , HOSPITAL_MISSION_4
    , HOSPITAL_MISSION_5
    , HOSPITAL_MISSION_6
    , HOSPITAL_MISSION_7
    , HOSPITAL_MISSION_8
    , HOSPITAL_MISSION_9
    , HOSPITAL_MISSION_10
    , HOSPITAL_MISSION_11
    , HOSPITAL_MISSION_12
    , HOSPITAL_MISSION_13
    , HOSPITAL_MISSION_14
    , HOSPITAL_MISSION_15
    , ORPHANAGE_MISSION_1
    , ORPHANAGE_MISSION_2
    , ORPHANAGE_MISSION_3
    , ORPHANAGE_MISSION_4
    , ORPHANAGE_MISSION_5
    , ORPHANAGE_MISSION_6
    , ORPHANAGE_MISSION_7
    , ORPHANAGE_MISSION_8
    , ORPHANAGE_MISSION_9
    , ORPHANAGE_MISSION_10
    , ORPHANAGE_MISSION_11
    , ORPHANAGE_MISSION_12
    , ORPHANAGE_MISSION_13
    , ORPHANAGE_MISSION_14
    , ORPHANAGE_MISSION_15
    , SCHOOL_MISSION_1
    , SCHOOL_MISSION_2
    , SCHOOL_MISSION_3
    , SCHOOL_MISSION_4
    , SCHOOL_MISSION_5
    , SCHOOL_MISSION_6
    , SCHOOL_MISSION_7
    , SCHOOL_MISSION_8
    , SCHOOL_MISSION_9
    , SCHOOL_MISSION_10
    , SCHOOL_MISSION_11
    , SCHOOL_MISSION_12
    , SCHOOL_MISSION_13
    , SCHOOL_MISSION_14
    , SCHOOL_MISSION_15
    , SHELTER_MISSION_1
    , SHELTER_MISSION_2
    , SHELTER_MISSION_3
    , SHELTER_MISSION_4
    , SHELTER_MISSION_5
    , SHELTER_MISSION_6
    , SHELTER_MISSION_7
    , SHELTER_MISSION_8
    , SHELTER_MISSION_9
    , SHELTER_MISSION_10
    , SHELTER_MISSION_11
    , SHELTER_MISSION_12
    , SHELTER_MISSION_13
    , SHELTER_MISSION_14
    , SHELTER_MISSION_15
    , KITCHEN_MISSION_1
    , KITCHEN_MISSION_2
    , KITCHEN_MISSION_3
    , KITCHEN_MISSION_4
    , KITCHEN_MISSION_5
    , KITCHEN_MISSION_6
    , KITCHEN_MISSION_7
    , KITCHEN_MISSION_8
    , KITCHEN_MISSION_9
    , KITCHEN_MISSION_10
    , KITCHEN_MISSION_11
    , KITCHEN_MISSION_12
    , KITCHEN_MISSION_13
    , KITCHEN_MISSION_14
    , KITCHEN_MISSION_15
    , CHURCH_MISSION_1
    , CHURCH_MISSION_2
    , CHURCH_MISSION_3
    , CHURCH_MISSION_4
    , CHURCH_MISSION_5
    , CHURCH_MISSION_6
    , CHURCH_MISSION_7
    , CHURCH_MISSION_8
    , CHURCH_MISSION_9
    , CHURCH_MISSION_10
    , CHURCH_MISSION_11
    , CHURCH_MISSION_12
    , CHURCH_MISSION_13
    , CHURCH_MISSION_14
    , CHURCH_MISSION_15
    , THANKYOU_HOSPITAL_1
    , THANKYOU_HOSPITAL_2
    , THANKYOU_HOSPITAL_3
    , THANKYOU_HOSPITAL_4
    , THANKYOU_HOSPITAL_5
    , THANKYOU_HOSPITAL_6
    , THANKYOU_HOSPITAL_7
    , THANKYOU_HOSPITAL_8
    , THANKYOU_HOSPITAL_9
    , THANKYOU_HOSPITAL_10
    , THANKYOU_HOSPITAL_11
    , THANKYOU_HOSPITAL_12
    , THANKYOU_HOSPITAL_13
    , THANKYOU_HOSPITAL_14
    , THANKYOU_ORPHANAGE_1
    , THANKYOU_ORPHANAGE_2
    , THANKYOU_ORPHANAGE_3
    , THANKYOU_ORPHANAGE_4
    , THANKYOU_ORPHANAGE_5
    , THANKYOU_ORPHANAGE_6
    , THANKYOU_ORPHANAGE_7
    , THANKYOU_ORPHANAGE_8
    , THANKYOU_ORPHANAGE_9
    , THANKYOU_ORPHANAGE_10
    , THANKYOU_ORPHANAGE_11
    , THANKYOU_ORPHANAGE_12
    , THANKYOU_ORPHANAGE_13
    , THANKYOU_ORPHANAGE_14
    , THANKYOU_SCHOOL_1
    , THANKYOU_SCHOOL_2
    , THANKYOU_SCHOOL_3
    , THANKYOU_SCHOOL_4
    , THANKYOU_SCHOOL_5
    , THANKYOU_SCHOOL_6
    , THANKYOU_SCHOOL_7
    , THANKYOU_SCHOOL_8
    , THANKYOU_SCHOOL_9
    , THANKYOU_SCHOOL_10
    , THANKYOU_SCHOOL_11
    , THANKYOU_SCHOOL_12
    , THANKYOU_SCHOOL_13
    , THANKYOU_SCHOOL_14
    , THANKYOU_SHELTER_1
    , THANKYOU_SHELTER_2
    , THANKYOU_SHELTER_3
    , THANKYOU_SHELTER_4
    , THANKYOU_SHELTER_5
    , THANKYOU_SHELTER_6
    , THANKYOU_SHELTER_7
    , THANKYOU_SHELTER_8
    , THANKYOU_SHELTER_9
    , THANKYOU_SHELTER_10
    , THANKYOU_SHELTER_11
    , THANKYOU_SHELTER_12
    , THANKYOU_SHELTER_13
    , THANKYOU_SHELTER_14
    , THANKYOU_COOK_1
    , THANKYOU_COOK_2
    , THANKYOU_COOK_3
    , THANKYOU_COOK_4
    , THANKYOU_COOK_5
    , THANKYOU_COOK_6
    , THANKYOU_COOK_7
    , THANKYOU_COOK_8
    , THANKYOU_COOK_9
    , THANKYOU_COOK_10
    , THANKYOU_COOK_11
    , THANKYOU_COOK_12
    , THANKYOU_COOK_13
    , THANKYOU_COOK_14
    , THANKYOU_CHURCH_1
    , THANKYOU_CHURCH_2
    , THANKYOU_CHURCH_3
    , THANKYOU_CHURCH_4
    , THANKYOU_CHURCH_5
    , THANKYOU_CHURCH_6
    , THANKYOU_CHURCH_7
    , THANKYOU_CHURCH_8
    , THANKYOU_CHURCH_9
    , THANKYOU_CHURCH_10
    , THANKYOU_CHURCH_11
    , THANKYOU_CHURCH_12
    , THANKYOU_CHURCH_13
    , THANKYOU_CHURCH_14
    , THANKYOU_CHURCH_15
    , BLOOD_DONATION
    , HEALTH_INSPECTION
    , NEW_TUTORIAL_1
    , NEW_TUTORIAL_2
    , NEW_TUTORIAL_3
    , NEW_TUTORIAL_35
    , NEW_TUTORIAL_4
    , NEW_TUTORIAL_5
    , NEW_TUTORIAL_6
    , NEW_TUTORIAL_7
    , NEW_TUTORIAL_FAILED_1
    , NEW_TUTORIAL_FAILED_2
    , NEW_TUTORIAL_FAILED_3
}
