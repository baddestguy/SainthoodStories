using UnityEngine;

public class SnowSpawner : MonoBehaviour
{
    public RectTransform spawnArea;
    public GameObject SnowFxPrefab;
    public int spawnRate = 20;
    public float fallSpeed = 1200f;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime * spawnRate;
        while (timer >= 1f)
        {
            SpawnDrop();
            timer -= 1f;
        }
    }

    void SpawnDrop()
    {
        float x = Random.Range(0, spawnArea.rect.width);

        GameObject drop = Instantiate(SnowFxPrefab, spawnArea);
        RectTransform rt = drop.GetComponent<RectTransform>();

        rt.anchoredPosition = new Vector2(x, spawnArea.rect.height + 20);

        drop.GetComponent<SnowFx>().Init();
    }
}
