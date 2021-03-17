using UnityEngine;


namespace Drawmasters.Proposal.Ui
{
    public class UiLiveOpsEvent : MonoBehaviour, IDeinitializable
    {
        #region Fields

        [SerializeField] private GameObject root = default;

        protected LiveOpsEventController controller;

        private bool allowForcePropose;

        #endregion



        #region Methods

        public virtual void Initialize(LiveOpsEventController _controller)
        {
            controller = _controller;

            controller.OnStarted += RefreshVisual;
            controller.OnFinished += RefreshVisual;
        }


        public virtual void Deinitialize()
        {
            // hotfix cuz of first deinit in main menu behaviour
            if (controller != null) 
            {
                controller.OnStarted -= RefreshVisual;
                controller.OnFinished -= RefreshVisual;
            }
        }


        public void SetForceProposePlaceAllowed(bool _allowForcePropose)
        {
            allowForcePropose = _allowForcePropose;
            RefreshVisual();
        }


        public void ForcePropose()
        {
            if (!allowForcePropose ||
                !controller.CanForcePropose)
            {
                return;
            }

            OnForcePropose();
            
            controller.MarkForceProposed();
        }

        protected virtual void OnForcePropose() { }


        private void RefreshVisual()
        {
            bool isConsiderAsProposed = allowForcePropose || (!allowForcePropose && controller.WasForceShow);
            bool shouldShowRoot = isConsiderAsProposed && controller.IsActive;
            CommonUtility.SetObjectActive(root, shouldShowRoot);
        }

        #endregion
    }
}
