using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    public TextMeshProUGUI TextMesh;
    private float _deltaTime = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        TextMesh.gameObject.SetActive(GameSettings.Instance.ShowFPSCounter);
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameSettings.Instance.ShowFPSCounter) return;

        _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
        var fps = 1.0f / _deltaTime;
        TextMesh.text = $"{fps:0.} fps";
    }
}
