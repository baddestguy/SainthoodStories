using DG.Tweening;
using UnityEngine;

public class GridCollectibleItem : MonoBehaviour
{
    protected int TotalAmount = 0;
    protected int CountdownTimer = 0;
    protected MapTile MyTile;


    private void OnEnable()
    {
        GameClock.Ticked += OnTick;
    }

    public virtual void Init(MapTile tile)
    {
        MyTile = tile;
        TotalAmount = Random.Range(1, 6);
        transform.DOMoveY(transform.position.y + 1f, 2f).SetLoops(-1, LoopType.Yoyo);
    }

    public virtual void Collect()
    {
        Delete();
    }

    public virtual void OnTick(double time, int day)
    {

    }

    public virtual void Delete()
    {
        GridCollectibleManager.Instance.SpawnedTiles.Remove(MyTile);
        Destroy(gameObject);
    }

    private void OnDisable()
    {
        GameClock.Ticked -= OnTick;
    }
}
