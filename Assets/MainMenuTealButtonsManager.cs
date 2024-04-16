using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuTealButtonsManager : MonoBehaviour
{
    public GameObject SettingsButtonGameObject;
    public GameObject SteamWishListGameObject;
    public GameObject DiscordGameObject;

    // Start is called before the first frame update
    void Start()
    {
        if (GameSettings.Instance.IsXboxMode) //Should this be replaced with demo check?
        {
            SteamWishListGameObject.SetActive(false);
            DiscordGameObject.SetActive(false);

            SettingsButtonGameObject.transform.localPosition = new Vector3(
                DiscordGameObject.transform.localPosition.x,
                DiscordGameObject.transform.localPosition.y);
        }    
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
