using UnityEngine;

namespace Assets._Scripts.Extensions
{
    public static class GameObjectExtensions
    {
        public static GameObject FindDeepChild(this GameObject parent, string childName)
        {
            foreach (Transform child in parent.transform)
            {
                if (child.name == childName) return child.gameObject;

                // Recursively search in each child
                var found = child.gameObject.FindDeepChild(childName);
                if (found != null) return found;
            }

            return null;
        }
    }
}
