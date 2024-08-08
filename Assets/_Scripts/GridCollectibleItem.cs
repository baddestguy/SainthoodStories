using DG.Tweening;
using UnityEngine;

public class GridCollectibleItem : MonoBehaviour
{
    protected int TotalAmount = 0;
    protected int CountdownTimer = 0;
    protected MapTile MyTile;
    protected MapTile SpawnedTile;
    protected SacredItemBehaviour Behaviour;
    protected Tween MyTween;
    protected MapTile TargetTile;

    private void OnEnable()
    {
        GameClock.Ticked += OnTick;
    }

    public virtual void Init(MapTile tile, SacredItemBehaviour behave)
    {
        MyTile = tile;
        SpawnedTile = tile;
        TotalAmount = Random.Range(1, 6);
        MyTween = transform.DOMoveY(transform.position.y + 1f, 2f).SetLoops(-1, LoopType.Yoyo);
        Behaviour = behave;
    }

    public virtual void Collect()
    {
        DeleteCollectible();
    }

    public virtual void OnTick(double time, int day)
    {

    }

    public virtual void DeleteCollectible()
    {
        GridCollectibleManager.Instance.SpawnedTiles.Remove(MyTile);
        Destroy(gameObject);
    }

    private void OnDisable()
    {
        GameClock.Ticked -= OnTick;
    }
}
