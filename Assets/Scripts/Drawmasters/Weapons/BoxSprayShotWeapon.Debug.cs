using UnityEngine;


namespace Drawmasters.Levels
{
    public partial class BoxSprayShotWeapon
    {
        #if UNITY_EDITOR

        #region Fields

        private Transform sprayDrawTransform;

        private readonly HitmastersShotgunSettings shotgunModeSettings;

        #endregion



        #region Methods

        public void SetupSprayDrawTransform(Transform sprayDrawTransformValue)
        {
            sprayDrawTransform = sprayDrawTransformValue;
        }


        private void DebugSubscribeToEvents()
        {
            //MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdate;
        }


        private void DebugUnsubscribeFromEvents()
        {
            //MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;
        }

        #endregion



        #region Events handlers

        private void MonoBehaviourLifecycle_OnUpdate(float deltatime)
        {
            spawnWidth = shotgunModeSettings.spawnWidth;

            Transform positionStartDrawTransform = (sprayDrawTransform == null) ? weaponTransform : sprayDrawTransform;

            // cuz of rotation shooter by Y
            Vector3 weaponRight = (Mathf.Sign(weaponTransform.right.y) < 0) ? weaponTransform.right : weaponTransform.right.SetY(-weaponTransform.right.y);
            Vector3 WidthDirection = weaponRight * spawnWidth;

            Debug.DrawRay(positionStartDrawTransform.position - (WidthDirection / 2), WidthDirection, Color.green);
        }

        #endregion

        #endif
    }
}
