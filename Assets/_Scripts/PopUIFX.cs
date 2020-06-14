using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopUIFX : MonoBehaviour
{
    public TextMeshProUGUI OperatorDisplay;
    public Image Icon;
    public TextMeshProUGUI ValueDisplay;

    private Transform CamTransform;

    private void Start()
    {
        CamTransform = Camera.main.transform;
    }

    public void Init(string sprite, int value, float speed = 0.5f)
    {
        Icon.sprite = Resources.Load<Sprite>($"Icons/{sprite}");
        OperatorDisplay.text = value >= 0 ? "+" : "-";
        ValueDisplay.text = $"{Mathf.Abs(value)}";

        if(value > 0)
        {
            ValueDisplay.color = Color.green;
            OperatorDisplay.color = Color.green;
            Icon.color = Color.green;
        }
        else if (value < 0)
        {
            ValueDisplay.color = Color.red;
            OperatorDisplay.color = Color.red;
            Icon.color = Color.red;
        }
        else
        {
            ValueDisplay.color = Color.white;
            OperatorDisplay.color = Color.white;
            Icon.color = Color.white;
        }

        StopCoroutine("FadeOut");
        StartCoroutine("FadeOut", speed);
    }
    
    IEnumerator FadeOut(float speed)
    {
        Color color = ValueDisplay.color;
        color.a = 1;
        ValueDisplay.color = color;
        OperatorDisplay.color = color;
        Icon.color = color;

        yield return new WaitForSeconds(speed);
        var originalPosition = transform.position;
        while (color.a - 0 > 0.01f)
        {
            color.a = Mathf.Lerp(color.a, 0, Time.deltaTime*2);

            ValueDisplay.color = color;
            OperatorDisplay.color = color;
            Icon.color = color;
            transform.position += new Vector3(0, 0.01f, 0);
            yield return null;
        }

        transform.position = originalPosition;
        color.a = 1f;

        ValueDisplay.color = color;
        OperatorDisplay.color = color;
        Icon.color = color;
        gameObject.SetActive(false);
    }

    void Update()
    {
        transform.forward = CamTransform.forward;
    }
}
