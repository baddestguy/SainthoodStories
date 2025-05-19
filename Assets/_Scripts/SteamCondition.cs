using UnityEngine;

[CreateAssetMenu(menuName = "UIConditions/Steam Condition")]
public class SteamCondition : ConditionSO
{
    public override bool IsTrue()
    {
#if STEAM_API
        return true;
#endif
        return false;
    }
}
