using System.Linq;
using CompassNavigatorPro;
using Opsive.UltimateCharacterController.Character;
using UnityEngine;

public class BuildingStateToggle : MonoBehaviour
{
    public string HouseName;
    public GameObject RubbleGO;
    public GameObject BuildingGO;
    public GameObject FireGO;
    public Transform PlayerAnchorPoint;

    public ObjectivesData MyObjective;

    // Start is called before the first frame update
    void Start()
    {
        var myState = GameManager.Instance.HouseStates[HouseName];
        var objectives = MissionManager.Instance.CurrentObjectives.Where(obj => obj.House == HouseName || (obj.House == "Any" && HouseName != "InteractableChurch"));
        if (objectives.Count() > 1)
        {
            MyObjective = objectives.Where(obj => obj.House == HouseName).FirstOrDefault();
        }
        else
        {
            MyObjective = objectives.FirstOrDefault();
        }
        gameObject.SetActive(myState != BuildingState.RUBBLE || (myState == BuildingState.RUBBLE && (MyObjective?.Event == BuildingEventType.CONSTRUCT || MyObjective?.Event == BuildingEventType.CONSTRUCT)));

        if (GameManager.Instance.HouseStates.ContainsKey(HouseName))
        {
            RubbleGO.SetActive(myState == BuildingState.RUBBLE);
            BuildingGO.SetActive(myState != BuildingState.RUBBLE);
            if(FireGO != null)
            {
                FireGO.SetActive(myState == BuildingState.HAZARDOUS);
            }
        }
        
        if(GameManager.Instance.CurrentBuilding != null && GameManager.Instance.CurrentBuilding == HouseName)
        {
            var player = FindObjectOfType<UltimateCharacterLocomotion>();
            player.SetPosition(PlayerAnchorPoint.position);
        }

        if(MyObjective != null)
        {
            var myPoi = gameObject.AddComponent<CompassProPOI>();
            myPoi.visibility = POIVisibility.AlwaysVisible;
            myPoi.iconNonVisited = Resources.Load<Sprite>($"Icons/{HouseName}");
        }
    }
}
