using System.Collections;
using UnityEngine;

public class SecurityLights : MonoBehaviour
{
    public GameObject Light;

    private void OnEnable()
    {
        GameClock.Ticked += CheckSecurityLights;
    }

    public void CheckSecurityLights(double time, int day)
    {
        if(time >= 19 || time < 6)
        {
            StartCoroutine(LightSwitch(true));
        }
        else
        {
            StartCoroutine(LightSwitch(false));
        }
    }

    IEnumerator LightSwitch(bool enable)
    {
        yield return new WaitForSeconds(Random.Range(0, 3f));
        Light.SetActive(enable);
    }

    private void OnDisable()
    {
        GameClock.Ticked -= CheckSecurityLights;
    }
}
