using System;
using System.Collections.Generic;
using UnityEngine;
using Drawmasters.ServiceUtil;
using Spine.Unity;
using Drawmasters.Pets;
using Object = UnityEngine.Object;


namespace Drawmasters.Levels
{
    public class Pet : IDeinitializable
    {
        #region Fields

        public event Action OnPreparePetSkinChange;
        public event Action<PetSkinType> OnPetSkinChange;
        public event Action OnPostPetSkinChange;


        private PetSkinLink currentSkinLink;
        private Transform rootTransform;
        private List<PetComponent> petComponents;
        private Shooter shooter;

        #endregion



        #region Properties

        public PetInput Input { get; private set; }

        public bool IsPetExists => Type != PetSkinType.None; 

        public virtual PetSkinType Type { get; set; }

        public PetSkinLink CurrentSkinLink => currentSkinLink;
        
        public Shooter Shooter => shooter;
        
        public ShooterColorType ColorType { get; private set; }

        public SkeletonAnimation SkeletonAnimation => 
            currentSkinLink == null ? null : currentSkinLink.SkeletonAnimation;
        
        protected virtual List<PetComponent> CoreComponents
        {
            get
            {
                var result = new List<PetComponent>() { };

                result.AddRange(new List<PetComponent>()
                {
                    new PetSkinComponent(),
                    new PetAnimationComponent(),
                    new PetFxComponent(),
                    new PetInvokeComponent(),
                    new PetMoveComponent(),
                    new PetShootComponent(),
                    new PetAimComponent(),
                    new PetTrailComponent(),
                    new PetCurrencyAbsorbComponent()
                });

                return result;
            }
        }

        #endregion



        #region Methods

        public void SetInputModule(PetInput inputToAdd) =>
            Input = inputToAdd;


        public void PetSkinChange(PetSkinType skinType) =>
            OnPetSkinChange?.Invoke(skinType);
        
        
        public void SetupColorType(ShooterColorType _colorType) =>
            ColorType = _colorType;


        public void Initialize(Shooter _shooter, Transform _rootTransform)
        {
            shooter = _shooter;
            rootTransform = _rootTransform;
            Type = GameServices.Instance.PlayerStatisticService.PlayerData.CurrentPetSkin;
            petComponents = CoreComponents;
            
            if (GameServices.Instance.LevelEnvironment.Context.IsBonusLevel)
            {
                petComponents.Add(new PetBonusLevelComponent());
            }

            GameServices.Instance.PlayerStatisticService.PlayerData.OnPetSkinSetted += OnSkinChanged;

            InitializeComponents();
        }
        
        public void Deinitialize()
        {
            GameServices.Instance.PlayerStatisticService.PlayerData.OnPetSkinSetted -= OnSkinChanged;
            DeinitializeComponents();
            ClearSkinLink();
        }

        private void InitializeComponents()
        {
            foreach (var component in petComponents)
            {
                component.Initialize(this);
            }
        }


        private void DeinitializeComponents()
        {
            foreach (var component in petComponents)
            {
                component.Deinitialize();
            }
        }
        
        public void RefreshSkin(PetSkinType type, Action callback) =>
            RefreshSkinLink(type, callback);
        

        private void ClearSkinLink()
        {
            if (currentSkinLink != null)
            {
                Object.Destroy(currentSkinLink.gameObject);
                currentSkinLink = null;
            }
        }
        
        private void RefreshSkinLink(PetSkinType type, Action callback)
        {
            Type = type;
            if (!IsPetExists)
            {
                callback?.Invoke();
                OnPostPetSkinChange?.Invoke();
                ClearSkinLink();
                return;
            }
            
            PetSkinLink link = IngameData.Settings.pets.skinsSettings.petLink;

            // TODO: Actually, we don't have to reinstantiate every time we change type, we can just reset data
            bool isLinksEquals = currentSkinLink != null && Type == currentSkinLink.SkinType;
            
            if (link == null || isLinksEquals)
            {
                SetupPosition();
                callback?.Invoke();
            }
            else
            {
                ClearSkinLink();
                currentSkinLink = Object.Instantiate(link, rootTransform);
                currentSkinLink.transform.SetParent(rootTransform.parent);
                SetupPosition();

                callback?.Invoke();
                OnPostPetSkinChange?.Invoke();
            }

            void SetupPosition()
            {
                if (currentSkinLink != null)
                {
                    PetSkinsSettings petSkinsSettings = IngameData.Settings.pets.skinsSettings;
                    Vector3 petPosition = rootTransform.position + petSkinsSettings.FindTargetPosition(Type);

                    currentSkinLink.transform.SetPositionAndRotation(petPosition, Quaternion.identity);
                }
            }
        }

        #endregion
        
        
        
        #region Events handlers
        
        private void OnSkinChanged() =>
            OnPreparePetSkinChange?.Invoke();
          
        #endregion

    }
}