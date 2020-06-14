using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopUI : MonoBehaviour
{
    private Transform CamTransform;
    public Action<string> Callback;
    private bool LockUI;

    public Image BuildingIcon;
    public Image ClockIcon;
    public TextMeshProUGUI ItemsRequiredDisplay;
    public TextMeshProUGUI DeadlineDisplay;

    void Start()
    {
        CamTransform = Camera.main.transform;
    }

    public void Init(Action<string> callback, string sprite, int items, GameClock deadline)
    {
        Callback = callback;

        BuildingIcon.sprite = Resources.Load<Sprite>($"Icons/{sprite}");
        ItemsRequiredDisplay.text = $"{items}";
        DeadlineDisplay.text = $"{(int)deadline.Time}:{(deadline.Time % 1 == 0 ? "00" : "30")}";

        if (items <= 0)
        {
            ItemsRequiredDisplay.gameObject.SetActive(false);
            ClockIcon.gameObject.SetActive(false);
            BuildingIcon.transform.localPosition = new Vector3(0, 1, 0);
        }
        else
        {
            ItemsRequiredDisplay.gameObject.SetActive(true);
            ClockIcon.gameObject.SetActive(true);
            BuildingIcon.transform.localPosition = new Vector3(0, 2, 0);
        }

        GameClock clock = GameManager.Instance.GameClock;
        if (clock.TimeDifference(deadline) <= 1.5)
        {
            DeadlineDisplay.color = Color.red;
            ClockIcon.color = Color.red;
        }
        else
        {
            DeadlineDisplay.color = Color.white;
            ClockIcon.color = Color.white;
        }
    }

    public void OnClick(string button)
    {
        if (LockUI) return;

        Callback?.Invoke(button);
        StartCoroutine(ActionPauseCycle());
    }

    private IEnumerator ActionPauseCycle()
    {
        LockUI = true;
        Player.LockMovement = true;
        var buttons = gameObject.GetComponentsInChildren<Button>();
        for(int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = false;
        }

        yield return new WaitForSeconds(1);
        
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = true;
        }
        
        LockUI = false;
        Player.LockMovement = false;
    }

    void Update()
    {
        transform.forward = CamTransform.forward;
    }
}
