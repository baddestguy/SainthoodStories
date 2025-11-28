using UnityEngine;

public class SnowFx : MonoBehaviour
{
    RectTransform rt;
    RectTransform parentRT;

    float fallSpeed;
    float driftSpeed;
    float rotationSpeed;

    public void Init()
    {
        rt = GetComponent<RectTransform>();
        parentRT = rt.parent as RectTransform;

        // Randomize speed
        fallSpeed = Random.Range(20f, 60f);       // slower than rain
        driftSpeed = Random.Range(-20f, 20f);     // side-to-side drift
        rotationSpeed = Random.Range(-30f, 30f);  // slow spin
        var size = Random.Range(0.2f, 1f);
        transform.localScale = new Vector3(size, size, size);
    }

    void Update()
    {
        // Vertical fall
        rt.anchoredPosition -= new Vector2(0, fallSpeed * Time.deltaTime);

        // Horizontal drift
        rt.anchoredPosition += new Vector2(driftSpeed * Time.deltaTime, 0);

        // Soft rotation
        rt.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        // Destroy when fully below screen
        if (rt.anchoredPosition.y < -parentRT.rect.height - 50)
            Destroy(gameObject);
    }
}
