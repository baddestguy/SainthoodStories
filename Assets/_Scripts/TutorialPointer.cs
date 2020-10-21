using UnityEngine;

public class TutorialPointer : MonoBehaviour
{
    private Vector3 StartPos = new Vector3(0,0,30f);
    private Vector3 EndPos = new Vector3(0,0,50f);
    private bool Forward = true;

    public GameObject GroundFX;
    public Transform Pointer;

    void Start()
    {
        GroundFX.SetActive(false);
        Pointer.gameObject.SetActive(false);
    }

    void Update()
    {
        if (TutorialManager.Instance.CurrentTutorialStep != 2) return;
        if (EventsManager.Instance.EventInProgress) return;
        if (GameManager.Instance.GameClock.Time > 7 || TutorialManager.Instance.CurrentTutorialStep > 2)
        {
            Destroy(gameObject);
        }

        Pointer.gameObject.SetActive(true);

        if (Mathf.Abs(Pointer.eulerAngles.z - EndPos.z) < 0.01f)
        {
            GroundFX.SetActive(true);
            Forward = false;
        }
        if (Mathf.Abs(Pointer.eulerAngles.z - StartPos.z) < 0.01f)
        {
            GroundFX.SetActive(false);
            Forward = true;
        }
        
        if(Forward)
            Pointer.eulerAngles = Vector3.Lerp(Pointer.eulerAngles, EndPos, Time.deltaTime * 5);
        else
            Pointer.eulerAngles = Vector3.Lerp(Pointer.eulerAngles, StartPos, Time.deltaTime * 5);

    }
}
