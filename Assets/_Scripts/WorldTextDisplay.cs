using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldTextDisplay : MonoBehaviour
{
    public Image Image;
    public TextMeshProUGUI DisplayCounter;

    public Image EnergyImage;
    public TextMeshProUGUI EnergyCounter;


    public bool CrossFading;
    public Image Black;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void DisplayEnergy(int energy)
    {
        EnergyCounter.DOKill();
        EnergyImage.DOKill();

        if (energy <= 0)
        {
            EnergyCounter.color = Color.red;
            EnergyImage.color = Color.red;
            SoundManager.Instance.PlayOneShotSfx("LowEnergy_SFX");
        }
        else
        {
            EnergyCounter.color = Color.white;
            EnergyImage.color = Color.white;
        }

        EnergyCounter.text = energy + "";

        EnergyCounter.DOFade(0, 5);
        EnergyImage.DOFade(0, 5);

    }

    public void Display(SacredItem sacredItem, int amount)
    {
        StartCoroutine(DisplayAsync(sacredItem, amount));
    }

    private IEnumerator DisplayAsync(SacredItem sacredItem, int amount)
    {
        var behaviour = sacredItem.Behaviour;

        DisplayCounter.DOKill();
        Image.DOKill();

        if(amount < 0)
        {
            DisplayCounter.color = Color.red;
        }
        else
        {
            DisplayCounter.color = Color.white;
        }

        Image.color = new Color(Image.color.r, Image.color.g, Image.color.b, 1);

        DisplayCounter.text = amount+"";

        if(behaviour == SacredItemBehaviour.CHASE)
        {
                Image.sprite = Resources.Load<Sprite>($"Icons/InteractableChurch");
        }
        else if(behaviour == SacredItemBehaviour.WANDER)
        {
                Image.sprite = Resources.Load<Sprite>($"Icons/CPHappy");
        }
        else
        {
            var item = sacredItem.GetComponent<CollectibleItem>();
            DisplayCounter.text = item.Name + "\n" + item.Description;
            Image.sprite = Resources.Load<Sprite>($"Icons/CPHappy");

            yield return new WaitForSeconds(10f);
        }

        DisplayCounter.DOFade(0, 5);
        Image.DOFade(0, 5);
    }

    public void CrossFade(float fade, float speed = 5f)
    {
        StartCoroutine(CrossFadeAsync(fade, speed));
    }

    private IEnumerator CrossFadeAsync(float fade, float speed)
    {
        CrossFading = true;
        Black.gameObject.SetActive(true);
        Color c = Black.color;

        while (Math.Abs(c.a - fade) > 0.01f)
        {
            c.a = Mathf.Lerp(c.a, fade, Time.deltaTime * speed);
            Black.color = c;
            yield return null;
        }

        c.a = fade;
        Black.color = c;
        Black.gameObject.SetActive(fade != 0);
        CrossFading = false;
    }

}
