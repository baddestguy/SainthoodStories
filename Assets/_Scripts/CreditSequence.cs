using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreditSequence : MonoBehaviour
{

    public TextMeshProUGUI Title;
    public TextMeshProUGUI Creator;
    public TextMeshProUGUI Creator2;
    public Image Logo;
    public Image Logo2;
    public TextMeshProUGUI Names;
    public TextMeshProUGUI Names2;
    public TextMeshProUGUI Names3;
    public TextMeshProUGUI Names4;
    public TextMeshProUGUI Song;

    public List<KSBackers> NameList;
    void Start()
    {
        LoadNames();
        StartCoroutine(Sequence());
    }

    IEnumerator Sequence()
    {
        yield return new WaitForSeconds(1f);

        Title.DOFade(1f, 1f);
        yield return new WaitForSeconds(3f);
        Title.DOFade(0f, 1f);
        yield return new WaitForSeconds(1f);

        Creator.DOFade(1f, 1f);
        Creator2.DOFade(1f, 1f);
        yield return new WaitForSeconds(3f);
        Creator.DOFade(0f, 1f);
        Creator2.DOFade(0f, 1f);
        yield return new WaitForSeconds(1f);

        Logo.DOFade(1f, 1f);
        Logo2.DOFade(1f, 1f);
        yield return new WaitForSeconds(3f);
        Logo.DOFade(0f, 1f);
        Logo2.DOFade(0f, 1f);
        yield return new WaitForSeconds(1f);

        PopulateNameText(Names2, 0, 15);
        Names.DOFade(1f, 1f);
        yield return new WaitForSeconds(2f);
        Names2.DOFade(1f, 1f);
        yield return new WaitForSeconds(3f);

        PopulateNameText(Names3, 15, 30);
        Names3.DOFade(1f, 1f);
        yield return new WaitForSeconds(3f);

        PopulateNameText(Names4, 30, 41);
        Names4.DOFade(1f, 1f);
        Song.DOFade(1f, 1f);
        yield return new WaitForSeconds(3f);

    }

    public void LoadNames()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("GameData/KSBackers");
        var x = CSVSerializer.Deserialize<KSBackers>(csvFile.text);
        NameList = x.ToList();
    }

    public void PopulateNameText(TextMeshProUGUI textDisplay, int start, int end)
    {
        for(int i = start; i < end; i++)
        {
            if (i >= NameList.Count) return;
            textDisplay.text += $"{NameList[i].Name}\n";
        }
    }
}
