using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class CollectiblesManager : MonoBehaviour
{
    public GameObject[] CollectibleCollections;

    // Start is called before the first frame update
    void Start()
    {
        if (MissionManager.Instance.CurrentCollectibleMissionId > CollectibleCollections.Count()) return;
        CollectibleCollections[MissionManager.Instance.CurrentCollectibleMissionId-1].SetActive(true);

        var collectibleList = new List<string>();
        collectibleList.AddRange(GetCollectibleList());
        
        if (collectibleList.Count == 0) return; //Player has collected all collectibles

        foreach (var collection in CollectibleCollections)
        {
            if (!collection.activeSelf) continue;

            var list = collection.GetComponentsInChildren<CollectibleItem>(true).Where(go => !InventoryManager.Instance.Collectibles.Any(c => c.Contains(go.name+":"))).ToArray();
            var spawnAmount = GameDataManager.Instance.CollectibleObjectivesData[MissionManager.Instance.CurrentCollectibleMissionId].Amount - MissionManager.Instance.CurrentCollectibleCounter;
            for (int i = 0; i < spawnAmount; i++)
            {
                var item = list[i];
                var it = collectibleList[Random.Range(0, collectibleList.Count-1)];
                collectibleList.Remove(it);
                item.gameObject.SetActive(true);
                item.Name = it;
            }
        }
    }

    private List<string> GetCollectibleList()
    {
        var saveData = GameManager.Instance.SaveData;

        if(saveData.WorldCollectibles == null || saveData.WorldCollectibles.Count() == 0)
        {
            var cols = GameDataManager.Instance.CollectibleData;
            var newList = (from objList in cols.Values
                               from obj in objList
                               where !saveData.Collectibles?.Contains(obj.Name) ?? true
                               select obj.Name).Distinct().ToList();
            GameManager.Instance.WorldCollectibles = newList;
            return newList;
        }

        return saveData.WorldCollectibles.ToList();
    }
}
