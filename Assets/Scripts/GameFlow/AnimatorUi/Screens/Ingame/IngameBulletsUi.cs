using System;
using UnityEngine;
using System.Collections.Generic;
using Drawmasters.ServiceUtil;
using Modules.General;

namespace Drawmasters.Levels
{
    public class IngameBulletsUi : MonoBehaviour
    {
        [Serializable]
        public class Data
        {
            public WeaponType weaponType = default;
            public Sprite enabledSprite = default;
            public Sprite disabledSprite = default;
            public Color disabledColor = default;
        }


        public class Counter : IDeinitializable
        {
            #region Fields
            
            private readonly List<BulletUi> bullets;

            private LevelStageController stageController;

            #endregion



            #region Lifecycle

            public Counter(List<BulletUi> _bullets)
            {
                bullets = new List<BulletUi>(_bullets);

                stageController = GameServices.Instance.LevelControllerService.Stage;
            }

            #endregion



            #region IInitializable

            public void Initialize()
            {
                foreach (var i in bullets)
                {
                    i.Initialize();
                }

                int projectilesCount = GameServices.Instance.LevelEnvironment.Context.ProjectilesCount;

                Weapon_OnProjectilesCountChange(projectilesCount, projectilesCount);

                Weapon.OnProjectilesCountChange += Weapon_OnProjectilesCountChange;

                stageController.OnFinishChangeStage += LevelStageController_OnFinishChangeStage;

            }


            public void InitializeVisual(Data data)
            {
                foreach (var i in bullets)
                {
                    i.InitializeVisual(data);
                }
            }


            public void InitializeAnimation(VectorAnimation scaleInAnimation,
                                            VectorAnimation scaleOutAnimation)
            {
                foreach (var i in bullets)
                {
                    i.InitializeAnimation(scaleInAnimation, scaleOutAnimation);
                }
            }


            #endregion



            #region Deinitializable

            public void Deinitialize()
            {
                foreach (var i in bullets)
                {
                    i.Deinitialize();
                }

                Weapon.OnProjectilesCountChange -= Weapon_OnProjectilesCountChange;

                stageController.OnFinishChangeStage -= LevelStageController_OnFinishChangeStage;

                Scheduler.Instance.UnscheduleAllMethodForTarget(this);
            }

            #endregion
            
            

            #region Events handlers

            private void LevelStageController_OnFinishChangeStage()
            {
                foreach (var i in bullets)
                {
                    i.Deinitialize();
                    i.Initialize();
                }

                int count = GameServices.Instance.LevelEnvironment.Context.ProjectilesCount;
                Weapon_OnProjectilesCountChange(count, count);
            }


            private void Weapon_OnProjectilesCountChange(int changedCount, int delta)
            {
                bullets.ForEach(bullet => bullet.CompleteAnimation());

                bool isAdding = (delta > 0);
                if (isAdding)
                {
                    for (int i = 0; i < bullets.Count; i++)
                    {
                        bool isDiabled = (i >= delta);

                        if (isDiabled)
                        {
                            bullets[i].Disable();
                        }
                        else
                        {
                            bullets[i].Initialize();
                        }
                    }
                }
                else if (delta < 0)
                {
                    bool isValid = (0 <= changedCount) && (changedCount < bullets.Count);

                    if (isValid)
                    {
                        BulletUi removeBullet = bullets[changedCount];

                        removeBullet.ChangeState(false);
                    }
                }
            }

            #endregion
        }
    }
}
