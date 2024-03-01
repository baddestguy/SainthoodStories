using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    public string Name;
    public string Description;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.name == "Nun")
        {
            MissionManager.Instance.Collect(gameObject.name + ":" + Name);
            gameObject.SetActive(false);
        }
    }
}
