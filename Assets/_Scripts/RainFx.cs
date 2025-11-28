using UnityEngine;

public class RainFx : MonoBehaviour
{
    RectTransform rt;
    RectTransform parentRT;
    float speed;

    public void Init(float fallSpeed)
    {
        rt = GetComponent<RectTransform>();
        parentRT = rt.parent as RectTransform;
        speed = fallSpeed;
    }

    void Update()
    {
        if (rt == null) return;
        rt.anchoredPosition -= new Vector2(0, speed * Time.deltaTime);

        if (rt.anchoredPosition.y < -parentRT.rect.height)
            Destroy(gameObject);
    }
}
