using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public List<Image> Items;
    public List<Image> Provisions;

    public GameObject ExtraSlots;

    public void RefreshInventory()
    {
        Clear();
        Init();
    }

    private void Init()
    {
        InventoryManager.RefreshInventoryUI += RefreshInventory;
  
        var items = InventoryManager.Instance.Items;
        var provisions = InventoryManager.Instance.Provisions;

        for (int i = 0; i < items.Count; i++)
        {
            Items[i].gameObject.SetActive(true);
            Items[i].sprite = Resources.Load<Sprite>($"Icons/{items[i]}");
        }

        for (int i = 0; i < provisions.Count; i++)
        {
            Provisions[i].gameObject.SetActive(true);
            Provisions[i].sprite = Resources.Load<Sprite>($"Icons/{provisions[i]}");
        }

        ExtraSlots.SetActive(InventoryManager.Instance.MaxInventorySlots > 2);
    }

    private void Clear()
    {
        InventoryManager.RefreshInventoryUI -= RefreshInventory;
 
        for (int i = 0; i < Items.Count; i++)
        {
            Items[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < Provisions.Count; i++)
        {
            Provisions[i].gameObject.SetActive(false);
        }
    }

    public void OnEnable()
    {
        Init();
    }

    public void OnDisable()
    {
        Clear();
    }
}
