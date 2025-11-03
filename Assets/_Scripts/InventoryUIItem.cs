using UnityEngine;

public class InventoryUIItem : MonoBehaviour
{
    public GameObject MyPrefab;

    public void OnClick()
    {
        var go = Instantiate(MyPrefab, RosaryMakerInventoryUI.Instance.transform);
        go.transform.position = Input.mousePosition;
        RosaryMakerInventoryUI.Instance.OnSelected(true);
    }

}
