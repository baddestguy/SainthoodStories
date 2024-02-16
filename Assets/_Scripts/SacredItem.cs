using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
//using FMODUnity;
using Opsive.UltimateCharacterController.Character;
using UnityEngine;

public class SacredItem : MonoBehaviour
{
    public SacredItemBehaviour Behaviour;
    public List<Transform> EndLocations = new List<Transform>();

    private Material RedMaterial;
    private Material GreenMaterial;
    private Renderer MyRenderer;
    private GameObject MyExplosion;
    private GameObject MySphere;
    private bool HasTriggered = false;

    // Start is called before the first frame update
    void Start()
    {
        MyRenderer = transform.GetComponentInChildren<Renderer>();
        RedMaterial = Resources.Load<Material>("Materials/Glow Mat Red");
        GreenMaterial = Resources.Load<Material>("Materials/Glow Mat Green");
        MyExplosion = Resources.Load<GameObject>("MemoryExplodeFx");
    //    MySphere = transform.Find("Sphere").gameObject;
        ExhibitBehaviour();
    }

    public void Triggered()
    {
        if (HasTriggered) return;
        StartCoroutine(TriggeredAsync());
    }

    IEnumerator TriggeredAsync()
    {
        if (Behaviour == SacredItemBehaviour.DECOY)
        {
            //TODO: Turn it back to original color after a few seconds
            MyRenderer.material = RedMaterial;
            yield break;
        }
        if(Behaviour == SacredItemBehaviour.WEATHER_CHANGING)
        {
            var times = new int [] { 6, 13, 17 };
            WeatherManager.Instance.ChangeWeather(Random.Range(3, 7));
            WeatherManager.Instance.ChangeTimeOfDay(times[Random.Range(0, 3)]);
    ///        SoundManager.Instance.PlayOneShot(FMODEvents.Instance.CoinCollected, transform.position);
            var currentMaterial = MyRenderer.material;
            MyRenderer.material = GreenMaterial;
            yield return new WaitForSeconds(1f);
            MyRenderer.material = currentMaterial;
            yield break;
        }

        transform.DOKill();
     //   SoundManager.Instance.PlayOneShot(FMODEvents.Instance.CoinCollected, transform.position);
        MyRenderer.material = GreenMaterial;


        yield return new WaitForSeconds(1f);

        transform.DOMoveY(transform.position.y + 1.5f, 3f);

        yield return new WaitForSeconds(3f);

    //    MySphere.SetActive(false);
        Instantiate(MyExplosion, transform);

        yield return new WaitForSeconds(1f);

    //    GameManager.Instance.CollectMemory(MemoryId);
        gameObject.SetActive(false);
    //    GetComponent<StudioEventEmitter>().enabled = false;
    }

    public void ExhibitBehaviour()
    {
        var savedLocations = EndLocations.Select(p => p.position).ToList();
        switch (Behaviour)
        {
            case SacredItemBehaviour.HOVER:
            case SacredItemBehaviour.DECOY:
            case SacredItemBehaviour.WEATHER_CHANGING:
            case SacredItemBehaviour.BURST:
            case SacredItemBehaviour.CONCENTRATION:
                transform.DOMoveY(transform.position.y + 1f, 2f).SetLoops(-1, LoopType.Yoyo);
                break;

            case SacredItemBehaviour.PATROL:
                savedLocations = EndLocations.Select(p => p.position).ToList();
                transform.DOPath(savedLocations.ToArray(), 20).SetLoops(-1, LoopType.Yoyo);
                break;

            case SacredItemBehaviour.BOUNCE:
                transform.DOJump(EndLocations[0].position, 25, 1, 10).SetLoops(-1, LoopType.Yoyo);
                break;

            case SacredItemBehaviour.TELEPORT:
                StartCoroutine(TeleportAsync());
                break;

            case SacredItemBehaviour.ZIP:
                savedLocations = EndLocations.Select(p => p.position).ToList();
                transform.DOPath(savedLocations.ToArray(), 5).SetLoops(-1, LoopType.Yoyo);
                break;

            case SacredItemBehaviour.RUNAWAY:
                StartCoroutine(RunawayAsync());
                break;

            case SacredItemBehaviour.SPIRAL:
                transform.DOSpiral(25, new Vector3(0, 1, 0), speed: 0.1f, frequency: 1).SetLoops(-1, LoopType.Yoyo);
                break;
        }
    }

    IEnumerator RunawayAsync()
    {
        var player = FindObjectOfType<WorldPlayer>();
        var posIndex = 0;
        var originalPos = transform.position;
        var savedLocations = EndLocations.Select(p => p.position).ToList();
        while (true)
        {
            yield return null;
            var speed = player.GetComponentInChildren<AnimatorMonitor>().ForwardMovement;
            if(speed > 0.7f && Vector3.Distance(player.transform.position, transform.position) < 5)
            {
                if(posIndex >= savedLocations.Count)
                {
                    posIndex = 0;
                    transform.DOJump(originalPos, 10, 5, 5);
                }
                else
                {
                    transform.DOJump(savedLocations[posIndex], 10, 5, 5);
                    posIndex++;
                }
                yield return new WaitForSeconds(5);
            }
        }
    }

    IEnumerator TeleportAsync()
    {
        var savedLocations = EndLocations.Select(p => p.position).ToList();
        savedLocations.Add(transform.position);
        var lastPos = transform.position;
        while (true)
        {
            yield return new WaitForSeconds(3f);
            Vector3 pos;
            do
                pos = savedLocations[Random.Range(0, savedLocations.Count)];
            while (pos == lastPos);
            
            lastPos = transform.position;
            transform.position = pos;
        }
    }
}
