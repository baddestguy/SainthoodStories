using UnityEngine;

public abstract class ConditionSO : ScriptableObject, ICondition
{
    public abstract bool IsTrue();
}
