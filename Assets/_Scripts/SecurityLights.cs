using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecurityLights : MonoBehaviour
{
    public Light Light;

    private void OnEnable()
    {
        GameClock.Ticked += CheckSecurityLights;
    }

    public void CheckSecurityLights(double time, int day)
    {
        if(time >= 21 || time < 6)
        {
            Light.enabled = true;
        }
        else
        {
            Light.enabled = false;
        }
    }

    private void OnDisable()
    {
        GameClock.Ticked -= CheckSecurityLights;
    }
}
