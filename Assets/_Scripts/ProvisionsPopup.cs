using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProvisionsPopup : MonoBehaviour
{
    public TextMeshProUGUI TitleText;
    private ProvisionsPopupPhase PopupPhase;
    private ProvisionData ProvisionToReplace;
    public GameObject XButton;
    public GameObject ProvisionItemResource;
    public ScrollRect NewProvisionScroller;
    public ScrollRect UpgradeProvisionScroller;

    public void Init(ProvisionData prov1, ProvisionData prov2)
    {
        StartCoroutine(WaitThenHideUI());

        SwitchPhase(ProvisionsPopupPhase.ADD_UPGRADE);
        CustomEventPopup.IsDisplaying = true;

        AddProvisionUIItem(prov1, NewProvisionScroller, ProvisionUIItemType.NEW);
        AddProvisionUIItem(prov2, NewProvisionScroller, ProvisionUIItemType.NEW);

        //Display Upgrade Provisions
        foreach (var provision in InventoryManager.Instance.Provisions)
        {
            var provisionToUpgrade = GameDataManager.Instance.GetProvision(provision.Id, provision.Level+1);
            if (provisionToUpgrade == null) continue;

            AddProvisionUIItem(provisionToUpgrade, UpgradeProvisionScroller, ProvisionUIItemType.UPGRADE);
        }
    }

    public void AddProvisionUIItem(ProvisionData prov, ScrollRect scroller, ProvisionUIItemType type)
    {
        GameObject provisionItemGO = Instantiate(ProvisionItemResource);
        provisionItemGO.transform.SetParent(scroller.content, false);
        provisionItemGO.GetComponent<ProvisionUIItem>().Init(prov, type);
    }

    private IEnumerator WaitThenHideUI()
    {
        yield return null;
        UI.Instance.EnableAllUIElements(false);
    }

    public void AddNewProvision(ProvisionData prov)
    {
        if (PopupPhase == ProvisionsPopupPhase.ADD_UPGRADE && InventoryManager.Instance.Provisions.Count == InventoryManager.Instance.MaxProvisionsSlots)
        {
            ProvisionToReplace = prov;
            SwitchPhase(ProvisionsPopupPhase.REPLACE);
            return;
        }
        else if (PopupPhase == ProvisionsPopupPhase.REPLACE)
        {
            InventoryManager.Instance.RemoveProvision(prov.Id);
            InventoryManager.Instance.AddProvision(ProvisionToReplace);

            CloseUI();
            return;
        }

        InventoryManager.Instance.AddProvision(prov);
        CloseUI();
    }

    public void UpgradeProvision(ProvisionData prov)
    {
        InventoryManager.Instance.UpgradeProvision(prov);

        CloseUI();
    }

    public void OnClick(string item)
    {
        if(item == "X")
        {
            SwitchPhase(ProvisionsPopupPhase.ADD_UPGRADE);
            return;
        }
    }

    private void SwitchPhase(ProvisionsPopupPhase phase)
    {
        PopupPhase = phase;
        switch (phase)
        {
            case ProvisionsPopupPhase.ADD_UPGRADE:
                TitleText.text = "CHOOSE ONE";
                XButton.SetActive(false);
                NewProvisionScroller.gameObject.SetActive(true);
                break;

            case ProvisionsPopupPhase.REPLACE:
                TitleText.text = "REPLACE EXISTING PROVISION?";
                XButton.SetActive(true);
                NewProvisionScroller.gameObject.SetActive(false);
                break;
        }
    }

    private void CloseUI()
    {
        CustomEventPopup.IsDisplaying = false;
        gameObject.SetActive(false);
        UI.Instance.EnableAllUIElements(true);
    }
}
