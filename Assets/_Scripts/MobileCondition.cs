using UnityEngine;

[CreateAssetMenu(menuName = "UIConditions/Mobile Condition")]
public class MobileCondition : ConditionSO
{
    public override bool IsTrue()
    {
#if PLATFORM_MOBILE
        return true;
#endif
        return false;
    }
}
