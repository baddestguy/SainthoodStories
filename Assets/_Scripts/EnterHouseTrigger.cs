using UnityEngine;

public class EnterHouseTrigger : MonoBehaviour
{
    public string HouseName;

    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("COLLIDED WITH:" + other.name);
        if (other.name == "Nun")
        {
            FindObjectOfType<WorldTextDisplay>().CrossFade(1f, 15);

            GameManager.Instance.CurrentBuilding = HouseName;
            SaveDataManager.Instance.SaveGame();
            GameManager.Instance.ReloadLevel();
        }
        
    }
}
