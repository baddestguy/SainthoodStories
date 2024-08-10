using UnityEngine;
using UnityEngine.UI;

public class GridTile : MonoBehaviour
{
    public Image GridImage;

    void Start()
    {
        GameClock.Ticked += UpdateGrid;
    }

    private void UpdateGrid(double time, int day)
    {
        //if(time < 6 || time >= 16)
        //{
        //    var c = GridImage.color;
        //    c.a = 0.025f;
        //    GridImage.color = c;
        //}
        //else
        //{
        //    var c = GridImage.color;
        //    c.a = 0.1f;
        //    GridImage.color = c;
        //}
    }

    private void OnDisable()
    {
        GameClock.Ticked -= UpdateGrid;
    }
}
