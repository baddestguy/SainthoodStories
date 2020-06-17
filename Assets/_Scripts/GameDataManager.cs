using System.Collections.Generic;
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }
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
    NONE = 0,
    PRESTORM,
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