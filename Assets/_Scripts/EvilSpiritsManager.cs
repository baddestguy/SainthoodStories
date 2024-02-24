using System.Collections;
using System.Collections.Generic;
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
        while (true)
        {
            if(EnviroManager.instance.Time.hours > 21 || EnviroManager.instance.Time.hours < 5)
            {
                yield return new WaitForSeconds(30);
                
                if(collectionIndex < EnemyCollection.Length)
                {
                    EnemyCollection[collectionIndex].SetActive(true);
                    collectionIndex++;
                }
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
