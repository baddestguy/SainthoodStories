using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ActsChurchNode : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool IsBase = false;
    public float ApostleGenerationInterval = 5f;
    public int ApostleCount = 0;
    public TextMeshProUGUI ApostleText;
    public bool IsDead = false;
    public bool HasBeenConvertedToBase = false;
    public float DecayTime = 300f;
    public int ApostlesNeededToConvert = 6;
    public Image ControlRingMax;
    public RectTransform CurrentControlRing; // Assign in Inspector
    public float DelayBeforeDecay = 10f;
    public ActsChurchRoute CurrentRoute;

    private float timer = 0f;
    private ActsApostleRouter router;
    private float decayTimer;
    private int decayTimerMultiplier;
    private Vector2 _initialCircleSize;

    void Awake()
    {
        router = FindObjectOfType<ActsApostleRouter>();
    }

    void Start()
    {
        DelayBeforeDecay = Random.Range(10, 20);
        decayTimer = DecayTime;
        decayTimerMultiplier = Random.Range(4, 10);
        if (CurrentControlRing != null)
            _initialCircleSize = CurrentControlRing.sizeDelta;
        UpdateApostleText();
        StartCoroutine(DelayConversionCheck());
    }

    void Update()
    {
        if (IsDead) return;

        if (!IsBase)
        {
            if(DelayBeforeDecay <= 0)
            {
                CurrentControlRing.gameObject.SetActive(true);

                var interval = (decayTimerMultiplier - ApostleCount) * ActsTimeKeeper.Instance.DeltaTime;
                if(interval < 0)
                {
                    CurrentControlRing.GetComponent<Image>().color = Color.grey;
                }
                else
                {
                    CurrentControlRing.GetComponent<Image>().color = Color.red;
                }

                decayTimer -= interval;
                if (CurrentControlRing != null && DecayTime > 0)
                {
                    float scale = 1f - Mathf.Clamp01(decayTimer / DecayTime);
                    CurrentControlRing.sizeDelta = _initialCircleSize * scale;
                }

                if (decayTimer <= 0f)
                {
                    CollapseChurch();
                }

                float tolerance = 90000f;
                bool almostSameSize = Mathf.Abs(CurrentControlRing.sizeDelta.sqrMagnitude - _initialCircleSize.sqrMagnitude) < tolerance;
                if (almostSameSize)
                {
                    ControlRingMax.gameObject.SetActive(true);
                }
                else
                {
                    ControlRingMax.gameObject.SetActive(false);
                }
            }
            else
            {
                CurrentControlRing.sizeDelta = new Vector2(0, 0);
                ControlRingMax.gameObject.SetActive(false);
                DelayBeforeDecay -= ActsTimeKeeper.Instance.DeltaTime;
            }
        }
        else
        {
            timer += ActsTimeKeeper.Instance.DeltaTime;
            if (timer >= ApostleGenerationInterval)
            {
                GenerateApostle();
                timer = 0f;
            }
        }
    }

    IEnumerator DelayConversionCheck()
    {
        if (IsBase) yield break;

        yield return new WaitForSeconds(25f);

        while (true)
        {
            yield return null;
            if (CurrentControlRing != null && CurrentControlRing.sizeDelta.sqrMagnitude < 0.001f)
            {
                ConvertToBase();
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        router?.BeginDrag(this);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        foreach (var obj in eventData.hovered)
        {
            var node = obj.GetComponent<ActsChurchNode>();
            if (node != null)
            {
                router?.EndDrag(node);
                return;
            }
        }
    }

    public bool CanSendTwoApostles()
    {
        return ApostleCount >= 2;
    }

    public bool UseTwoApostles()
    {
        if (ApostleCount >= 2)
        {
            SpendApostles(2);
            return true;
        }
        return false;
    }

    public void ReturnApostles(int count)
    {
        ApostleCount += count;
    }

    public void ReceiveApostles(int amount, ActsChurchRoute route)
    {
        if (IsDead) return;

        ApostleCount += amount;

        UpdateApostleText();
    }

    void GenerateApostle()
    {
        ApostleCount += 1;
        UpdateApostleText();
    }

    public void SpendApostles(int amount)
    {
        ApostleCount -= amount;
        UpdateApostleText();
    }

    public bool HasEnoughApostles(int amount)
    {
        return ApostleCount >= amount;
    }

    void UpdateApostleText()
    {
        if (ApostleText != null && IsBase)
            ApostleText.text = ApostleCount.ToString();
    }

    void CollapseChurch()
    {
        IsDead = true;
        ApostleText.text = "LOST";
        //Grey/Disable icons

        Destroy(ControlRingMax.gameObject, 5/ActsTimeKeeper.Instance.SpeedMultiplier);
        Destroy(CurrentControlRing.gameObject, 5/ ActsTimeKeeper.Instance.SpeedMultiplier);
    }

    void ConvertToBase()
    {
        IsBase = true;
        HasBeenConvertedToBase = true;

        //Destroy all routes connected to me
        foreach (var route in IncomingRoutes.ToList()) // clone to avoid modifying during iteration
        {
            route.DetachRoute();
        }
        IncomingRoutes.Clear();

        Instantiate(ActsChurchSpawner.Instance.BaseChurchPrefab, transform.position, Quaternion.identity, this.transform.parent);
        Destroy(gameObject);
    }

    public List<ActsChurchRoute> IncomingRoutes = new();

    public void AddIncomingRoute(ActsChurchRoute route)
    {
        if (!IncomingRoutes.Contains(route))
            IncomingRoutes.Add(route);
    }

    public void RemoveIncomingRoute(ActsChurchRoute route)
    {
        IncomingRoutes.Remove(route);
    }

}
