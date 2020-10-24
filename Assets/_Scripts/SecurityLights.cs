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
        if(time >= 21 || time < 6)
        {
            Light.SetActive(true);
        }
        else
        {
            Light.SetActive(false);
        }
    }

    private void OnDisable()
    {
        GameClock.Ticked -= CheckSecurityLights;
    }
}
