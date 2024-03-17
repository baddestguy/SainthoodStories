using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinigamePlayer : MonoBehaviour
{
    public MinigameData Minigame;
    public Image GameImage;
    private Sprite[] Images;
    private int SequenceCounter;
    private Action<string> Callback;
    public GameObject OKGO;

    public void Init(MinigameType type, Action<string> callback)
    {
        Callback = callback;
        Minigame = GameDataManager.Instance.MinigameData[type];
        Images = Resources.LoadAll<Sprite>($"Minigames/{Minigame.IconPath}");
        OKGO.SetActive(false);

        GameImage.sprite = Images[0];
        SequenceCounter = 0;
    }

    public void OnClick()
    {
        SequenceCounter++;

        if (SequenceCounter >= Minigame.Sequences)
        {
            OKGO.SetActive(true);
            return;
        }

        GameImage.sprite = Images[SequenceCounter];
    }

    public void OK()
    {
        Callback?.Invoke(Minigame.Id.ToString());
        gameObject.SetActive(false);
    }

    public void OnCancel()
    {
        if (SequenceCounter >= Minigame.Sequences)
        {
            Callback?.Invoke(Minigame.Id.ToString());
        }

        gameObject.SetActive(false);
    }
}
