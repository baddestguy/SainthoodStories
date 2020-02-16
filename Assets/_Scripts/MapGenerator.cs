using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class TileData
{
    public int TileId { get; }

    public TileData(int tileId) {
        TileId = tileId;
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

    public IEnumerator LoadAllMaps() 
    {
        yield return StartCoroutine(LoadMapObject(MapGroundsCSV, MapGrounds));

        yield return null;

        yield return StartCoroutine(LoadMapObject(MapSurfaceCSV, MapSurface));

        StartCoroutine("DisplayMap", 0);
    }

    IEnumerator LoadMapObject(string mapObject, List<MapData> mapObjectList) {
        TextAsset mapGroundsText = Resources.Load<TextAsset>(mapObject);
        string[] mapRawData = mapGroundsText.text.Split('x');
        int mapId = 0;

        foreach (var map in mapRawData) {
            var tileList = new List<TileData>();
            string[] mapRows = map.Split('\n');

            int rowCount = 0;
            int columnCount = 0;

            foreach (string row in mapRows) {
                if (string.IsNullOrEmpty(row) || string.IsNullOrWhiteSpace(row)) continue;

                string[] columns = row.Split(',');
                foreach (var col in columns) {
                    tileList.Add(new TileData(int.Parse(col)));
                }

                columnCount = columns.Length;
                rowCount++;
            }

            mapId++;
            mapObjectList.Add(new MapData(mapId, rowCount, columnCount, tileList));
            yield return null;
        }

        Debug.Log("MAPS: " + MapGrounds.Count);
        foreach (var map in MapGrounds) {
            Debug.Log("Map row: " + map.Rows);
            Debug.Log("Map columns: " + map.Columns);
        }
    }

    //Overload that uses files dragged in Inspector
    IEnumerator LoadMapObject(TextAsset mapGroundsText, List<MapData> mapObjectList)
    {
        string[] mapRawData = mapGroundsText.text.Split('x');
        int mapId = 0;

        foreach (var map in mapRawData)
        {
            var tileList = new List<TileData>();
            string[] mapRows = map.Split('\n');

            int rowCount = 0;
            int columnCount = 0;

            foreach (string row in mapRows)
            {
                if (string.IsNullOrEmpty(row) || string.IsNullOrWhiteSpace(row)) continue;

                string[] columns = row.Split(',');
                foreach (var col in columns)
                {
                    tileList.Add(new TileData(int.Parse(col)));
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

    IEnumerator DisplayMap(int mapId) {
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
            GameObject mapGroundTile = GameObject.Instantiate(mapTileObj, originalPosition, Quaternion.identity) as GameObject;
            Transform mapTileTransform = mapGroundTile.GetComponent<Transform>();

            if(columnCount >= mapGrounds.Columns) {
                rowCount++;
                columnCount = 0;
            }

            mapTileTransform.position += (columnCount * shiftVectorX) - (rowCount * shiftVectorY);

            mapGroundTile.GetComponent<MapTile>().LoadSprite(mapGrounds.Tiles.ElementAt(i), sprites);

            columnCount++;

            yield return null;
        }

        columnCount = 0;
        rowCount = 0;
        for (int i = 0; i < mapSurface.Tiles.Count(); i++) {
            GameObject mapGroundTile = GameObject.Instantiate(mapTileObj, originalPosition, Quaternion.identity) as GameObject;
            Transform mapTileTransform = mapGroundTile.GetComponent<Transform>();

            if (columnCount >= mapSurface.Columns) {
                rowCount++;
                columnCount = 0;
            }

            mapTileTransform.position += (columnCount * shiftVectorX) - (rowCount * shiftVectorY - new Vector3(0,1,0)); //Hack
            Debug.Log(mapSurface.Tiles.ElementAt(i).TileId);
            mapGroundTile.GetComponent<MapTile>().LoadSprite(mapSurface.Tiles.ElementAt(i), sprites, 2);

            columnCount++;

            yield return null;
        }
    }

    private void OnEnable() {
        StartCoroutine("LoadAllMaps");
    }
}
