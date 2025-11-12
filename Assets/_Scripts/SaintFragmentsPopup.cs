using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaintFragmentsPopup : MonoBehaviour
{
    public Dictionary<SaintID, List<SaintFragmentData>> Data;
    public Image CharPotrait;
    public TextMeshProUGUI Fragment;
    public TextMeshProUGUI SaintName;
    public ScrollRect ScrollRect;

    public void Open()
    {
        CustomEventPopup.IsDisplaying = true;
        UI.Instance.EnableAllUIElements(false);
        gameObject.SetActive(true);

        //Load/Refresh data
        Data = InventoryManager.Instance.SaintFragments;
        //SaintName.text = LocalizationManager.Instance.GetText(Data.NameKey);
        //CharPotrait.sprite = Resources.Load<Sprite>(Data.IconPath);
        //foreach(var d in data)
        //{
        //    Fragment.text += LocalizationManager.Instance.GetText(d.DescriptionKey) + "\n\n";
        //}
    }

    public void Close()
    {
        CustomEventPopup.IsDisplaying = false;
        UI.Instance.EnableAllUIElements(true);
        gameObject.SetActive(false);
    }
}
