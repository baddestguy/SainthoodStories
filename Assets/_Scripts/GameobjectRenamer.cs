using UnityEngine;

public class GameObjectRenamer : MonoBehaviour
{
    [ContextMenu("Rename Game Objects")]
    private void RenameGameObjects()
    {
        CollectibleItem [] sacredItems = FindObjectsByType<CollectibleItem>(FindObjectsInactive.Include, FindObjectsSortMode.None);

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