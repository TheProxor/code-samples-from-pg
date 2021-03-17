using System.Collections.Generic;
using Modules.InAppPurchase;
using Drawmasters.Levels;
using Drawmasters.Monolith;
using Drawmasters.Pool;
using Drawmasters.Pool.Interfaces;
using Drawmasters.Ui;
using Drawmasters.Announcer;
using Drawmasters.Proposal;
using Modules.General.InAppPurchase;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace Drawmasters
{
    public class ContentManagement
    {
        #region Fields

        private bool isBonusViewComponentCached;
        private bool isCurrencyAnnouncerComponentCached;
        private bool isCurrencyMonopolyAnnouncerComponentCached;

        private bool isHelpBubbleComponentCached;

        #endregion



        #region Properties

        public IPoolHelper<int, LevelObject> PoolHelper { get; private set; }

        private TMP_Text CachedCurrencyAnnouncerComponent { get; set; }

        private MonopolyCurrencyAnnouncer CachedCurrencyMonopolyAnnouncerComponent { get; set; }

        private SpeechBubble CachedSpeechBubbleComponent { get; set; }

        private ComponentPool CornersPool { get; set; }
        private Dictionary<ProjectileType, ComponentPool> ProjectilePools { get; set; }
        private ComponentPool uiSeasonEventLevelElementPool { get; set; }
        private ComponentPool uiSeasonEventRewardSimpleElementPool { get; set; }
        private ComponentPool uiSeasonEventRewardPassElementPool { get; set; }
        private ComponentPool uiSeasonEventRewardBonusElementPool { get; set; }
        private ComponentPool uiSeasonEventRewardMainElementPool { get; set; }

        private ComponentPool uiLeagueLeaderBoardElementPool { get; set; }

        #endregion



        #region Ctor

        public ContentManagement()
        {
            Dictionary<int, AssetLink> assets = new Dictionary<int, AssetLink>();

            foreach (var i in Content.Storage.LevelObjects)
            {
                if (i.shouldUsePool)
                {
                    if (!assets.ContainsKey(i.Index))
                    {
                        assets.Add(i.Index, i.Link);
                    }
                    else
                    {
                        CustomDebug.Log("Assets have already had the same asset. Index = " + i.Index + ".");
                    }
                }
            }

            PoolHelper = new CommonPoolHelper<int, LevelObject>(assets);

            ProjectilePools = new Dictionary<ProjectileType, ComponentPool>();

            CornerGraphic cornerPrefab = Content.Storage.PrefabByType(CustomPrefabType.Corner).GetComponent<CornerGraphic>();
            CornersPool = PoolManager.Instance.GetComponentPool(cornerPrefab);

            var uiSeasonEventLevelElementPrefab = Content.Storage.PrefabByType(CustomPrefabType.UiSeasonEventLevelElement).GetComponent<UiSeasonEventLevelElement>();
            uiSeasonEventLevelElementPool = PoolManager.Instance.GetComponentPool(uiSeasonEventLevelElementPrefab);

            var uiSeasonEventRewardSimpleElementPrefab = Content.Storage.PrefabByType(CustomPrefabType.RewardSimpleElement).GetComponent<UiSeasonEventRewardSimpleElement>();
            uiSeasonEventRewardSimpleElementPool = PoolManager.Instance.GetComponentPool(uiSeasonEventRewardSimpleElementPrefab);

            var uiSeasonEventRewardPassElementPrefab = Content.Storage.PrefabByType(CustomPrefabType.RewardPassElement).GetComponent<UiSeasonEventRewardPassElement>();
            uiSeasonEventRewardPassElementPool = PoolManager.Instance.GetComponentPool(uiSeasonEventRewardPassElementPrefab);
            
            var uiSeasonEventRewardMainElementPrefab = Content.Storage.PrefabByType(CustomPrefabType.RewardMainElement).GetComponent<UiSeasonEventRewardMainElement>();
            uiSeasonEventRewardMainElementPool = PoolManager.Instance.GetComponentPool(uiSeasonEventRewardMainElementPrefab);

            var uiSeasonEventRewardBonusElementPrefab = Content.Storage.PrefabByType(CustomPrefabType.RewardBonusElement).GetComponent<UiSeasonEventRewardBonusElement>();
            uiSeasonEventRewardBonusElementPool = PoolManager.Instance.GetComponentPool(uiSeasonEventRewardBonusElementPrefab);

            var uiLeagueLeaderBoardElementPrefab = Content.Storage.PrefabByType(CustomPrefabType.UiLeagueLeaderBoardElement).GetComponent<UiLeagueLeaderBoardElement>();
            uiLeagueLeaderBoardElementPool = PoolManager.Instance.GetComponentPool(uiLeagueLeaderBoardElementPrefab);
        }

        #endregion




        #region Methods

        public SpeechBubble CreateSpeechBubble(Transform parentTransform, Vector3 position)
        {
            if (!isHelpBubbleComponentCached)
            {
                CachedSpeechBubbleComponent = Content.Storage.PrefabByType(CustomPrefabType.SpeechBubble).GetComponent<SpeechBubble>();

                isHelpBubbleComponentCached = true;
            }

            ComponentPool pool = PoolManager.Instance.GetComponentPool(CachedSpeechBubbleComponent);

            SpeechBubble created = pool.Pop() as SpeechBubble;
            created.transform.SetParent(parentTransform);
            created.transform.position = position;

            return created;
        }
        

        public TMP_Text CreateCurrencyAnnouncer(Vector3 position)
        {
            if (!isCurrencyAnnouncerComponentCached)
            {
                CachedCurrencyAnnouncerComponent = (Content.Storage.PrefabByType(CustomPrefabType.CurrencyObjectAnnouncer) as GameObject).GetComponent<TMP_Text>();

                isCurrencyAnnouncerComponentCached = true;
            }

            ComponentPool pool = PoolManager.Instance.GetComponentPool(CachedCurrencyAnnouncerComponent);

            TMP_Text view = pool.Pop() as TMP_Text;

            view.transform.position = position;

            return view;
        }


        public SpriteRenderer CreateShooterScope(Transform parent, WeaponType type)
        {
            GameObject scope = UnityEngine.Object.Instantiate(Content.Storage.PrefabByType(CustomPrefabType.ShooterScope), parent);

            WeaponSettings settings = IngameData.Settings.modesInfo.GetSettings(type);

            SpriteRenderer scopeSpriteRenderer = scope.GetComponent<SpriteRenderer>();

            if (scopeSpriteRenderer != null)
            {
                if (settings is IProjectileSightSettings sightSettings)
                {
                    scopeSpriteRenderer.sprite = sightSettings.SightSprite;
                    scopeSpriteRenderer.color = sightSettings.SightColor;
                }
            }

            return scopeSpriteRenderer;
        }


        public MonopolyCurrencyAnnouncer CreateCurrencyMonopolyAnnouncer(Transform parent = null)
        {
            if (!isCurrencyMonopolyAnnouncerComponentCached)
            {
                CachedCurrencyMonopolyAnnouncerComponent = (Content.Storage.PrefabByType(CustomPrefabType.CurrencyMonopolyAnnouncer)).GetComponent<MonopolyCurrencyAnnouncer>();

                isCurrencyMonopolyAnnouncerComponentCached = true;
            }

            ComponentPool pool = PoolManager.Instance.GetComponentPool(CachedCurrencyMonopolyAnnouncerComponent);

            MonopolyCurrencyAnnouncer view = pool.Pop() as MonopolyCurrencyAnnouncer;
            view.transform.SetParent(parent);
            view.transform.localScale = Vector3.one;

            return view;
        }
        

        private readonly static HashSet<int> exlcludedObjects = new HashSet<int>
            {
                18, 19, 25, 39, 26, 41, 45, 46, 47, 42,
            };


        public LevelObject CreateLevelObject(int index, Transform parent = null)
        {
            //if (exlcludedObjects.Contains(index))
            //{
            //    //hotfix by maxim.a
            //    index = 18;
            //}

            LevelObject foundedLevelObject = FindLevelObject(index);            

            ContentStorage.LevelObjectItem foundItem = Content.Storage.LevelObjects.Find(i => i.Index == index);

            if (foundedLevelObject != null)
            {
                LevelObject levelObject;

                if (foundItem.shouldUsePool)
                {
                    levelObject = PoolHelper.PopObject(index);
                }
                else
                {
                    levelObject = Object.Instantiate(foundedLevelObject).GetComponent<LevelObject>();
                }

                levelObject.transform.SetParent(parent);
                levelObject.transform.localScale = Vector3.one;

                return levelObject;
            }

            return null;
        }


        public LevelObject FindLevelObject(int index)
        {
            ContentStorage.LevelObjectItem foundItem = Content.Storage.LevelObjects.Find(i => i.Index == index);

            bool isItemExists = foundItem != null &&
                                foundItem.Link != null &&
                                foundItem.Link.IsAssetExists;

            if (!isItemExists)
            {
                CustomDebug.Log("Level object with index " + index + " don't exists");
                return null;
            }
            else
            {
                return (foundItem.Link.GetAsset() as GameObject).GetComponent<LevelObject>();
            }
        }

        public WeaponType DeafultWeaponType(GameMode mode)
        {
            WeaponType result;

            switch (mode)
            {
                case GameMode.Draw:
                    result = WeaponType.Sniper;
                    break;

                default:
                    result = WeaponType.Sniper;
                    break;
            }

            return result;
        }


        public UiSeasonEventLevelElement CreateUiSeasonEventLevelElement(Transform transform)
        {
            UiSeasonEventLevelElement result = uiSeasonEventLevelElementPool.Pop() as UiSeasonEventLevelElement;
            result.transform.SetParent(transform);
            result.transform.SetAsFirstSibling();
            result.transform.localScale = Vector3.one;
            result.transform.position = Vector3.zero;

            if (result.transform is RectTransform rectTransform)
            {
                rectTransform.anchoredPosition3D = Vector3.zero;
            }

            return result;
        }


        public UiSeasonEventRewardElement CreateUiSeasonEventRewardElement(SeasonEventRewardType seasonEventRewardType, Transform transform)
        {
            ComponentPool componentPool = default;

            switch (seasonEventRewardType)
            {
                case SeasonEventRewardType.Simple:
                    componentPool = uiSeasonEventRewardSimpleElementPool;
                    break;
                case SeasonEventRewardType.Pass:
                    componentPool = uiSeasonEventRewardPassElementPool;
                    break;
                case SeasonEventRewardType.Bonus:
                    componentPool = uiSeasonEventRewardBonusElementPool;
                    break;
                case SeasonEventRewardType.Main:
                    componentPool = uiSeasonEventRewardMainElementPool;
                    break;

                default:
                    CustomDebug.Log("Not Implemented");
                    break;
            }

            UiSeasonEventRewardElement result = componentPool.Pop() as UiSeasonEventRewardElement;
            result.transform.SetParent(transform);
            result.transform.SetAsFirstSibling();
            result.transform.localScale = Vector3.one;
            result.transform.position = Vector3.zero;

            if (result.transform is RectTransform rectTransform)
            {
                rectTransform.anchoredPosition3D = Vector3.zero;
            }
            return result;
        }


        public UiLeagueLeaderBoardElement CreateUiLeagueLeaderBoardElement(Transform transform)
        {
            UiLeagueLeaderBoardElement result = uiLeagueLeaderBoardElementPool.Pop() as UiLeagueLeaderBoardElement;
            result.transform.SetParent(transform);
            result.transform.localScale = Vector3.one;
            result.transform.position = Vector3.zero;

            if (result.transform is RectTransform rectTransform)
            {
                rectTransform.anchoredPosition3D = Vector3.zero;
            }

            return result;
        }


        public UiLeagueLeaderBoardElement CreateUiLeagueEndElement(Transform transform)
        {
            GameObject go = UnityEngine.Object.Instantiate(Content.Storage.PrefabByType(CustomPrefabType.UiLeagueEndBoardElement), transform);
            UiLeagueLeaderBoardElement result = go.GetComponent<UiLeagueLeaderBoardElement>();
            if (result != null)
            {
                result.transform.localScale = Vector3.one;
                result.transform.position = Vector3.zero;

                if (result.transform is RectTransform rectTransform)
                {
                    rectTransform.anchoredPosition3D = Vector3.zero;
                }
            }
            return result;
        }


        public void DestroyUiSeasonEventLevelElement(UiSeasonEventLevelElement component) =>
            uiSeasonEventLevelElementPool.Push(component);


        public void DestroyUiSeasonEventRewardElement(UiSeasonEventRewardElement component)
        {
            ComponentPool componentPool = default;

            switch (component.SeasonEventRewardType)
            {
                case SeasonEventRewardType.Simple:
                    componentPool = uiSeasonEventRewardSimpleElementPool;
                    break;
                case SeasonEventRewardType.Pass:
                    componentPool = uiSeasonEventRewardPassElementPool;
                    break;
                case SeasonEventRewardType.Bonus:
                    componentPool = uiSeasonEventRewardBonusElementPool;
                    break;
                case SeasonEventRewardType.Main:
                    componentPool = uiSeasonEventRewardMainElementPool;
                    break;

                default:
                    CustomDebug.Log("Not Implemented");
                    break;
            }

            componentPool.Push(component);
        }


        public void DestroyUiLeagueLeaderBoardElement(UiLeagueLeaderBoardElement component) =>
            uiLeagueLeaderBoardElementPool.Push(component);

        
        public RopeSegment CreateRopeSegment(Transform transform)
        {
            GameObject result = Object.Instantiate(Content.Storage.PrefabByType(CustomPrefabType.RopeSegment), transform);
            return result.GetComponent<RopeSegment>();
        }

        public Button CreateMansionLock(Transform transform)
        {
            GameObject result = Object.Instantiate(Content.Storage.PrefabByType(CustomPrefabType.MansionLock), transform);
            return result.GetComponent<Button>();
        }


        public Weapon CreateWeapon(WeaponType type, 
                                   Transform weaponTransform,
                                   Transform projectilesSpawnRoot,
                                   int projectilesCount = int.MaxValue)
        {
            Weapon result;

            switch (type)
            {
                case WeaponType.Sniper:
                case WeaponType.BossLauncher:
                    result = new PathShotWeapon(type, projectilesSpawnRoot);
                    break;

                case WeaponType.HitmastersSniper:
                    result = new SingleShotWeapon(type, projectilesSpawnRoot, projectilesCount);
                    break;

                case WeaponType.HitmastersShotgun:                    
                    result = new BoxSprayShotWeapon(type, projectilesCount, projectilesSpawnRoot);
                    break;

                case WeaponType.HitmastersGravitygun:
                    result = new GravitygunWeapon(type, projectilesCount, projectilesSpawnRoot);
                    break;

                case WeaponType.HitmasteresPortalgun:
                    result = new PortalgunWeapon(type, projectilesCount, projectilesSpawnRoot);
                    break;

                case WeaponType.Pet:
                    result = new SingleShotWeapon(type, projectilesSpawnRoot, projectilesCount);
                    break;

                default:
                    CustomDebug.Log($"No weapon logic for type {type}. Created Sniper as default");
                    result = new SingleShotWeapon(WeaponType.Sniper, projectilesSpawnRoot, projectilesCount);
                    break;
            }


            ProjectileType projectileType = IngameData.Settings.modesInfo.GetSettings(result.Type).ProjectileType;
            result.SetupProjectileType(projectileType);
            result.SetupWeaponTransform(weaponTransform);
            return result;
        }
        
        public ShooterAimingDrawer CreateShooterAimDrawer(WeaponType type, Shooter shooter)
        {
            ShooterAimingDrawer result;

            switch (type)
            {
                case WeaponType.Sniper:
                    result = new TrajectoryDrawer(shooter.ColorType, IngameData.Settings.trajectoryDrawSettings);
                    break;
                case WeaponType.HitmastersSniper:
                case WeaponType.HitmastersShotgun:
                case WeaponType.HitmasteresPortalgun:
                    result = new ShooterLineAimingDrawer();
                    break;
                case WeaponType.HitmastersGravitygun:
                    result = new ShooterFxAimingDrawer();
                    break;
                default:
                    CustomDebug.Log($"No aim draw logic for weapon type {type}. Create linear as default");
                    result = new TrajectoryDrawer(shooter.ColorType, IngameData.Settings.trajectoryDrawSettings);
                    break;
            }

            return result;
        }


        public ShooterComponent CreateShooterVfxComponent(WeaponType type)
        {
            ShooterComponent result;

            switch (type)
            {
                case WeaponType.Sniper:
                    result = new ShotVfx();
                    break;

                case WeaponType.HitmasteresPortalgun:
                    result = new PortalShotVfx();
                    break;

                default:
                    CustomDebug.Log($"No aim logic for weapon type {type}. Create ShotVfx as default");
                    result = new ShotVfx();
                    break;
            }

            return result;
        }


        public Projectile CreateProjectile(ProjectileType type, 
                                           WeaponType weaponType, 
                                           ShooterColorType shooterColorType, 
                                           Vector2[] trajectory, 
                                           Transform protectileSpawnRoot,
                                           Vector3? spawnPoint = null)
        {
            if (!ProjectilePools.ContainsKey(type))
            {
                GameObject loadedObject = Content.Storage.PrefabByType(type);
                if (loadedObject == null)
                {
                    CustomDebug.Log($"Projectile with type {type} doesn't exists in content storage");
                    return default;
                }
                Projectile component = loadedObject.GetComponent<Projectile>();
                ComponentPool pool = PoolManager.Instance.GetComponentPool(component);
                ProjectilePools.Add(type, pool);
            }
            Projectile result = ProjectilePools[type].Pop() as Projectile;

            Vector3 projectilePosition;
            if (spawnPoint.HasValue)
            {
                projectilePosition = spawnPoint.Value;
            }
            else
            {
                projectilePosition = trajectory.FirstObject();
            }

            if (result != null)
            {
                Transform cachedTransform = result.transform;
                cachedTransform.rotation = default;
                cachedTransform.position = projectilePosition;
                
                result.MainRigidbody2D.position = projectilePosition;

                result.SetupColorType(shooterColorType);
                result.Initialize(weaponType);

                result.InvokeShotEvent(trajectory);
            }

            return result;
        }


        public PortalObject CreatePortalObject(Transform parent, Vector3 position, float angle, PortalObject.Type type)
        {
            GameObject go = UnityEngine.Object.Instantiate(Content.Storage.PrefabByType(CustomPrefabType.PortalObject));

            PortalObject portal = go.GetComponent<PortalObject>();
            portal.transform.eulerAngles = portal.transform.eulerAngles.SetZ(angle);
            portal.transform.position = position + (portal.transform.up * IngameData.Settings.portalsSettings.portalsCreateOffset);
            portal.transform.SetParent(parent);

            portal.Initialize(type);

            return portal;
        }


        public LineRenderer CreateShotLineRenderer(Transform parent)
        {
            GameObject result = Object.Instantiate(Content.Storage.PrefabByType(CustomPrefabType.ShotLineRenderer), parent);
            return result.GetComponent<LineRenderer>();
        }


        public GravitygunAimVfx CreateGravyShotLineRenderer(Transform parent)
        {
            GameObject result = Object.Instantiate(Content.Storage.PrefabByType(CustomPrefabType.GravyShotLineRenderer), parent);
            return result.GetComponent<GravitygunAimVfx>();
        }

        public LineRenderer CreateShotDirectLineRenderer(Transform parent)
        {
            GameObject result = Object.Instantiate(Content.Storage.PrefabByType(CustomPrefabType.DirectShotLineRenderer), parent);

            return result.GetComponent<LineRenderer>();
        }


        public LineRenderer CreateDefaultLineRenderer(Transform parent = null)
        {
            GameObject lineRendererGo = new GameObject("CustomLineRenderer", typeof(LineRenderer));
            lineRendererGo.transform.SetParent(parent);
            LineRenderer linerenderer = lineRendererGo.GetComponent<LineRenderer>();
            linerenderer.material = new Material(Shader.Find("Sprites/Default"));

            return linerenderer;
        }


        public ShooterAimingDrawer CreateRocketLaunchAimDrawer(ShooterColorType rocketColorType)
        {
            ShooterAimingDrawer result = new TrajectoryDrawer(rocketColorType, IngameData.Settings.rocketLaunchDrawSettings);
            return result;
        }


        public GameObject CreateTrajectoryDot(Transform parent)
            => Object.Instantiate(Content.Storage.PrefabByType(CustomPrefabType.ArcPoint), parent);


        public CornerGraphic CreateCorner(CornerInfo cornerInfo, Transform parent)
        {
            CornerGraphic result = CornersPool.Pop() as CornerGraphic;
            if (result != null)
            {
                result.transform.SetParent(parent);
                result.transform.localPosition = cornerInfo.Position;
                result.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, cornerInfo.Rotation);
                result.Initialize(cornerInfo.CornerSprite, Color.white);
            }
 
            return result;
        }
        
        
        public void DestroyCorner(CornerGraphic corner) =>
            CornersPool.Push(corner);
        
        
        public GameObject CreateObject(GameObject go) => Object.Instantiate(go);

        public T Create<T>(T go, Transform parent) where T : Object =>
            Object.Instantiate(go, parent);


        public void DestroyObject(GameObject go, float time = default)
        {
            if (Mathf.Approximately(time, default))
            {
                Object.Destroy(go);
            }
            else
            {
                Object.Destroy(go, time);
            }
        }

        #endregion



        #region Private methods

        private CustomPrefabType DefineCellPrefabType(StoreItem storeItem)
        {
            CustomPrefabType originalPrefabType = CustomPrefabType.None;

            if (storeItem.IsIdentifierEqualsTo(IAPs.Keys.NonConsumable.NoAdsId))
            {
                originalPrefabType = CustomPrefabType.NoAdsShopCell;
            }
            else
            {
                IapsRewardSettings.RewardData rewardData = IngameData.Settings.iapsRewardSettings.GetRewardData(storeItem.Identifier);

                if (rewardData != null)
                {
                    originalPrefabType = rewardData.shooterSkinReward.skinType == ShooterSkinType.None ?
                        CustomPrefabType.CurrencyBundleShopCell : CustomPrefabType.SkinBundleShopCell;
                }
            }

            if (originalPrefabType == CustomPrefabType.None)
            {
                CustomDebug.Log($"Cannot define prefab typ. Store item : " + storeItem.Identifier);
            }

            return originalPrefabType;
        }

        #endregion
    }
}
