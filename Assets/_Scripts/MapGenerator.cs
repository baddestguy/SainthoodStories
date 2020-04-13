using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;

public enum TileType
{
    WALKABLE = 0,
    INTERACTABLE,
    BARRIER
}

public class TileData
{
    public int Id { get; }
    public int TileId { get; }
    public TileType TileType { get; }

    public TileData(int id, int tileId, TileType tileType) {
        Id = id;
        TileId = tileId;
        TileType = tileType;
    }
}

public class MapData
{
    public int MapId { get; }
    public IEnumerable<TileData> Tiles{ get; }
    public int Rows { get; }
    public int Columns { get; }

    public MapData(int mapId, int rows, int columns, IEnumerable<TileData> tiles) {
        MapId = mapId;
        Rows = rows;
        Columns = columns;
        Tiles = tiles;
    }
}

public class MapGenerator : MonoBehaviour
{
    public List<MapData> MapGrounds = new List<MapData>();
    public List<MapData> MapSurface = new List<MapData>();
    public TextAsset MapGroundsCSV, MapSurfaceCSV;
    public List<MapTile> MapTiles = new List<MapTile>();

    private int CurrentMapId;
    private bool Loading;

    public void LoadAllMaps()
    {
        Loading = true;
        StartCoroutine("LoadAllMapsAsync");
    }

    public void DisplayMap(int mapId, UnityAction complete)
    {
        StartCoroutine(DisplayMapAsync(mapId, complete));
    }

    private IEnumerator LoadAllMapsAsync() 
    {
        yield return StartCoroutine(LoadMapObject(MapGroundsCSV, MapGrounds));

        yield return null;

        yield return StartCoroutine(LoadMapObject(MapSurfaceCSV, MapSurface));
        
        Loading = false;
    }

    IEnumerator LoadMapObject(string mapObject, List<MapData> mapObjectList) {
        TextAsset mapGroundsText = Resources.Load<TextAsset>(mapObject);
        yield return StartCoroutine(LoadMapObject(mapGroundsText, mapObjectList));
    }

    //Overload that uses files dragged in Inspector
    IEnumerator LoadMapObject(TextAsset mapGroundsText, List<MapData> mapObjectList)
    {
        string[] mapRawData = mapGroundsText.text.Split('x');
        int mapId = 0;
        int tileId = 0; //TODO: HACK!

        foreach (var map in mapRawData)
        {
            var tileList = new List<TileData>();
            string[] mapRows = map.Split('\n');

            int rowCount = 0;
            int columnCount = 0;

            for (int i = 0; i < mapRows.Length; i++)
            {
                string row = mapRows[i];
                if (string.IsNullOrEmpty(row) || string.IsNullOrWhiteSpace(row)) continue;

                string[] columns = row.Split(',');
                foreach (var col in columns)
                {
                    tileId++;
                    tileList.Add(new TileData(tileId, int.Parse(col), TileType.WALKABLE)); //TODO: HACK! Read TileType from FILE! Not all tiles are WALKABLE!
                }

                columnCount = columns.Length;
                rowCount++;
            }

            mapId++;
            mapObjectList.Add(new MapData(mapId, rowCount, columnCount, tileList));
            yield return null;
        }

        Debug.Log("MAPS: " + MapGrounds.Count);
        foreach (var map in MapGrounds)
        {
            Debug.Log("Map row: " + map.Rows);
            Debug.Log("Map columns: " + map.Columns);
        }
    }

    IEnumerator DisplayMapAsync(int mapId, UnityAction complete) {
        while (Loading) yield return null;

        CurrentMapId = mapId;
        var mapTileObj = Resources.Load("Environment/MapTile");
        var mapGrounds = MapGrounds[mapId];
        var mapSurface = MapSurface[mapId];
        var sprites = Resources.LoadAll<Sprite>("Environment/EnvironmentSprites");
        Vector3 originalPosition = new Vector3(0, 0, 0);
        Vector3 shiftVectorY = new Vector2(0, 0.8f);
        Vector3 shiftVectorX = new Vector2(1.3f, 0);

        int columnCount = 0;
        int rowCount = 0;
        for (int i = 0; i < mapGrounds.Tiles.Count(); i++) {
            if (mapGrounds.Tiles.ElementAt(i).TileId == -1) continue;

            GameObject mapGroundTile = GameObject.Instantiate(mapTileObj, originalPosition, Quaternion.identity) as GameObject;
            Transform mapTileTransform = mapGroundTile.GetComponent<Transform>();
            mapTileTransform.parent = transform;

            if(columnCount >= mapGrounds.Columns) {
                rowCount++;
                columnCount = 0;
            }

            mapTileTransform.position += (columnCount * shiftVectorX) - (rowCount * shiftVectorY);

            mapGroundTile.GetComponent<MapTile>().Init(mapGrounds.Tiles.ElementAt(i), sprites);

            MapTiles.Add(mapGroundTile.GetComponent<MapTile>());

            columnCount++;

            yield return null;
        }

        columnCount = 0;
        rowCount = 0;
        for (int i = 0; i < mapSurface.Tiles.Count(); i++) {
            if (mapSurface.Tiles.ElementAt(i).TileId == -1)
            {
                if (columnCount >= mapSurface.Columns)
                {
                    rowCount++;
                    columnCount = 0;
                }
                columnCount++;
                continue;
            }

            GameObject mapGroundTile = GameObject.Instantiate(mapTileObj, originalPosition, Quaternion.identity) as GameObject;
            Transform mapTileTransform = mapGroundTile.GetComponent<Transform>();
            mapTileTransform.parent = transform;

            if (columnCount >= mapSurface.Columns) {
                rowCount++;
                columnCount = 0;
            }

            mapTileTransform.position += (columnCount * shiftVectorX) - (rowCount * shiftVectorY - new Vector3(0,1,0)); //Hack
            mapGroundTile.GetComponent<MapTile>().Init(mapSurface.Tiles.ElementAt(i), sprites, 2);

            columnCount++;

            yield return null;
        }

        complete?.Invoke();
    }

    public IEnumerable<MapTile> GetAdjacentTiles(MapTile mapTile)
    {
        var currentPosition = -1;
        var list = new List<MapTile>();

        for (int i = 0; i < MapTiles.Count; i++)
        {
            if (mapTile.TileData.Id == MapTiles[i].TileData.Id)
            {
                currentPosition = i;
                var tile = (i - MapGrounds[CurrentMapId].Columns) < 0 ? null : MapTiles[i - MapGrounds[CurrentMapId].Columns];
                if (tile != null && tile.TileData.TileType != TileType.BARRIER) 
                {
                    list.Add(tile);
                }
                tile = (i - 1) < 0 ? null : MapTiles[i - 1];
                if (tile != null && tile.TileData.TileType != TileType.BARRIER) 
                {
                    list.Add(tile);
                }
                tile = (i + 1) > MapTiles.Count ? null : MapTiles[i + 1];
                if (tile != null && tile.TileData.TileType != TileType.BARRIER)
                {
                    list.Add(tile);
                }
                tile = (i + MapGrounds[CurrentMapId].Columns) > MapTiles.Count ? null : MapTiles[i + MapGrounds[CurrentMapId].Columns];
                if (tile != null && tile.TileData.TileType != TileType.BARRIER)
                {
                    list.Add(tile);
                }
                return list;
            }
        }
        return null;
    }
}
