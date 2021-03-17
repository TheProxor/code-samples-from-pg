using Spine;
using Spine.Unity;
using UnityEngine;


namespace Drawmasters.Utils
{
    public class AllSpineSkinsLogger : MonoBehaviour
    {
        [SerializeField] private SkeletonAnimation asset = default;

        [Sirenix.OdinInspector.Button]
        private void LogAllSKins()
        {
            foreach (var skin in asset.Skeleton.Data.Skins)
            {
                Debug.Log($"{skin}");
            }
        }


        private void Reset()
        {
            asset = asset ?? GetComponent<SkeletonAnimation>();
            asset = asset ?? GetComponentInChildren<SkeletonAnimation>();
            asset = asset ?? GetComponentInParent<SkeletonAnimation>();
        }
    }
}
