using System.Collections.Generic;
using UnityEngine;

public class HideUIElement : MonoBehaviour
{
    public List<ConditionSO> ConditionsList;
    public Operator Operator;

    void Start()
    {
        bool allTrue = true;
        switch (Operator)
        {
            case Operator.AND:
                foreach (var condition in ConditionsList)
                {
                    if (condition == null || !condition.IsTrue())
                    {
                        allTrue = false;
                        break;
                    }
                }

                break;

            case Operator.OR:
                allTrue = false;
                foreach (var condition in ConditionsList)
                {
                    if (condition != null && condition.IsTrue())
                    {
                        allTrue = true;
                        break;
                    }
                }

                break;
        }

        gameObject.SetActive(!allTrue);
    }
}

public enum Operator
{
    AND,
    OR
}
