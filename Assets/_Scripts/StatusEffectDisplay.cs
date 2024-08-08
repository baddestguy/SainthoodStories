using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusEffectDisplay : MonoBehaviour
{
    public ScrollRect Scroller;
    public TextMeshProUGUI StatusEffectDescription;
    private List<GameObject> InstantiatedItems = new List<GameObject>();

    private void OnEnable()
    {
        Player.StatusEffectTrigger += Init;
        Init();
    }

    public void Init()
    {
        foreach(var go in InstantiatedItems)
        {
            Destroy(go);
        }
        InstantiatedItems.Clear();

        var statusFx = GameManager.Instance.Player.StatusEffects;

        StatusEffectDescription.text = "Status ailment descriptions will show up here";

        var statusEffectDisplayItemResource = Resources.Load<GameObject>("UI/StatusEffectItem");
        foreach(var fx in statusFx)
        {
            GameObject itemGO = Instantiate(statusEffectDisplayItemResource);
            itemGO.transform.SetParent(Scroller.content, false);
            itemGO.GetComponent<StatusEffectDisplayItem>().Init(fx);
            InstantiatedItems.Add(itemGO);
        }
    }

    public void OnClose()
    {
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        Player.StatusEffectTrigger -= Init;
    }
}
