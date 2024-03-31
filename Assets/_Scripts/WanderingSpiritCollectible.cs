using UnityEngine;

public class WanderingSpiritCollectible : GridCollectibleItem
{
    private SacredItemBehaviour Behaviour;
    private Renderer MyRenderer;
    private Material RedMaterial;
    private Material GreenMaterial;
    private Material WhiteMaterial;
    private GameObject MyExplosion;

    public void Awake()
    {
        MyRenderer = transform.GetComponentInChildren<Renderer>();
        WhiteMaterial = MyRenderer.material;
        RedMaterial = Resources.Load<Material>("Materials/Glow Mat Red");
        GreenMaterial = Resources.Load<Material>("Materials/Glow Mat Green");
        MyExplosion = Resources.Load<GameObject>("MemoryExplodeFx");
    }

    public override void Init(MapTile tile)
    {
        base.Init(tile);
        CountdownTimer = Random.Range(10, 15);
        ExhibitBehaviour(GameManager.Instance.GameClock.Time, GameManager.Instance.GameClock.Day);
    }

    public override void Collect()
    {
        if(Behaviour == SacredItemBehaviour.HOVER)
        {
            InventoryManager.Instance.AddWanderers(TotalAmount);
        }
        else if (Behaviour == SacredItemBehaviour.CHASE)
        {
            MissionManager.Instance.UpdateFaithPoints(-1);
            TreasuryManager.Instance.LoseTempMoney();
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
        if(time > 19 || time < 5)
        {
            Behaviour = SacredItemBehaviour.CHASE;

            if(MyRenderer.material != RedMaterial)
            {
                MyRenderer.material = RedMaterial;
            }
        }
        else
        {
            Behaviour = SacredItemBehaviour.HOVER;
            if(MyRenderer.material != WhiteMaterial)
            {
                MyRenderer.material = WhiteMaterial;
            }
        }

        switch (Behaviour)
        {
            case SacredItemBehaviour.HOVER:
                CountdownTimer--;
                Debug.Log("DISSAPEARING IN " + CountdownTimer);
                if (CountdownTimer <= 0)
                {
                    Delete();
                }
                break;

            case SacredItemBehaviour.CHASE:

                break;
        }

    }

}
