using JetBrains.Annotations;
using UnityEngine;

public class ActsTimeKeeper : MonoBehaviour
{
    public static ActsTimeKeeper Instance { get; private set; }

    public int SpeedMultiplier;
    public int Timer;
    public float DeltaTime;
    private float currentTime;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        DeltaTime = Time.deltaTime * SpeedMultiplier;
        currentTime += DeltaTime;

        if (currentTime >= 1f)
        {
            Timer += 1;
            currentTime -= 1f; // keep the leftover time
        }
    }
}
