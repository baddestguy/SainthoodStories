public class SacredItemCollectible : GridCollectibleItem
{
    public string SacredName;
    public string SacredDescription;

    public override void Collect()
    {
        MissionManager.Instance.Collect(gameObject.name + ":" + SacredName);
        Delete();
    }
}
