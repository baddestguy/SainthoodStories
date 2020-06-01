using System;
using UnityEngine;

public class PopUI : MonoBehaviour
{
    private Transform CamTransform;
    public Action<string> Callback;

    void Start()
    {
        CamTransform = Camera.main.transform;
    }

    public void Init(Action<string> callback)
    {
        Callback = callback;
    }

    public void OnClick(string button)
    {
        Callback?.Invoke(button);
    }

    void Update()
    {
        transform.forward = CamTransform.forward;
    }
}
