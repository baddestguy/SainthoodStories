using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProvisionsPopup : MonoBehaviour
{
    private ProvisionData Provision1;
    public TextMeshProUGUI ProvisionName1;
    public Image ProvisionIcon1;
    public TextMeshProUGUI ProvisionDescription1;

    private ProvisionData Provision2;
    public TextMeshProUGUI ProvisionName2;
    public Image ProvisionIcon2;
    public TextMeshProUGUI ProvisionDescription2;

    private ProvisionData ProvisionToUpgrade1;
    public TextMeshProUGUI ProvisionName3;
    public Image ProvisionIcon3;
    public TextMeshProUGUI ProvisionDescription3;

    private ProvisionData ProvisionToUpgrade2;
    public TextMeshProUGUI ProvisionName4;
    public Image ProvisionIcon4;
    public TextMeshProUGUI ProvisionDescription4;

    public void Init(ProvisionData prov1, ProvisionData prov2)
    {
        CustomEventPopup.IsDisplaying = true;
        Provision1 = prov1;
        ProvisionName1.text = LocalizationManager.Instance.GetText(prov1.NameKey);
        ProvisionDescription1.text = LocalizationManager.Instance.GetText(prov1.DescriptionKey);
        ProvisionIcon1.sprite = Resources.Load<Sprite>($"Icons/{prov1.Id}");

        Provision2 = prov2;
        ProvisionName2.text = LocalizationManager.Instance.GetText(prov2.NameKey);
        ProvisionDescription2.text = LocalizationManager.Instance.GetText(prov2.DescriptionKey);
        ProvisionIcon2.sprite = Resources.Load<Sprite>($"Icons/{prov2.Id}");

        if(InventoryManager.Instance.Provisions.Count > 0)
            ProvisionToUpgrade1 = GameDataManager.Instance.GetProvision(InventoryManager.Instance.Provisions[0].Id);
        if(InventoryManager.Instance.Provisions.Count > 1)
            ProvisionToUpgrade2 = GameDataManager.Instance.GetProvision(InventoryManager.Instance.Provisions[1].Id);

        //Display Upgrade Provisions

    }

    public void OnClick(string item)
    {
        switch (item)
        {
            case "ITEM1":
                InventoryManager.Instance.AddProvision(Provision1);
                break;

            case "ITEM2":
                InventoryManager.Instance.AddProvision(Provision2);
                break;

            case "UPGRADE1":
                if(ProvisionToUpgrade1 != null)
                    InventoryManager.Instance.UpgradeProvision(ProvisionToUpgrade1);
                break;

            case "UPGRADE2":
                if(ProvisionToUpgrade2 != null)
                    InventoryManager.Instance.UpgradeProvision(ProvisionToUpgrade2);
                break;
        }
        CustomEventPopup.IsDisplaying = false;
        gameObject.SetActive(false);
    }
}
