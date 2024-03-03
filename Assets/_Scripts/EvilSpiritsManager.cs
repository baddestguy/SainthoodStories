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
        int collectionIndex = 0;
        var player = FindObjectOfType<WorldPlayer>();

        while (true)
        {
            yield return new WaitForSeconds(20);

            var allEnemies = GetComponentsInChildren<SacredItem>().Where(c => c.Behaviour == SacredItemBehaviour.CHASE || c.Behaviour == SacredItemBehaviour.WANDER);
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
    }
}
