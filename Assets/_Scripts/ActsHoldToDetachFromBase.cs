using UnityEngine;
using UnityEngine.EventSystems;

public class ActsHoldToDetachFromBase : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float HoldDuration = 1.5f;
    private float _holdTimer = 0f;
    private bool _holding = false;

    private ActsChurchNode _node;

    void Awake()
    {
        _node = GetComponent<ActsChurchNode>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_node.IsBase && _node.CurrentRoute != null)
        {
            _holding = true;
            _holdTimer = 0f;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _holding = false;
    }

    void Update()
    {
        if (_holding)
        {
            _holdTimer += Time.deltaTime;

            if (_holdTimer >= HoldDuration)
            {
                _node.CurrentRoute.DetachRoute(); // route will clear itself from both base & satellite
                _holding = false;
            }
        }
    }
}
