using UnityEngine;

public class ActsApostleRouter : MonoBehaviour
{
    public LineRenderer Line;
    private ActsChurchNode _startNode = null;

    void Update()
    {
        if (_startNode != null)
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;
            Line.SetPosition(1, mouseWorld);
        }
    }

    public void BeginDrag(ActsChurchNode node)
    {
        if (!node.IsBase) return;

        _startNode = node;
        Line.positionCount = 2;
        Line.SetPosition(0, node.transform.position);
        Line.SetPosition(1, node.transform.position);
        Line.enabled = true;
    }

    public void EndDrag(ActsChurchNode target)
    {
        if (_startNode == null || target == _startNode)
        {
            CancelDrag();
            return;
        }

        if (target.IsBase || target.IsDead)
        {
            CancelDrag();
            return;
        }

        // If _startNode already has an active route, detach it
        if (_startNode.CurrentRoute != null)
        {
            _startNode.CurrentRoute.DetachRoute();
            _startNode.CurrentRoute = null;
        }

        // Create new route GameObject
        GameObject routeGO = new GameObject("ChurchRoute");
        routeGO.transform.SetParent(ActsChurchSpawner.Instance.transform);
        ActsChurchRoute route = routeGO.AddComponent<ActsChurchRoute>();
        route.Initialize(_startNode, target);

        // Assign the route so base can reference it later
        _startNode.CurrentRoute = route;
        target.AddIncomingRoute(route);

        CancelDrag();
    }

    public void CancelDrag()
    {
        _startNode = null;
        Line.positionCount = 0;
        Line.enabled = false;
    }
}
