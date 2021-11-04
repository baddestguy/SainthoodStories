using System.Collections.Generic;
using System.Linq;
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
        List<SaintID> saintIDs = GameDataManager.Instance.Saints.Keys.ToList();
        saintIDs = saintIDs.Where(x => !UnlockedSaints.Any(s => s.Id == x)).ToList(); //here we have al the saints not unlocked
        if(saintIDs.Count > 0)
        {
            SaintID randomPick = saintIDs[Random.Range(0, saintIDs.Count - 1)];
            saint = GameDataManager.Instance.Saints[randomPick];
            UnlockedSaints.Add(saint);
            NewSaint = saint;
        }
    }

    public void LoadSaints(SaintID[] saintIDs)
    {
        NewSaint = new SaintData() { Id = SaintID.NONE };
        if (saintIDs == null) return;
        UnlockedSaints.Clear();
        foreach(var saintID in saintIDs)
        {
            UnlockedSaints.Add(GameDataManager.Instance.Saints[saintID]);
        }
    }

    public void OnOverride(SaintID newSaints)
    {
        SaintData saint = GameDataManager.Instance.Saints[newSaints];
        if (UnlockedSaints.Contains(saint)) return;

        UnlockedSaints.Add(saint);
        NewSaint = saint;
    }

    public void OnOverrideRandom()
    {
        UnlockSaint();
    }
}
