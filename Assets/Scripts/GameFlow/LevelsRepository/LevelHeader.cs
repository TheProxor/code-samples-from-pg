using UnityEngine;


namespace Drawmasters.LevelsRepository
{
    public class LevelHeader : ScriptableObject
    {
        public string title = default;
        public GameMode mode = default;
        public bool isDisabled = default;
        
        public int projectilesCount = default;
        public WeaponType weaponType = default;

        public LevelType levelType = LevelType.Simple;
        public int stagesCount = default;

        [Header("Boss stage settings")]
        public int[] stageProjectilesCount = default;
    }
}
