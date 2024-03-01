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

    // Start is called before the first frame update
    void Start()
    {
        
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
}
