using UnityEngine;

public class GameObjectRenamer : MonoBehaviour
{
    [ContextMenu("Rename Game Objects")]
    private void RenameGameObjects()
    {
        CollectibleItem [] sacredItems = FindObjectsOfType<CollectibleItem>(true);

        int index = 1;
        foreach (CollectibleItem item in sacredItems)
        {
            GameObject obj = item.gameObject;
                obj.name = "SacredObject_" + index;
                index++;
        }

        Debug.Log("Game objects renamed successfully.");
    }
}