using UnityEngine;


namespace Drawmasters.Levels
{
    public class LevelTargetLink : MonoBehaviour
    {
        [SerializeField] private LevelTarget levelTarget = default;

        public LevelTarget LevelTarget => levelTarget;


        #if UNITY_EDITOR
        private void Reset()
        {
            levelTarget = GetComponent<LevelTarget>();

            if (levelTarget == null)
            {
                levelTarget = GetComponentInChildren<LevelTarget>();
            }

            if (levelTarget == null)
            {
                levelTarget = GetComponentInParent<LevelTarget>();
            }
        }
        #endif
    }
}