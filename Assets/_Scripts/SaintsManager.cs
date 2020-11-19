using System.Collections.Generic;
using UnityEngine;

public class SaintsManager : MonoBehaviour
{
    public static SaintsManager Instance { get; private set; }
    public List<SaintData> UnlockedSaints = new List<SaintData>();
    public SaintData NewSaint;

    private void Awake()
    {
        Instance = this;
    }

    public void UnlockSaint()
    {
        SaintData saint;
        do
        {
            saint = GameDataManager.Instance.Saints[(SaintID)Random.Range(0, GameDataManager.Instance.Saints.Count)];
        }
        while (UnlockedSaints.Contains(saint));

        UnlockedSaints.Add(saint);
        NewSaint = saint;
    }

    public void LoadSaints(SaintID[] saintIDs)
    {
        NewSaint = null;
        if (saintIDs == null) return;
        UnlockedSaints.Clear();
        foreach(var saintID in saintIDs)
        {
            UnlockedSaints.Add(GameDataManager.Instance.Saints[saintID]);
        }
    }
}
