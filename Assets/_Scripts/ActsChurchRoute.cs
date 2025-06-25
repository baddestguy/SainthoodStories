using UnityEngine;
using System.Collections.Generic;

public class ActsChurchRoute : MonoBehaviour
{
    public ActsChurchNode FromBase;
    public ActsChurchNode ToSatellite;
    public LineRenderer LineRenderer;

    private List<ActsApostleMover> inTransit = new List<ActsApostleMover>();
    private bool active = false;
    private Vector3[] routePath;

    public void Initialize(ActsChurchNode from, ActsChurchNode to)
    {
        FromBase = from;
        ToSatellite = to;

        LineRenderer = gameObject.AddComponent<LineRenderer>();
        LineRenderer.positionCount = 2;
        LineRenderer.startWidth = 0.1f;
        LineRenderer.endWidth = 0.1f;
        LineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        LineRenderer.startColor = Color.white;
        LineRenderer.endColor = Color.white;

        routePath = GetOrthogonalPath(FromBase.transform.position, ToSatellite.transform.position);
        UpdateLine();
        active = true;

        TrySendApostles(); // Begin sending first pair
    }

    void Update()
    {
        if (active && inTransit.Count == 0 && FromBase.CanSendTwoApostles())
        {
            TrySendApostles();
        }

        UpdateLine();
    }

    public void UpdateLine()
    {
        if (routePath == null) return;
        LineRenderer.positionCount = routePath.Length;
        LineRenderer.SetPositions(routePath);
    }

    public void TrySendApostles()
    {
        if (!FromBase.UseTwoApostles()) return;

        // Get orthogonal path
        List<Vector3> path = new List<Vector3>(routePath);

        Vector3 direction = (path[1] - path[0]).normalized;
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.forward).normalized; // 2D perpendicular

        float offsetAmount = 0.1f; // Adjust spacing between apostles

        for (int i = -1; i <= 1; i += 2) // i = -1, 1
        {
            // Offset starting position
            Vector3 startPos = path[0] + (perpendicular * offsetAmount * i);

            // Offset the entire path so they follow parallel tracks
            List<Vector3> offsetPath = new List<Vector3>();
            foreach (var point in path)
                offsetPath.Add(point + (perpendicular * offsetAmount * i));

            // Create ApostleMover
            GameObject apostleObj = new GameObject("ApostleMover");
            apostleObj.transform.SetParent(ActsChurchSpawner.Instance.transform);
            apostleObj.transform.position = startPos;

            ActsApostleMover mover = apostleObj.AddComponent<ActsApostleMover>();
            mover.Initialize(offsetPath, () =>
            {
                ToSatellite.ReceiveApostles(1, this);
                inTransit.Remove(mover);
            });

            inTransit.Add(mover); // Don’t forget to track it
        }
    }

    public void DetachRoute()
    {
        active = false;

        // Cancel all in-transit apostles and return them
        foreach (var mover in inTransit)
        {
            FromBase.ReturnApostles(2); // Or however you want to refund
            Destroy(mover.gameObject);
        }
        ToSatellite.RemoveIncomingRoute(this);
        inTransit.Clear();
        Destroy(LineRenderer);
        Destroy(gameObject);
    }

    private Vector3[] GetOrthogonalPath(Vector3 from, Vector3 to)
    {
        Vector3 mid1 = new Vector3(to.x, from.y, to.z); // horizontal first
        Vector3 mid2 = new Vector3(from.x, to.y, to.z); // vertical first

        // You can later randomize between the two styles if you want variety
        bool preferHorizontalFirst = true;

        if (preferHorizontalFirst)
            return new Vector3[] { from, mid1, to };
        else
            return new Vector3[] { from, mid2, to };
    }
}
