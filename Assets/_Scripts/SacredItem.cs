using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Enviro;
//using FMODUnity;
using Opsive.UltimateCharacterController.Character;
using UnityEngine;
using UnityEngine.AI;

public class SacredItem : MonoBehaviour
{
    public SacredItemBehaviour Behaviour;
    public List<Transform> EndLocations = new List<Transform>();

    private Material RedMaterial;
    private Material GreenMaterial;
    private Material WhiteMaterial;
    private Renderer MyRenderer;
    private GameObject MyExplosion;
    private GameObject MySphere;
    private bool HasTriggered = false;
    private NavMeshAgent Agent;

    // Start is called before the first frame update
    void Start()
    {
        MyRenderer = transform.GetComponentInChildren<Renderer>();
        WhiteMaterial = MyRenderer.material;
        RedMaterial = Resources.Load<Material>("Materials/Glow Mat Red");
        GreenMaterial = Resources.Load<Material>("Materials/Glow Mat Green");
        MyExplosion = Resources.Load<GameObject>("MemoryExplodeFx");
        MyRenderer.material = GreenMaterial;
    //    MySphere = transform.Find("Sphere").gameObject;
        ExhibitBehaviour();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.name == "Nun")
        {
            if (Behaviour == SacredItemBehaviour.CHASE)
            {
                MissionManager.Instance.UpdateFaithPoints((int)-transform.localScale.x);
            }
            else if (Behaviour == SacredItemBehaviour.WANDER)
            {
                InventoryManager.Instance.AddWanderers((int)transform.localScale.x);
            }
            gameObject.SetActive(false);
        }
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

            case SacredItemBehaviour.CHASE:
                StartCoroutine(ChaseAsync());
                break;
        }
    }

    IEnumerator ChaseAsync()
    {
        Behaviour = SacredItemBehaviour.CHASE;
        var size = Random.Range(1, 5);
        transform.localScale = new Vector3(size, size, size);
        var player = FindObjectOfType<WorldPlayer>();
        Agent = GetComponent<NavMeshAgent>();
        Agent.speed = Random.Range(3.5f, 6);
        MyRenderer.material = RedMaterial;

        while (true)
        {
            if (MyRenderer.material != RedMaterial && (EnviroManager.instance.Time.hours > 18 || EnviroManager.instance.Time.hours < 5))
            {
                MyRenderer.material = RedMaterial;
            }
            else if (MyRenderer.material != WhiteMaterial)
            {
                MyRenderer.material = WhiteMaterial;
                StartCoroutine(WanderAsync());
                yield break;
            }

            Agent.SetDestination(player.transform.position);
            yield return null;
        }
    }

    IEnumerator WanderAsync()
    {
        Behaviour = SacredItemBehaviour.WANDER;
        Agent = GetComponent<NavMeshAgent>();
        Agent.speed = Random.Range(3.5f, 6);
        Vector3 boundsCenter = transform.position; 
        Vector3 boundsSize = new Vector3(500,0,500);
        MyRenderer.material = WhiteMaterial;

        while (true)
        {
            if (MyRenderer.material != RedMaterial && (EnviroManager.instance.Time.hours > 18 || EnviroManager.instance.Time.hours < 5))
            {
                MyRenderer.material = RedMaterial;
                StartCoroutine(ChaseAsync());
                yield break;
            }
            else if (MyRenderer.material != WhiteMaterial)
            {
                MyRenderer.material = WhiteMaterial;
            }

            Vector3 randomPosition = boundsCenter + new Vector3(Random.Range(-boundsSize.x / 2f, boundsSize.x / 2f),
                                                            0f,
                                                            Random.Range(-boundsSize.z / 2f, boundsSize.z / 2f));

            Agent.SetDestination(randomPosition);

            yield return new WaitForSeconds (5f);
        }
    }

    IEnumerator RunawayAsync()
    {
        var player = FindObjectOfType<WorldPlayer>();
        Agent = GetComponent<NavMeshAgent>();
        Agent.speed = 6;
        var playerMotor = player.GetComponentInChildren<AnimatorMonitor>();

        while (true)
        {
            yield return null;
            var speed = playerMotor.ForwardMovement;
            if(speed > 1f && Vector3.Distance(player.transform.position, transform.position) < 5)
            {
                var fleeDirection = transform.position - player.transform.position;
                Agent.destination = transform.position + fleeDirection;
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
