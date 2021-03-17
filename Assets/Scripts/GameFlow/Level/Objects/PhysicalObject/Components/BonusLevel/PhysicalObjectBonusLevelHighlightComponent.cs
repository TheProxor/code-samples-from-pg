using Drawmasters.ServiceUtil;
using UnityEngine;
using ObjectState = Drawmasters.Levels.BonusLevelBehaviourComponent.BonusLevelObjectState;


namespace Drawmasters.Levels
{
    public class PhysicalObjectBonusLevelHighlightComponent : PhysicalObjectHighlightComponent
    {
        #region Fields

        private bool isComponentEnabled;
        private BonusLevelSettings settings;

        #endregion



        #region Properties

        protected override SpriteRenderer Renderer =>
            sourceLevelObject.SpriteRenderer;

        protected override Material OutlineMaterial =>
            settings.outlineMaterial;

        protected override float OutlineWidth =>
            settings.FindOutlideWidth(sourceLevelObject.PhysicalData, sourceLevelObject.CurrentData.bonusData.currencyType);

        protected override Color OutlineColor =>
            settings.outlineColor;

        protected override FactorAnimation OutlineThresholdAnimation =>
            settings.outlineThresholdAnimation;

        #endregion



        #region Abstract implementation

        public override void Initialize(CollisionNotifier notifier, Rigidbody2D rigidbody, PhysicalLevelObject sourceObject)
        {
            base.Initialize(notifier, rigidbody, sourceObject);

            isComponentEnabled = GameServices.Instance.LevelEnvironment.Context.IsBonusLevel &&
                                 sourceObject.PhysicalData.type == PhysicalLevelObjectType.Bonus;

            settings = IngameData.Settings.bonusLevelSettings;
        }


        public override void Enable()
        {
            if (!isComponentEnabled)
            {
                return;
            }

            base.Enable();

            BonusLevelBehaviourComponent.OnStateChanged += BonusLevelBehaviourComponent_OnStateChanged;
        }


        public override void Disable()
        {
            if (!isComponentEnabled)
            {
                return;
            }

            BonusLevelBehaviourComponent.OnStateChanged -= BonusLevelBehaviourComponent_OnStateChanged;

            base.Disable();
        }

        #endregion



        #region Events handlers

        private void BonusLevelBehaviourComponent_OnStateChanged(PhysicalLevelObject anotherObject, ObjectState state)
        {
            if (sourceLevelObject == anotherObject)
            {
                if (state == ObjectState.Stopped)
                {
                    StartHighlighting();
                }
                else if (state == ObjectState.Unstopped || state == ObjectState.Thrown)
                {
                    FinishHighlighting();
                }
            }
        }

        #endregion
    }
}
