using Assets._Scripts.Xbox;

public class SacredItemCollectible : GridCollectibleItem
{
    public string SacredName;
    public string SacredDescription;

    public override void Collect()
    {
        MissionManager.Instance.Collect(gameObject.name + ":" + SacredName);
        //Trigger fancy VFX+UI
        DeleteCollectible();
        SoundManager.Instance.PlayOneShotSfx("Success_SFX", 1f, 5f);
        SteamManager.Instance.UnlockAchievement("SEEKER");
        XboxUserHandler.Instance.UnlockAchievement("13");
    }
}
