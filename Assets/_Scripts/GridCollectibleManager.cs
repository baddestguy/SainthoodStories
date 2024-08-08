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
    public List<SacredItemBehaviour> Behaviours = new List<SacredItemBehaviour>();

    private GridCollectibleItem NewCollectibleSpawned;
    public int SacredItemSpawned = 0;

    bool Spawning;

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

    private IEnumerator SpawnCollectible(GameObject resource, SacredItemBehaviour behaviour)
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
        NewCollectibleSpawned.Init(tile, behaviour);
        NewCollectibleSpawned.transform.SetParent(transform);

        SpawnedTiles.Add(tile);
    }

    public void GenerateCollectibles(int maxCount = 15)
    {
        StartCoroutine(GenerateCollectiblesAsync(maxCount));
    }

    IEnumerator GenerateCollectiblesAsync(int maxCount)
    {
        while (Spawning)
            yield return null;
        
        Spawning = true;
        var count = maxCount - SpawnedTiles.Count;
        if (!Behaviours.Any())
        {
            Behaviours.Add(SacredItemBehaviour.HARMLESS);
        }
        for (int i = 0; i < count; i++)
        {
            yield return StartCoroutine(SpawnCollectible(WanderingSpiritResource, Behaviours[Random.Range(0,Behaviours.Count()-1)]));
        }
        Spawning = false;
    }

    public void OnTick(double time, int day)
    {
        if (!GameClock.DeltaTime) return;

        if(time == 19)
        {
            SpawnedTiles.Clear();
            BroadcastMessage("DeleteCollectible", SendMessageOptions.DontRequireReceiver);
        }

        if (time > 19 || time < 2)
        {
            StartCoroutine(SpawnSacredItemAsync());

            switch (day)
            {
                case 2:
                    Behaviours.Clear();
                    Behaviours.Add(SacredItemBehaviour.HOVER);
                    GenerateCollectibles(1);
                    break;
                case 3:
                    Behaviours.Clear();
                    Behaviours.Add(SacredItemBehaviour.HOVER);
                    GenerateCollectibles(2);
                    break;
                case 4:
                    Behaviours.Clear();
                    Behaviours.Add(SacredItemBehaviour.HOVER);
                    GenerateCollectibles(4);
                    break;
                case 5:
                    Behaviours.Clear();
                    Behaviours.Add(SacredItemBehaviour.HOVER);
                    GenerateCollectibles(6);
                    break;
                case 6:
                    Behaviours.Clear();
                    Behaviours.Add(SacredItemBehaviour.HOVER);
                    GenerateCollectibles(7);
                    break;
                case 7:
                    Behaviours.Clear();
                    Behaviours.Add(SacredItemBehaviour.HOVER);
                    GenerateCollectibles(8);
                    break;
                case 8:
                    Behaviours.Clear();
                    Behaviours.Add(SacredItemBehaviour.HOVER);
                    GenerateCollectibles(10);
                    break;
                case 9:
                    Behaviours.Clear();
                    Behaviours.Add(SacredItemBehaviour.HOVER);
                    GenerateCollectibles(12);
                    break;
                case 10:
                    Behaviours.Clear();
                    Behaviours.Add(SacredItemBehaviour.HOVER);
                    GenerateCollectibles(15);

                    break;
                case 11:
                    Behaviours.Clear();
                    Behaviours.Add(SacredItemBehaviour.PATROL);
                    GenerateCollectibles(1);

                    break;
                case 12:
                    Behaviours.Clear();
                    Behaviours.Add(SacredItemBehaviour.PATROL);
                    GenerateCollectibles(2);

                    break;
                case 13:
                    Behaviours.Clear();
                    Behaviours.Add(SacredItemBehaviour.PATROL);
                    GenerateCollectibles(3);

                    break;
                case 14:
                    Behaviours.Clear();
                    Behaviours.Add(SacredItemBehaviour.HOVER);
                    Behaviours.Add(SacredItemBehaviour.PATROL);
                    GenerateCollectibles(3);

                    break;
                case 15:
                    Behaviours.Clear();
                    Behaviours.Add(SacredItemBehaviour.HOVER);
                    Behaviours.Add(SacredItemBehaviour.PATROL);
                    GenerateCollectibles(3);

                    break;
                case 16:
                    Behaviours.Clear();
                    Behaviours.Add(SacredItemBehaviour.HOVER);
                    Behaviours.Add(SacredItemBehaviour.PATROL);
                    GenerateCollectibles(4);

                    break;
                case 17:
                    Behaviours.Clear();
                    Behaviours.Add(SacredItemBehaviour.HOVER);
                    Behaviours.Add(SacredItemBehaviour.PATROL);
                    GenerateCollectibles(4);

                    break;
                case 18:
                    Behaviours.Clear();
                    Behaviours.Add(SacredItemBehaviour.HOVER);
                    Behaviours.Add(SacredItemBehaviour.PATROL);
                    GenerateCollectibles(4);

                    break;
                case 19:
                    Behaviours.Clear();
                    Behaviours.Add(SacredItemBehaviour.BOUNCE);
                    GenerateCollectibles(1);

                    break;
                case 20:
                    Behaviours.Clear();
                    Behaviours.Add(SacredItemBehaviour.BOUNCE);
                    Behaviours.Add(SacredItemBehaviour.PATROL);
                    Behaviours.Add(SacredItemBehaviour.HOVER);
                    GenerateCollectibles(4);

                    break;
                case 21:
                    Behaviours.Clear();
                    Behaviours.Add(SacredItemBehaviour.HOVER);
                    Behaviours.Add(SacredItemBehaviour.BOUNCE);
                    Behaviours.Add(SacredItemBehaviour.PATROL);
                    GenerateCollectibles(5);

                    break;
                case 22:
                    Behaviours.Clear();
                    Behaviours.Add(SacredItemBehaviour.HOVER);
                    Behaviours.Add(SacredItemBehaviour.PATROL);
                    Behaviours.Add(SacredItemBehaviour.BOUNCE);
                    GenerateCollectibles(5);

                    break;
                case 23:
                    Behaviours.Clear();
                    Behaviours.Add(SacredItemBehaviour.HOVER);
                    Behaviours.Add(SacredItemBehaviour.PATROL);
                    Behaviours.Add(SacredItemBehaviour.BOUNCE);
                    GenerateCollectibles(5);

                    break;
                case 24:
                    Behaviours.Clear();
                    Behaviours.Add(SacredItemBehaviour.HOVER);
                    Behaviours.Add(SacredItemBehaviour.PATROL);
                    Behaviours.Add(SacredItemBehaviour.BOUNCE);
                    GenerateCollectibles(5);

                    break;
                case 25:
                    Behaviours.Clear();
                    Behaviours.Add(SacredItemBehaviour.HOVER);
                    Behaviours.Add(SacredItemBehaviour.PATROL);
                    Behaviours.Add(SacredItemBehaviour.BOUNCE);
                    GenerateCollectibles(5);

                    break;
                case 26:
                    Behaviours.Clear();
                    Behaviours.Add(SacredItemBehaviour.HOVER);
                    Behaviours.Add(SacredItemBehaviour.BOUNCE);
                    Behaviours.Add(SacredItemBehaviour.PATROL);
                    GenerateCollectibles(5);

                    break;
                case 27:
                    Behaviours.Clear();
                    Behaviours.Add(SacredItemBehaviour.CHASE);
                    GenerateCollectibles(1);

                    break;
                case 28:
                    Behaviours.Clear();
                    Behaviours.Add(SacredItemBehaviour.CHASE);
                    GenerateCollectibles(1);

                    break;
                case 29:
                    Behaviours.Clear();
                    Behaviours.Add(SacredItemBehaviour.CHASE);
                    GenerateCollectibles(2);

                    break;
                case 30:
                    Behaviours.Clear();
                    Behaviours.Add(SacredItemBehaviour.CHASE);
                    GenerateCollectibles(2);

                    break;
                case 31:
                    Behaviours.Clear();
                    Behaviours.Add(SacredItemBehaviour.CHASE);
                    Behaviours.Add(SacredItemBehaviour.HOVER);
                    Behaviours.Add(SacredItemBehaviour.BOUNCE);
                    Behaviours.Add(SacredItemBehaviour.PATROL);
                    GenerateCollectibles(5);

                    break;
                case 32:
                    Behaviours.Clear();
                    Behaviours.Add(SacredItemBehaviour.CHASE);
                    Behaviours.Add(SacredItemBehaviour.HOVER);
                    Behaviours.Add(SacredItemBehaviour.BOUNCE);
                    Behaviours.Add(SacredItemBehaviour.PATROL);
                    GenerateCollectibles(5);

                    break;
                case 33:
                    Behaviours.Clear();
                    Behaviours.Add(SacredItemBehaviour.CHASE);
                    Behaviours.Add(SacredItemBehaviour.HOVER);
                    Behaviours.Add(SacredItemBehaviour.BOUNCE);
                    Behaviours.Add(SacredItemBehaviour.PATROL);
                    GenerateCollectibles(5);

                    break;
            }
        }
    }

    IEnumerator SpawnSacredItemAsync()
    {
        if (SacredItemSpawned >= (GameDataManager.Instance.CollectibleObjectivesData[MissionManager.Instance.CurrentCollectibleMissionId].Amount - MissionManager.Instance.CurrentCollectibleCounter)) yield break;
        if (MissionManager.Instance.CurrentCollectibleCounter >= GameDataManager.Instance.CollectibleObjectivesData[MissionManager.Instance.CurrentCollectibleMissionId].Amount) yield break;

        var collectibleList = new List<string>();
        collectibleList.AddRange(GetCollectibleList());

        if (collectibleList.Count == 0) yield break; //Player has collected all collectibles
        SacredItemSpawned++;

        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(SpawnCollectible(SacredItemResource, SacredItemBehaviour.HARMLESS));
        
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

    public void ClearAll()
    {
        Behaviours.Clear();
        SpawnedTiles.Clear();
        BroadcastMessage("DeleteCollectible", SendMessageOptions.DontRequireReceiver);
    }


    private void OnDisable()
    {
        GameClock.Ticked -= OnTick;
    }

}
