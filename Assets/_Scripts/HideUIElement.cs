using System.Collections.Generic;
using UnityEngine;

public class HideUIElement : MonoBehaviour
{
    public List<ConditionSO> ConditionsList;

    void Start()
    {
        bool allTrue = true;
        foreach (var condition in ConditionsList)
        {
            if (condition == null || !condition.IsTrue())
            {
                allTrue = false;
                break;
            }
        }

        gameObject.SetActive(!allTrue);
    }
}
