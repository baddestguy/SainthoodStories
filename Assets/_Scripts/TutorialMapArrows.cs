using UnityEngine;

public class TutorialMapArrows : MonoBehaviour
{
    public GameObject Group1;
    public GameObject Group2;
    public GameObject Group3;
    public GameObject Group4;
    public GameObject Group5;

    public void SetActive(bool active)
    {
        Group1.SetActive(active);
        Group2.SetActive(active);
        Group3.SetActive(active);
        Group4.SetActive(active);
        Group5.SetActive(active);
    }
}
