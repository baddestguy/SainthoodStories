using System.Collections.Generic;
using UnityEngine;

public class DataClasses : MonoBehaviour
{

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
    NORMAL,
    RUBBLE,
    FIRE
}

public enum EventType
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
    RIOTS
}

public enum EventGroup
{
    DAILY = 0,
    IMMEDIATE
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
public class CustomEventData
{
    public EventType Id;
    public EventGroup EventGroup;
    public int Weight;
    public float Cost;
    public float Gain;
    public bool IsOrderedSequence;
    public string LocalizationKey;
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
