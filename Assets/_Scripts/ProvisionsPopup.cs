using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProvisionsPopup : MonoBehaviour
{
    private Provision Provision1;
    public TextMeshProUGUI ProvisionName1;
    public Image ProvisionIcon1;
    public TextMeshProUGUI ProvisionDescription1;

    private Provision Provision2;
    public TextMeshProUGUI ProvisionName2;
    public Image ProvisionIcon2;
    public TextMeshProUGUI ProvisionDescription2;

    public void Init(ProvisionData prov1, ProvisionData prov2)
    {
        Provision1 = prov1.Id;
        ProvisionName1.text = LocalizationManager.Instance.GetText(prov1.NameKey);
        ProvisionDescription1.text = LocalizationManager.Instance.GetText(prov1.DescriptionKey);
        ProvisionIcon1.sprite = Resources.Load<Sprite>($"Icons/{prov1.Id}");

        Provision2 = prov2.Id;
        ProvisionName2.text = LocalizationManager.Instance.GetText(prov2.NameKey);
        ProvisionDescription2.text = LocalizationManager.Instance.GetText(prov2.DescriptionKey);
        ProvisionIcon2.sprite = Resources.Load<Sprite>($"Icons/{prov2.Id}");
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
        }

        gameObject.SetActive(false);
    }
}
