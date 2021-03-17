using System;
using System.Collections.Generic;
using Drawmasters.Levels;
using Drawmasters.Levels.Data;
using Drawmasters.ServiceUtil;
using JoystickPlugin;
using UnityEngine;


namespace Drawmasters.Pets
{
    public class PetsInputLevelController : SwitchableLevelController
    {
        #region Fields

        private readonly Dictionary<Pet, PetInput> targets = new Dictionary<Pet, PetInput>();
        private DynamicJoystick petMoveJoystick;

        private Vector2 previousDirection;

        #endregion



        #region Abstract implementation

        protected override bool IsControllerEnabled
        {
            get
            {
                bool result = false;

                LevelContext context = GameServices.Instance.LevelEnvironment.Context;

                if (context != null)
                {
                    result = !context.IsSceneMode;
                }

                return result;
            }
        }

        #endregion



        #region Methods

        public void SetupJoystick(DynamicJoystick _petMoveJoystick)
        {
            AttemptDeinitializeJoystick();

            petMoveJoystick = _petMoveJoystick;

            petMoveJoystick.onPressed.AddListener(ScreenBehaviour_OnJoystickPressed);
        }


        public override void CustomInitialize()
        {
            Level.OnLevelStateChanged += Level_OnLevelStateChanged;
        }


        public override void CustomDeinitialize()
        {
            Level.OnLevelStateChanged -= Level_OnLevelStateChanged;

            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;
            AttemptDeinitializeJoystick();

            foreach (var data in targets)
            {
                data.Value.Deinitialize();
            }

            targets.Clear();
        }


        public void Add(Pet target)
        {
            if (!IsControllerEnabled)
            {
                return;
            }

            PetInput inputToAdd = new PetInput();
            inputToAdd.Initialize();

            target.SetInputModule(inputToAdd);
            targets.Add(target, inputToAdd);
        }


        public void Remove(Pet target)
        {
            if (!IsControllerEnabled)
            {
                return;
            }

            TryPerfomActionForInput(target, e => e.Deinitialize());
            targets.Remove(target);
        }


        private void PerfomActionForAllTargets(Action<Pet, PetInput> action)
        {
            foreach (var target in targets)
            {
                action?.Invoke(target.Key, target.Value);
            }
        }


        private void AttemptDeinitializeJoystick()
        {
            if (petMoveJoystick != null)
            {
                petMoveJoystick.onPressed.RemoveListener(ScreenBehaviour_OnJoystickPressed);
                petMoveJoystick.onPressedUp.RemoveListener(ScreenBehaviour_OnJoystickPressedUp);
            }
        }

        private bool TryPerfomActionForInput(Pet target, Action<PetInput> action)
        {
            if (targets.TryGetValue(target, out PetInput input))
            {
                action?.Invoke(input);
                return true;
            }

            return false;
        }

        #endregion



        #region Events handlers

        private void ScreenBehaviour_OnJoystickPressed()
        {
            PerfomActionForAllTargets((target, input) => input.InvokeOnStartDraw(default));

            petMoveJoystick.onPressedUp.AddListener(ScreenBehaviour_OnJoystickPressedUp);
            MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdate;
        }


        private void MonoBehaviourLifecycle_OnUpdate(float deltaTime)
        {
            Vector2 direction = petMoveJoystick.Direction;

            if (Vector2.Distance(previousDirection, direction) >= IngameData.Settings.pets.levelSettings.minDistanceBetweenPoints)
            {
                PerfomActionForAllTargets((target, input) => input.InvokeOnDraw(direction, deltaTime));
                previousDirection = direction;
            }
        }



        private void ScreenBehaviour_OnJoystickPressedUp()
        {
            petMoveJoystick.onPressedUp.RemoveListener(ScreenBehaviour_OnJoystickPressedUp);
            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;

            PerfomActionForAllTargets((target, input) =>
            {
                input.InvokeOnFinishDraw(true, default);
            });
        }


        // TODO: temp. just for test. From vladislav.K : copypasted from shooters input controller.
        private void Level_OnLevelStateChanged(LevelState state)
        {
            if (state == LevelState.AllTargetsHitted)
            {
                Deinitialize();
            }
        }

        #endregion
    }
}
