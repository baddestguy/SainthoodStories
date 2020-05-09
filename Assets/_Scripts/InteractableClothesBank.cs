public class InteractableClothesBank : InteractableHouse
{
    public int ClothePoints;

    protected override void Start()
    {
        UI.DonatedClothes += GiveClothes;
        base.Start();
    }

    public override void OnPlayerMoved(Energy energy, MapTile tile)
    {
        if (tile.GetInstanceID() == GetInstanceID())
        {
            UI.Instance.EnableClothes(true, this);
        }
        else
        {
            UI.Instance.EnableClothes(false, this);
        }
    }

    public void GiveClothes()
    {
        Player player = GameManager.Instance.Player;
        PlayerItem item = player.GetItem(ItemType.CLOTHES);

        if (item != null)
        {
            UI.Instance.DisplayMessage("CLOTHED THE NAKED!");
            UpdateCharityPoints(ClothePoints);
        }
        else
        {
            UI.Instance.DisplayMessage("YOU HAVE NO CLOTHES TO GIVE!");
        }
    }

    private void OnDisable()
    {
        UI.DonatedClothes += GiveClothes;
        UI.Meditate -= Meditated;
    }
}
