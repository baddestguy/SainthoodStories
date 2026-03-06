using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;

public class UGS_Analytics : MonoBehaviour
{
    public static UGS_Analytics Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    async void Start()
    {
        await UnityServices.InitializeAsync();
        GiveConsent(); //Get user consent according to various legislations
    }

    public void GiveConsent()
    {
        AnalyticsService.Instance.StartDataCollection();
        Debug.Log($"Consent has been provided. The SDK is now collecting data!");
    }

    public void LogSaintBegin(string saint)
    {
        CustomEvent e = new CustomEvent("story_begin")
        {
            { "saint", saint }
        };

        AnalyticsService.Instance.RecordEvent(e);
    }

    public void LogCompletedSaint(string saint)
    {
        CustomEvent e = new CustomEvent("story_end")
        {
            { "saint", saint }
        };

        AnalyticsService.Instance.RecordEvent(e);
    }

    public void LogQuitStory(string saint, int sequenceNumber)
    {
        CustomEvent e = new CustomEvent("story_cancel")
        {
            { "saint", saint },
            { "sequence_number", sequenceNumber }
        };

        AnalyticsService.Instance.RecordEvent(e);
    }
}