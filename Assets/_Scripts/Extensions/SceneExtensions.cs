using UnityEngine.SceneManagement;

namespace Assets._Scripts.Extensions
{
    public static class SceneExtensions
    {
        public static bool IsGameLevel(this Scene scene) => scene.name.Contains("Level");
        public static bool IsMenu(this Scene scene) => scene.name.Contains("MainMenu");
        public static bool IsSaintShowcase(this Scene scene) => scene.name.Contains(SceneID.SaintsShowcase_Day.ToString());
        public static bool IsWorldMap(this Scene scene) => scene.name.Contains(SceneID.WorldMap.ToString());
        public static bool IsPauseMenu(this Scene scene) => scene.name.Contains(SceneID.PauseMenu.ToString());
    }
}
