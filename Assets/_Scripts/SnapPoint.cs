using UnityEngine;

public class SnapPoint : MonoBehaviour
{
    public enum BeadType { Crucifix, OurFather, HailMary, Medal }
    public BeadType expectedType;
    public bool isOccupied = false;
}
