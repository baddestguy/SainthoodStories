using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enviro;
using UnityEngine;

public class EvilSpiritsManager : MonoBehaviour
{
    public GameObject[] EnemyCollection;
  
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ActivateNewEnemies());
    }

    IEnumerator ActivateNewEnemies()
    {
        int collectionIndex = 0;
        var player = FindObjectOfType<WorldPlayer>();

        while (true)
        {
            if(EnviroManager.instance.Time.hours > 21 || EnviroManager.instance.Time.hours < 5)
            {
                yield return new WaitForSeconds(30);

                var allEnemies = GetComponentsInChildren<SacredItem>().Where(c => c.Behaviour == SacredItemBehaviour.CHASE);
                if (allEnemies.Count() > 5) continue;

                var distance = 1000000f;
                for (int i = 0; i < EnemyCollection.Length; i++)
                {
                    var newDistance = Vector3.Distance(EnemyCollection[i].transform.position, player.transform.position);
                    if(newDistance < distance)
                    {
                        distance = newDistance;
                        collectionIndex = i;
                    }
                }

                EnemyCollection[collectionIndex].SetActive(true);
            }
            else
            {
                collectionIndex = 0;
                foreach(var col in EnemyCollection)
                {
                    col.SetActive(false);
                }
                yield return null;
            }
        }
    }
}
