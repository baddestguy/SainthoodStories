using UnityEngine;
using System;
using System.Collections.Generic;

public class ActsApostleMover : MonoBehaviour
{
    public float Speed = 1f;
    private List<Vector3> pathPoints;
    private int currentIndex = 0;
    private Action onArrive;

    public void Initialize(List<Vector3> path, Action arriveCallback)
    {
        pathPoints = path;
        onArrive = arriveCallback;
        transform.position = pathPoints[0];

        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visual.transform.SetParent(this.transform, false);
        visual.transform.localScale = Vector3.one * 0.3f;
        visual.GetComponent<Renderer>().material.color = Color.yellow;
    }

    void Update()
    {
        if (currentIndex >= pathPoints.Count - 1) return;

        Vector3 start = pathPoints[currentIndex];
        Vector3 end = pathPoints[currentIndex + 1];

        float step = Speed * ActsTimeKeeper.Instance.DeltaTime;
        transform.position = Vector3.MoveTowards(transform.position, end, step);

        if (Vector3.Distance(transform.position, end) < 0.001f)
        {
            currentIndex++;
            if (currentIndex >= pathPoints.Count - 1)
            {
                onArrive?.Invoke();
                Destroy(gameObject);
            }
        }
    }
}
