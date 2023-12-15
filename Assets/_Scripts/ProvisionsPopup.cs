using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProvisionsPopup : MonoBehaviour
{
    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI Title2Text;
    private ProvisionsPopupPhase PopupPhase;
    private ProvisionData ProvisionToReplace;
    public GameObject XButton;
    public GameObject ProvisionItemResource;
    public ScrollRect NewProvisionScroller;
    public ScrollRect UpgradeProvisionScroller;
    public ProvisionUIItem[] NewProvisionUIItems = new ProvisionUIItem[2];
    public ProvisionUIItem[] UpgradeProvisionUIItems = new ProvisionUIItem[5];

    public void Init(ProvisionData prov1, ProvisionData prov2)
    {
        StartCoroutine(WaitThenHideUI());

        SwitchPhase(ProvisionsPopupPhase.ADD_UPGRADE);
        CustomEventPopup.IsDisplaying = true;

        NewProvisionUIItems[0].Init(prov1, ProvisionUIItemType.NEW);
        NewProvisionUIItems[1].Init(prov2, ProvisionUIItemType.NEW);

        //Display Upgrade Provisions
        for(int i = 0; i < InventoryManager.Instance.Provisions.Count; i++)
        {
            UpgradeProvisionUIItems[i].Init(InventoryManager.Instance.Provisions[i], ProvisionUIItemType.UPGRADE);
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

        SoundManager.Instance.PlayOneShotSfx("Provisions_Collect", timeToDie: 5f);
        SoundManager.Instance.PlayOneShotSfx("Success_SFX", 1f, 5f);

        InventoryManager.Instance.AddProvision(prov);
        CloseUI();
    }

    public void UpgradeProvision(ProvisionData prov)
    {
        if (PopupPhase == ProvisionsPopupPhase.REPLACE)
        {
            InventoryManager.Instance.RemoveProvision(prov.Id);
            InventoryManager.Instance.AddProvision(ProvisionToReplace);

            CloseUI();
            return;
        }

        ProvisionData nextLevel = GameDataManager.Instance.GetProvision(prov.Id, prov.Level + 1);
        if (nextLevel == null) return;

        SoundManager.Instance.PlayOneShotSfx("Provisions_Collect", timeToDie: 5f);
        SoundManager.Instance.PlayOneShotSfx("Success_SFX", 1f, 5f);
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
                Title2Text.text = "---------------- OR UPGRADE ----------------";
                XButton.SetActive(false);
                foreach (var provItem in NewProvisionUIItems)
                {
                    provItem.gameObject.SetActive(true);
                }
                break;

            case ProvisionsPopupPhase.REPLACE:
                TitleText.text = "";
                Title2Text.text = "-- REPLACE EXISTING PROVISION? --";
                XButton.SetActive(true);
                foreach(var provItem in NewProvisionUIItems)
                {
                    provItem.gameObject.SetActive(false);
                }
                break;
        }
    }

    private void CloseUI()
    {
        var bonusEnergy = InventoryManager.Instance.GetProvision(Provision.ENERGY_DRINK);
        Player player = GameManager.Instance.Player;
        player.ConsumeEnergy(-bonusEnergy?.Value ?? 0);

        CustomEventPopup.IsDisplaying = false;
        gameObject.SetActive(false);
        UI.Instance.EnableAllUIElements(true);
        GamepadCursor.CursorSpeed = 2000f;

    }
}
