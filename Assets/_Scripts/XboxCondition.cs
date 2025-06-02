using UnityEngine;

[CreateAssetMenu(menuName = "UIConditions/XBOX Condition")]
public class XboxCondition : ConditionSO
{
    public override bool IsTrue()
    {
#if MICROSOFT_GDK_SUPPORT
        return true;
#endif
        return false;
    }
}
