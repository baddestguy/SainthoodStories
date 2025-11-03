using UnityEngine;

public class RosaryMakerInventoryUI : MonoBehaviour
{
    public static RosaryMakerInventoryUI Instance;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnSelected(bool selected)
    {
        //if (selected)
        //{
        //    gameObject.SetActive(false);
        //}
        //else
        //{
        //    gameObject.SetActive(true);
        //}
    }
}
