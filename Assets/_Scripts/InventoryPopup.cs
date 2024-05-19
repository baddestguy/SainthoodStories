using UnityEngine;

public class InventoryPopup : MonoBehaviour
{
    public PackageItem[] ItemList;
    public ProvisionUIItem[] UpgradeProvisionUIItems;


    // Start is called before the first frame update
    void OnEnable()
    {
        Player.LockMovement = true;
        for (int i = 0; i < InventoryManager.Instance.Items.Count; i++)
        {
            ItemList[i].PackageIcon.gameObject.SetActive(true);
            ItemList[i].PackageIcon.sprite = Resources.Load<Sprite>($"Icons/{InventoryManager.Instance.Items[i]}");
        }

        for (int i = 0; i < InventoryManager.Instance.Provisions.Count; i++)
        {
            UpgradeProvisionUIItems[i].Init(InventoryManager.Instance.Provisions[i], ProvisionUIItemType.UPGRADE);
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < InventoryManager.Instance.Items.Count; i++)
        {
            ItemList[i].PackageIcon.gameObject.SetActive(false);
        }
        Player.LockMovement = false;
    }
}
