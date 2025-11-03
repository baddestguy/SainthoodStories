using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using static SnapPoint;

public class DraggableBead : MonoBehaviour, IPointerClickHandler
{
    public BeadType beadType;

    private bool Snapped;
    private SnapPoint MySnapPoint;
    private CanvasGroup canvasGroup;
    private Vector3 originalPosition;


    void Awake()
    {
   //     canvasGroup = GetComponent<CanvasGroup>();
    }

    //public void OnBeginDrag(PointerEventData eventData)
    //{
    //    originalParent = transform.parent;
    //    originalPosition = transform.position;
    //    canvasGroup.blocksRaycasts = false;
    //}

    void Update()
    {
        if (Snapped) return;

        Vector3 worldPoint;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            transform.parent.GetComponent<RectTransform>(),
            Input.mousePosition,
            Camera.main,
            out worldPoint
        );

        transform.position = Input.mousePosition;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var hit in results)
            {
                SnapPoint snap = hit.gameObject.GetComponent<SnapPoint>();
                if (snap != null && !snap.isOccupied && snap.expectedType == beadType)
                {
                    transform.position = snap.transform.position;
                    snap.isOccupied = true;
                    RosaryMakerInventoryUI.Instance.OnSelected(false);
                    Snapped = true;
                    MySnapPoint = snap;
                    snap.gameObject.SetActive(false);
                    return;
                }
                else if (Snapped && hit.gameObject == gameObject)
                {
                    MySnapPoint.isOccupied = false;
                    MySnapPoint.gameObject.SetActive(true);
                    RosaryMakerInventoryUI.Instance.OnSelected(true);
                    Snapped = false;
                    return;
                }
            }

            //Error shake/wobble
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if(MySnapPoint != null)
            {
                MySnapPoint.isOccupied = false;
                MySnapPoint.gameObject.SetActive(true);
            }
            RosaryMakerInventoryUI.Instance.OnSelected(false);
            Destroy(gameObject);
        }
    }
}
