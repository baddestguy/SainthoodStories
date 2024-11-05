using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SacredItemPopup : MonoBehaviour
{
    public TextMeshProUGUI Title;
    public TextMeshProUGUI Description;
    public Image Icon;

    public static bool IsOpen;

    public void Init(string itemName)
    {
        transform.DOJump(transform.position, 30f, 1, 1f);
        var item = GameDataManager.Instance.GetCollectibleData(itemName.Split(':')[1]);
        Title.text = item.Name;
        Description.text = item.Description;
        Icon.sprite = Resources.Load<Sprite>($"Icons/{item.Id}");
        IsOpen = true;
    }

    public void Close()
    {
        gameObject.SetActive(false);
        UI.Instance.EnableAllUIElements(true);
        IsOpen = false;
    }
}
