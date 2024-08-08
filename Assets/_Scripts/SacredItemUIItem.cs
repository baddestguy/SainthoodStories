using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SacredItemUIItem : MonoBehaviour
{
    public TextMeshProUGUI Title;
    public Image Icon;

    public void Init(string itemName)
    {
        var item = GameDataManager.Instance.GetCollectibleData(itemName);
        Title.text = item.Name;
        Icon.sprite = Resources.Load<Sprite>($"Icons/{item.Id}");
    }

    public void OnSelect()
    {
        SendMessageUpwards("ShowArtifact", Title.text);
    }

    public void Remove()
    {
        Destroy(gameObject);
    }
}
