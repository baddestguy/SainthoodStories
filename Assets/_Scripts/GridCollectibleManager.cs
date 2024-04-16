using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridCollectibleManager : MonoBehaviour
{
    public static GridCollectibleManager Instance { get; private set; }
    private GameObject WanderingSpiritResource;
    private GameObject SacredItemResource;
    public List<MapTile> SpawnedTiles = new List<MapTile>();

    private GridCollectibleItem NewCollectibleSpawned;
    public bool SacredItemSpawned;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        WanderingSpiritResource = Resources.Load<GameObject>("Environment/WanderingSpiritCollectible");
        SacredItemResource = Resources.Load<GameObject>("Environment/SacredItemCollectible");

    }


    private void OnEnable()
    {
        GameClock.Ticked += OnTick;
    }

    private IEnumerator SpawnCollectible(GameObject resource)
    {
        MapTile tile;
        var player = GameManager.Instance.Player;
        bool houseSpawn;
        do
        {
            houseSpawn = false;

            tile = player.Map.MapTiles[Random.Range(0, player.Map.MapTiles.Count)];
            var houses = GameManager.Instance.Houses;
            for(int i = 0; i < houses.Length; i++)
            {
                if(houses[i].CurrentGroundTile == tile)
                {
                    houseSpawn = true;
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
        while (tile == null || houseSpawn || SpawnedTiles.Contains(tile) || GameManager.Instance.Player.GetCurrentTile() == tile);

        var go = Instantiate(resource);
        go.transform.position = tile.transform.position;
        NewCollectibleSpawned = go.GetComponent<GridCollectibleItem>();
        NewCollectibleSpawned.Init(tile);

        SpawnedTiles.Add(tile);
    }

    public void GenerateCollectibles()
    {
        if (SpawnedTiles.Count > 0) return;

        for (int i = 0; i < 10; i++)
        {
            StartCoroutine(SpawnCollectible(WanderingSpiritResource));
        }
    }

    public void OnTick(double time, int day)
    {
        if (!GameClock.DeltaTime) return;

        var random = Random.Range(0, 100);
        if (random > 30) return;


        if (time > 19 || time < 5)
        {
            StartCoroutine(SpawnSacredItemAsync());

            if (SpawnedTiles.Count < 5)
            {
                GenerateCollectibles();
            }
        }
    }

    IEnumerator SpawnSacredItemAsync()
    {
        if (SacredItemSpawned) yield break;
        if (MissionManager.Instance.CurrentCollectibleCounter >= GameDataManager.Instance.CollectibleObjectivesData[MissionManager.Instance.CurrentCollectibleMissionId].Amount) yield break;

        var collectibleList = new List<string>();
        collectibleList.AddRange(GetCollectibleList());

        if (collectibleList.Count == 0) yield break; //Player has collected all collectibles

        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(SpawnCollectible(SacredItemResource));
        
        SacredItemSpawned = true;
        var it = collectibleList[Random.Range(0, collectibleList.Count - 1)];
        collectibleList.Remove(it);
        (NewCollectibleSpawned as SacredItemCollectible).SacredName = it;
        (NewCollectibleSpawned as SacredItemCollectible).SacredDescription = GameDataManager.Instance.GetCollectibleData(it).Description;
    }

    private List<string> GetCollectibleList()
    {
        var saveData = GameManager.Instance.SaveData;

        if (saveData.WorldCollectibles == null || saveData.WorldCollectibles.Count() == 0)
        {
            var cols = GameDataManager.Instance.CollectibleData;
            var newList = (from objList in cols.Values
                           from obj in objList
                           where !saveData.Collectibles?.Contains(obj.Name) ?? true
                           select obj.Name).Distinct().ToList();
            GameManager.Instance.WorldCollectibles = newList;
            return newList;
        }

        return saveData.WorldCollectibles.ToList();
    }

    private void OnDisable()
    {
        GameClock.Ticked -= OnTick;
    }

}
