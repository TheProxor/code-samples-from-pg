using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Sirenix.OdinInspector;
using Drawmasters.Levels;
using UnityEngine;
using Object = UnityEngine.Object;
using Drawmasters.Pool;
using Drawmasters.Ui;


namespace Drawmasters
{
    [CreateAssetMenu(fileName = "ContentStorage",
                   menuName = NamingUtility.MenuItems.Singletons + "ContentStorage")]
    public partial class ContentStorage : SerializedScriptableObject
    {
        #region Fields

        public class LevelObjectItem
        {
            public int Index = default;
            [ResourceLink] public AssetLink Link = default;

            // https://fogbugz.unity3d.com/default.asp?1103327_ieeg1qk22ppb39tg&_ga=2.61857388.1712007023.1577339339-1498481534.1569474615
            // Monolith dissapeared after pooling in certain moments
            public bool shouldUsePool = default;
        }

        [Header("Storage data")]
        [SerializeField] [ResourceLink] private List<LevelObjectItem> levelObjects = default;

        [SerializeField] private Dictionary<CustomPrefabType, AssetLinkWrapper> prefabs = new Dictionary<CustomPrefabType, AssetLinkWrapper>();
        [SerializeField] private Dictionary<ProjectileType, AssetLinkWrapper> projectilePrefabs = new Dictionary<ProjectileType, AssetLinkWrapper>();
        [SerializeField] private Dictionary<ScreenType, AssetLinkWrapper> guiScreens = new Dictionary<ScreenType, AssetLinkWrapper>();
        [SerializeField] private Dictionary<MultipleRewardType, UiRewardItem> multipleRewardItems = default;

        #endregion



        #region Properties

        public List<LevelObjectItem> LevelObjects => levelObjects;

        public int[] Indexes => levelObjects.Select(a => a.Index).ToArray();

        #endregion



        #region Methods

        public void InitializePools()
        {
            InitializePools<ProjectileType, Projectile>(projectilePrefabs);
        }


        public UiRewardItem CreateRewardItem(MultipleRewardType type, Transform root)
        {
            UiRewardItem result = default;

            if (multipleRewardItems.TryGetValue(type, out UiRewardItem original))
            {
                result = Instantiate(original);

                result.MainTransform.SetParent(root);
                result.MainTransform.anchoredPosition = Vector3.zero;
                result.MainTransform.localPosition = Vector3.zero;
                result.MainTransform.localScale = Vector3.one;
            }
            else
            {
                CustomDebug.Log("Cannot find prefab. For type: " + type);
            }

            return result;
        }


        public LevelObject GetLevelObject(int index)
        {
            LevelObject result = default;

            LevelObjectItem foundItem = levelObjects.Find(i => (i != null && i.Index == index));

            bool isItemExists = foundItem != null &&
                                foundItem.Link != null &&
                                foundItem.Link.IsAssetExists;

            if (isItemExists)
            {
                result = (foundItem.Link.GetAsset() as GameObject).GetComponent<LevelObject>();
            }

            return result;
        }


        public GameObject PrefabByType<T>(T type) where T : IConvertible
        {
            GameObject result = null;
            AssetLinkWrapper link = LinkWrapperByType<T>(type);

            if (link != null)
            {
                result = LoadObjectFromLinkWrapper(link);
            }

            return result;
        }


        public Object OriginalObjectByType<T>(T type) where T : IConvertible
        {
            Object result = null;

            AssetLinkWrapper link = LinkWrapperByType<T>(type);

            if (link != null)
            {
                result = LoadOriginalObjectFromLinkWrapper(link);
            }

            return result;
        }


        private AssetLinkWrapper LinkWrapperByType<T>(T type) where T : IConvertible
        {
            AssetLinkWrapper linkWraper = null;

            if (typeof(T) == typeof(CustomPrefabType))
            {
                CustomPrefabType currentType = (CustomPrefabType)type.ToInt32(CultureInfo.InvariantCulture);

                TryGetLink(prefabs, currentType, out linkWraper);
            }
            else if (typeof(T) == typeof(ScreenType))
            {
                ScreenType currentType = (ScreenType)type.ToInt32(CultureInfo.InvariantCulture);

                TryGetLink(guiScreens, currentType, out linkWraper);
            }
            else if (typeof(T) == typeof(ProjectileType))
            {
                ProjectileType currentType = (ProjectileType)type.ToInt32(CultureInfo.InvariantCulture);

                TryGetLink(projectilePrefabs, currentType, out linkWraper);
            }

            return linkWraper;
        }


        private bool TryGetLink<T>(Dictionary<T, AssetLinkWrapper> sourceDictionary, T type, out AssetLinkWrapper linkWrapper)
        {
            return sourceDictionary.TryGetValue(type, out linkWrapper);
        }


        private GameObject LoadObjectFromLinkWrapper(AssetLinkWrapper linkWrapper)
        {
            GameObject result = null;

            Object originalObject = LoadOriginalObjectFromLinkWrapper(linkWrapper);

            if (originalObject != null)
            {
                result = originalObject as GameObject;

                if (linkWrapper.shouldUsePool)
                {
                    // TODO check
                    //PoolManager.Instance.PoolForObject(result);
                }
            }

            return result;
        }


        private Object LoadOriginalObjectFromLinkWrapper(AssetLinkWrapper linkWrapper)
        {
            Object result = null;

            if (linkWrapper != null)
            {
                AssetLink link = linkWrapper.link;

                result = link.GetAsset();
            }

            return result;
        }


        private void InitializePools<T, HComponent>(Dictionary<T, AssetLinkWrapper> info) where HComponent : MonoBehaviour
        {
            foreach (KeyValuePair<T, AssetLinkWrapper> item in info)
            {
                if (item.Value.shouldUsePool &&
                    item.Value.preInstantiateCount > 0)
                {
                    Object originalObject = LoadOriginalObjectFromLinkWrapper(item.Value);

                    if (originalObject != null)
                    {
                        GameObject originalGameObject = originalObject as GameObject;

                        if (originalGameObject != null)
                        {
                            HComponent component = originalGameObject.GetComponent<HComponent>();

                            if (component != null)
                            {
                                PoolManager.Instance.GetComponentPool(component, true, item.Value.preInstantiateCount);
                            }
                            else
                            {
                                CustomDebug.Log("Loaded original object doesn't contation component : " + nameof(HComponent) + ".");
                            }
                        }
                        else
                        {
                            CustomDebug.Log("Loaded original object is not a gameObject.");
                        }
                    }
                    else
                    {
                        CustomDebug.Log("Cannot load original object.");
                    }
                }
            }
        }

        #endregion
    }
}
