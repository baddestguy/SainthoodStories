using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    public TextMeshProUGUI LoadingText;
    public Image LoadingBarFill;

    void OnEnable()
    {
        LoadingText.text = "LOADING";
        StartCoroutine(Loading());
    }

    IEnumerator Loading()
    {
        var count = 0;
        while (true)
        {
            if (count == 4)
            {
                LoadingText.text = "LOADING";
            }
            yield return new WaitForSeconds(0.5f);
            count++;
            LoadingText.text += ".";
        }
    }
}
