using System.Linq;
using Opsive.UltimateCharacterController.Character;
using UnityEngine;

public class BuildingStateToggle : MonoBehaviour
{
    public string HouseName;
    public GameObject RubbleGO;
    public GameObject BuildingGO;
    public GameObject FireGO;

    public Transform PlayerAnchorPoint;
    // Start is called before the first frame update
    void Start()
    {
        var myState = GameManager.Instance.HouseStates[HouseName];
        var myObjective = MissionManager.Instance.CurrentObjectives.Where(obj => obj.House == HouseName).FirstOrDefault();
        gameObject.SetActive(myState == BuildingState.NORMAL || (myState == BuildingState.RUBBLE && myObjective?.Event == BuildingEventType.CONSTRUCT));

        if (GameManager.Instance.HouseStates.ContainsKey(HouseName))
        {
            RubbleGO.SetActive(myState == BuildingState.RUBBLE);
            BuildingGO.SetActive(myState == BuildingState.NORMAL);
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
    }

}
