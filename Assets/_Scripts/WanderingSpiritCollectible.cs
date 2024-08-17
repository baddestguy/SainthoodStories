using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class WanderingSpiritCollectible : GridCollectibleItem
{
    private Renderer MyRenderer;
    private Material RedMaterial;
    private Material GreenMaterial;
    private Material WhiteMaterial;
    private GameObject MyExplosion;
    private GameObject MyExplosionEvil;

    public GameObject EvilAura;
    public Dictionary<PlayerFacingDirection, MapTile> AdjacentTiles;

    public void Awake()
    {
        MyRenderer = transform.GetComponentInChildren<Renderer>();
        WhiteMaterial = MyRenderer.material;
        RedMaterial = Resources.Load<Material>("Materials/Glow Mat Red");
        GreenMaterial = Resources.Load<Material>("Materials/Glow Mat Green");
        MyExplosion = Resources.Load<GameObject>("Environment/MemoryExplodeFx");
        MyExplosionEvil = Resources.Load<GameObject>("Environment/MemoryExplodeFxEvil");
    }

    public override void Init(MapTile tile, SacredItemBehaviour b)
    {
        base.Init(tile, b);
        CountdownTimer = Random.Range(10, 15);

        switch (Behaviour)
        {
            case SacredItemBehaviour.HARMLESS:
                if (MyRenderer.material != WhiteMaterial)
                {
                    MyRenderer.material = WhiteMaterial;
                }
                break;

            case SacredItemBehaviour.PATROL:
                TargetTile = FindTargetTile();

                break;
        }

        if (Behaviour != SacredItemBehaviour.HARMLESS)
        {
            if(Behaviour != SacredItemBehaviour.HOVER)
            {
                MyTween.Kill();
            }

            if (MyRenderer.material != RedMaterial)
            {
                EvilAura.SetActive(true);
                transform.DOScale(1, 1f);
                MyRenderer.material = RedMaterial;
            }
        }
    }

    public override void Collect()
    {
        if(Behaviour == SacredItemBehaviour.HARMLESS)
        {
            InventoryManager.Instance.AddWanderers(TotalAmount);
            Instantiate(MyExplosion, transform.position, transform.rotation);
            ExteriorCamera.Instance.GetComponent<CameraControls>()?.MyCamera.DOShakeRotation(1f, 0.25f);
            SoundManager.Instance.PlayOneShotSfx("ActionButton_SFX");
        }
        else
        {
            MissionManager.Instance.FaithPointsPermanentlyLost += 1;
            MissionManager.Instance.UpdateFaithPoints(-1);
            TreasuryManager.Instance.LoseTempMoney();

            Instantiate(MyExplosionEvil, transform.position, transform.rotation);
            ExteriorCamera.Instance.GetComponent<CameraControls>()?.MyCamera.DOShakeRotation(1f, 1.5f);
            WeatherManager.Instance.DayNightCycle.Light.color = Color.red;
            SoundManager.Instance.PlayOneShotSfx("EvilImpact");

            if (GameSettings.Instance.TUTORIAL_MODE)
            {
                GameManager.Instance.ReloadLevel();
            }
        }
        base.Collect();
    }

    public override void OnTick(double time, int day)
    {
        if (!GameClock.DeltaTime) return;
        ExhibitBehaviour(time, day);
    }

    private void ExhibitBehaviour(double time, int day) 
    {  
        List<MapTile> path = new List<MapTile>();
        
        switch (Behaviour)
        {
            case SacredItemBehaviour.HARMLESS:
                CountdownTimer--;
               // Debug.Log("DISSAPEARING IN " + CountdownTimer);
                //if (CountdownTimer <= 0)
                //{
                //    DeleteCollectible();
                //}
                break;

            case SacredItemBehaviour.CHASE:
                if (!MyTween.IsPlaying())
                {
                    path = PathToTile(GameManager.Instance.Player.GetCurrentTile());
                    if (path.Count > 0)
                    {
                        MyTile.TileType = TileType.ROAD;
                        MyTile = path[0];
                        MyTween = transform.DOMove(MyTile.transform.position, 2f);
                    }
                }
                break;

            case SacredItemBehaviour.PATROL:
                if (!MyTween.IsPlaying())
                {
                    if (Mathf.FloorToInt((float)time / 0.25f) % 2 == 0)
                    {
                        MyTile.TileType = TileType.ROAD;
                        MyTile = TargetTile;
                        MyTween = transform.DOMove(MyTile.transform.position, 5f);
                    }
                    else
                    {
                        MyTile.TileType = TileType.ROAD;
                        MyTile = SpawnedTile;
                        MyTween = transform.DOMove(SpawnedTile.transform.position, 5f);
                    }
                }
                break;

            case SacredItemBehaviour.BOUNCE:
                if (!MyTween.IsPlaying())
                {
                    if (Mathf.FloorToInt((float)time / 0.25f) % 2 == 0)
                    {
                        MyTile.TileType = TileType.ROAD;
                        MyTile = GameManager.Instance.Player.GetCurrentTile();
                        MyTween = transform.DOJump(MyTile.transform.position, 5f, 1, 5f);
                    }
                    else
                    {
                        MyTile.TileType = TileType.ROAD;
                        MyTile = SpawnedTile;
                        MyTween = transform.DOJump(MyTile.transform.position, 5f, 1, 5f);
                    }
                }
                break;

            case SacredItemBehaviour.RUNAWAY:
                if (Mathf.FloorToInt((float)time / 0.25f) % 2 == 0)
                {
                    path = PathToTile(GameManager.Instance.Player.GetCurrentTile());
                    if (path.Count > 1)
                    {
                        path.RemoveAt(path.Count - 1);
                        MyTile.TileType = TileType.ROAD;
                        MyTile = path[path.Count-1];
                        transform.DOKill(true);
                        var tweenPath = path.Select(x => x.transform.position).ToArray();
                        transform.DOPath(tweenPath, 5f);
                    }
                }
                else
                {
                    path = PathToTile(SpawnedTile);
                    if (path.Count > 0)
                    {
                        MyTile.TileType = TileType.ROAD;
                        MyTile = path[path.Count-1];
                        transform.DOKill(true);
                        var tweenPath = path.Select(x => x.transform.position).ToArray();
                        transform.DOPath(tweenPath, 5f);
                    }
                }
                break;
        }
        MyTile.TileType = TileType.BUILDING;

    }

    private MapTile FindTargetTile()
    {
        var currentTile = MyTile;

        var map = GameManager.Instance.Player.Map;
        AdjacentTiles = map.GetAdjacentTiles(MyTile);
        var directions = AdjacentTiles.Keys.Where(d => AdjacentTiles[d].TileType != TileType.BUILDING);

        if (!directions.Any()) return currentTile;
        
        var direction = directions.ToArray()[Random.Range(0, directions.Count() - 1)];
        currentTile = AdjacentTiles[direction];

        while (currentTile.TileType != TileType.BUILDING)
        {
            AdjacentTiles = map.GetAdjacentTiles(currentTile);
            if (!AdjacentTiles.ContainsKey(direction)) break;
            currentTile = AdjacentTiles[direction];
        }

        return currentTile;
    }

    private List<MapTile> PathToTile(MapTile target)
    {
        var currentTile = MyTile;
        var map = GameManager.Instance.Player.Map;
        var path = new List<MapTile>();

        if(target != null && target.TileType == TileType.BUILDING)
        {
            return path;
        }

        while (currentTile != target)
        {
            AdjacentTiles = map.GetAdjacentTiles(currentTile);
            var shortestDistance = 100000f;
            foreach (var tile in AdjacentTiles.Values)
            {
                if (tile.TileType == TileType.BUILDING || path.Contains(tile)) continue;
                if (Behaviour == SacredItemBehaviour.RUNAWAY && tile.TileType == TileType.PLAYER) continue;

                var newDistance = Vector3.Distance(tile.transform.position, target.transform.position);
                if (newDistance < shortestDistance)
                {
                    shortestDistance = newDistance;
                    currentTile = tile;
                }
            }
            path.Add(currentTile);
            if(path.Count >= 64)
            {
                return path.Distinct().ToList();
            }
        }

        return path;
    }
}
