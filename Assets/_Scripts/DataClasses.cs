using System.Collections.Generic;
using UnityEngine;

public class DataClasses : MonoBehaviour
{

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
    FATIGUED
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
    DAY = 0,
    NIGHT,
    PRERAIN,
    RAIN,
    SNOW
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
    EXTRA_INVENTORY,
    ENERGY_DRINK,
    ROSARY,
    SHOES,
    COOKING_UTENSILS,
    DISCOUNT_CARD
}

public enum BuildingState
{
    RUBBLE = 0,
    NORMAL,
    FIRE
}

public enum CustomEventType
{
    MARKET_HOURS = 0,
    RAIN,
    BABY_FEVER,
    MARKET_DISCOUNT,
    MARKET_MARKUP,
    WEEKDAY_MASS,
    SCHOOL_CLOSED,
    SICK,
    HIGH_SPIRIT,
    LOW_SPIRIT,
    HIGH_MORALE,
    LOW_MORALE,
    ORPHANAGE_BONUS,
    HOSPITAL_BONUS,
    VANDALISM,
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
    ,ORPHANAGE_COMPLETE
    ,SCHOOL_COMPLETE
    ,KITCHEN_COMPLETE
    ,SHELTER_COMPLETE
    ,CLOTHES_COMPLETE
    ,BABY_FAILED
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
    public string NameKey;
    public string DescriptionKey;
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
    public string LocalizationKey;
}

[System.Serializable]
public class StoryEventData
{
    public string Id;
    public int Week;
    public int Day;
    public double Time;
    public bool IsOrderedSequence;
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
    public int CP;
    public int Energy;
    public int Week;
    public int Day;
    public double Time;
    public int TutorialSteps;
    public double Money;
    public int HospitalRelationshipPoints;
    public int SchoolRelationshipPoints;
    public int OrphanageRelationshipPoints;
    public int ShelterRelationshipPoints;
    public int ClothesRelationshipPoints;
    public BuildingState HospitalBuildingState;
    public BuildingState SchoolBuildingState;
    public BuildingState OrphanageBuildingState;
    public BuildingState ShelterBuildingState;
    public BuildingState ClothesBuildingState;
    public BuildingState KitchenBuildingState;
}

