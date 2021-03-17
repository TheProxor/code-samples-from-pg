using UnityEngine;
using Drawmasters.Utils.Ui;


namespace Drawmasters.Ui
{
    public class UiLeagueLeaderBoardElementScrollLocker : IInitializable, IDeinitializable
    {
        #region Fields

        private readonly IScrollHelper scrollHelper;

        private readonly float lockBottomPositionY;
        private readonly float lockTopPositionY;

        private UiLeagueLeaderBoardElement elementToLock;
        private RectTransform elementToLockRectTransform;

        private UiLeagueLeaderBoardElement lockedElement;

        #endregion



        #region Class lifecycle

        public UiLeagueLeaderBoardElementScrollLocker(UiLeagueLeaderBoardElement _elementToLock, IScrollHelper _scrollHelper)
        {
            SetupElementToMonitor(_elementToLock);
            scrollHelper = _scrollHelper;

            lockBottomPositionY = -scrollHelper.ViewportRectTransform.rect.height + (elementToLockRectTransform.rect.height * 0.5f);
            lockTopPositionY = -elementToLockRectTransform.rect.height * 0.5f;
        }

        #endregion



        #region Methods

        public void SetupElementToMonitor(UiLeagueLeaderBoardElement _elementToLock)
        {
            elementToLock = _elementToLock;
            elementToLockRectTransform = elementToLock.transform as RectTransform;
        }


        public void Initialize()
        {
            scrollHelper.AddOnValueChangedCallback(OnScrollValueChanged);
        }


        public void Deinitialize()
        {
            FinishChecking();
            scrollHelper.RemoveOnValueChangedCallback(OnScrollValueChanged);

            DestroyLockedElement();
        }


        public void StartChecking() =>
            MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdate;
        

        public void FinishChecking() =>
            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;
        
        #endregion



        #region Events handlers

        private void MonoBehaviourLifecycle_OnUpdate(float delta) =>
            RefreshLocking();


        private void OnScrollValueChanged(Vector2 value) =>
            RefreshLocking();


        public void RefreshLocking()
        {
            Vector3 elementAnchoredPosition = scrollHelper.GetElementViewPosition(elementToLockRectTransform);

            bool allowBottomLock = elementAnchoredPosition.y < lockBottomPositionY;
            bool allowTopLock = elementAnchoredPosition.y > lockTopPositionY;
            bool shouldLockElement = allowBottomLock || allowTopLock;

            if (shouldLockElement)
            {
                if (lockedElement != null)
                {
                    return;
                }

                lockedElement = Content.Management.CreateUiLeagueLeaderBoardElement(scrollHelper.ViewportRectTransform);
                lockedElement.CopyFrom(elementToLock);
                lockedElement.Initialize();

                if (allowTopLock)
                {
                    lockedElement.EnableTopMask();
                }
                else
                {
                    lockedElement.EnableBottomMask();
                }

                float lockedElementPositionY = allowBottomLock ? lockBottomPositionY : lockTopPositionY;
                Vector3 lockedElementPosition = elementAnchoredPosition.SetY(lockedElementPositionY);

                lockedElement.RectTransform.anchorMin = elementToLock.RectTransform.anchorMin;
                lockedElement.RectTransform.anchorMax = elementToLock.RectTransform.anchorMax;
                lockedElement.RectTransform.anchoredPosition3D = lockedElementPosition;
            }
            else
            {
                DestroyLockedElement();
            }
        }


        private void DestroyLockedElement()
        {
            if (lockedElement == null)
            {
                return;
            }

            lockedElement.Deinitialize();
            Content.Management.DestroyUiLeagueLeaderBoardElement(lockedElement);
            lockedElement = null;
        }

        #endregion
    }
}
