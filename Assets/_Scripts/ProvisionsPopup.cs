using System.Collections;
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

    public GameObject ProvisionUpgrade1Obj;
    private ProvisionData ProvisionToUpgrade1;
    public TextMeshProUGUI ProvisionName3;
    public Image ProvisionIcon3;
    public TextMeshProUGUI ProvisionDescription3;

    public GameObject ProvisionUpgrade2Obj;
    private ProvisionData ProvisionToUpgrade2;
    public TextMeshProUGUI ProvisionName4;
    public Image ProvisionIcon4;
    public TextMeshProUGUI ProvisionDescription4;

    public void Init(ProvisionData prov1, ProvisionData prov2)
    {
        StartCoroutine(WaitThenHideUI());
        CustomEventPopup.IsDisplaying = true;
        Provision1 = prov1;
        ProvisionName1.text = LocalizationManager.Instance.GetText(prov1.NameKey);
        ProvisionDescription1.text = LocalizationManager.Instance.GetText(prov1.DescriptionKey);
        ProvisionIcon1.sprite = Resources.Load<Sprite>($"Icons/{prov1.Id}");

        Provision2 = prov2;
        ProvisionName2.text = LocalizationManager.Instance.GetText(prov2.NameKey);
        ProvisionDescription2.text = LocalizationManager.Instance.GetText(prov2.DescriptionKey);
        ProvisionIcon2.sprite = Resources.Load<Sprite>($"Icons/{prov2.Id}");

        var provisionsCount = InventoryManager.Instance.Provisions.Count;
        if (provisionsCount > 0)
        {
            var p = InventoryManager.Instance.Provisions[Random.Range(0, provisionsCount)];
            ProvisionToUpgrade1 = GameDataManager.Instance.GetProvision(p.Id, p.Level+1);
        }
        if(provisionsCount > 1)
        {
            var p = InventoryManager.Instance.Provisions[Random.Range(0, provisionsCount)];
            while(p.Id == ProvisionToUpgrade1.Id) p = InventoryManager.Instance.Provisions[Random.Range(0, provisionsCount)];
            ProvisionToUpgrade2 = GameDataManager.Instance.GetProvision(p.Id, p.Level+1);
        }

        //Display Upgrade Provisions
        if(ProvisionToUpgrade1 != null)
        {
            ProvisionUpgrade1Obj.SetActive(true);
            ProvisionName3.text = LocalizationManager.Instance.GetText(ProvisionToUpgrade1.NameKey);
            ProvisionDescription3.text = LocalizationManager.Instance.GetText(ProvisionToUpgrade1.DescriptionKey);
            ProvisionIcon3.sprite = Resources.Load<Sprite>($"Icons/{ProvisionToUpgrade1.Id}");
        }

        if(ProvisionToUpgrade2 != null)
        {
            ProvisionUpgrade2Obj.SetActive(true);
            ProvisionName4.text = LocalizationManager.Instance.GetText(ProvisionToUpgrade2.NameKey);
            ProvisionDescription4.text = LocalizationManager.Instance.GetText(ProvisionToUpgrade2.DescriptionKey);
            ProvisionIcon4.sprite = Resources.Load<Sprite>($"Icons/{ProvisionToUpgrade2.Id}");
        }
    }

    private IEnumerator WaitThenHideUI()
    {
        yield return null;
        UI.Instance.EnableAllUIElements(false);
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
        UI.Instance.EnableAllUIElements(true);
    }
}
