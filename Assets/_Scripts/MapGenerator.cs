using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class MapGenerator : MonoBehaviour
{
    public List<MapData> MapGrounds = new List<MapData>();
    public List<MapData> MapSurface = new List<MapData>();
    public TextAsset MapGroundsCSV, MapSurfaceCSV;
    public List<MapTile> MapTiles = new List<MapTile>();
    public List<InteractableObject> Interactables = new List<InteractableObject>();

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
        int tileId = 0; //TODO: HACK! Read from FILE!

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
                    string[] tileData = col.Split('|');
                    if (tileData.Length < 2) continue;
                    tileId++;
                    tileList.Add(new TileData(tileId, int.Parse(tileData[0]), (TileType)int.Parse(tileData[1])));
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
            if (mapGrounds.Tiles.ElementAt(i).TileSpriteId == -1) continue;

            GameObject mapGroundTile = GameObject.Instantiate(mapTileObj, originalPosition, Quaternion.identity) as GameObject;
            Transform mapTileTransform = mapGroundTile.GetComponent<Transform>();
            mapTileTransform.parent = transform;

            if(columnCount >= mapGrounds.Columns) {
                rowCount++;
                columnCount = 0;
            }

            mapTileTransform.position += (columnCount * shiftVectorX) - (rowCount * shiftVectorY);

            mapGroundTile.AddComponent<MapTile>().Init(mapGrounds.Tiles.ElementAt(i), sprites);

            MapTiles.Add(mapGroundTile.GetComponent<MapTile>());

            columnCount++;

            yield return null;
        }

        columnCount = 0;
        rowCount = 0;
        for (int i = 0; i < mapSurface.Tiles.Count(); i++) {
            if (mapSurface.Tiles[i].TileSpriteId == -1)
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

            mapTileTransform.position += (columnCount * shiftVectorX) - (rowCount * shiftVectorY - new Vector3(0,0,-5)); //TODO: Hack!
            var interac = MapTile.GetInteractableObject(mapSurface.Tiles[i], mapGroundTile, MapTiles[i], sprites, (i+1));
            
            Interactables.Add(interac);

            columnCount++;

            yield return null;
        }

        transform.position = new Vector3(-6.2f, -3.2f, -4.9f); //TODO: HACK!
        complete?.Invoke();
    }

    public MapTile GetPlayerStartingTile()
    {
        return Interactables.Where(i => i.TileData.TileType == TileType.PLAYER).FirstOrDefault(); //TODO: HACK!
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
