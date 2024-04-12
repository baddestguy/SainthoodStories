using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enviro;
using UnityEngine;

public class EvilSpiritsManager : MonoBehaviour
{
    public GameObject[] EnemyCollection;
    public TextMesh TextDisplay;
  
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ActivateNewEnemies());
    }

    IEnumerator ActivateNewEnemies()
    {
        var player = FindObjectOfType<WorldPlayer>();

        while (true)
        {
            yield return new WaitForSeconds(20);

            var allEnemies = GetComponentsInChildren<SacredItem>().Where(c => c.Behaviour == SacredItemBehaviour.CHASE || c.Behaviour == SacredItemBehaviour.WANDER);
            var shouldContinue = false;
            if (allEnemies.Count() > 5)
            {
                for (int i = 0; i < EnemyCollection.Length; i++)
                {
                    if (!EnemyCollection[i].activeSelf) continue;

                    var newDistance = Vector3.Distance(EnemyCollection[i].transform.position, player.transform.position);
                    if (newDistance < 200)
                    {
                        shouldContinue = true;
                        break;
                    }
                }

                if (shouldContinue) continue;
            }

            var distance = 1000000f;
            var largerDistance = 0f;
            int collectionIndex = -1;
            var furthestAway = -1;
            for (int i = 0; i < EnemyCollection.Length; i++)
            {
                var newDistance = Vector3.Distance(EnemyCollection[i].transform.position, player.transform.position);
                if(newDistance < distance && !EnemyCollection[i].activeSelf)
                {
                    distance = newDistance;
                    collectionIndex = i;
                }
                if(newDistance > largerDistance && EnemyCollection[i].activeSelf)
                {
                    largerDistance = newDistance;
                    furthestAway = i;
                }
            }

            if(collectionIndex != -1)
            {
                EnemyCollection[collectionIndex].SetActive(true);
            }
            if(furthestAway != -1)
            {
                EnemyCollection[furthestAway].SetActive(false);
            }
        }
    }
}
