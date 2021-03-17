using UnityEngine;

namespace Drawmasters.Ui
{
    public class RoomAnimationHandler
    {
        #region Constants

        private const string Open = "Open";

        private const string Complete = "Complete";

        private const string Opened = "Opened";

        private const string Completed = "Completed";

        #endregion



        #region Fields

        private readonly Animator mainAnimator;

        #endregion



        #region Ctor

        public RoomAnimationHandler(Animator _mainAnimator)
        {
            mainAnimator = _mainAnimator;
        }

        #endregion



        #region Public methods

        public void PlayOpenRoomAnimation()
        {
            mainAnimator.SetTrigger(Open);
        }


        public void SetOpenedState()
        {
            mainAnimator.SetTrigger(Opened);
        }


        public void SetCompletedState()
        {
            mainAnimator.SetTrigger(Completed);
        }


        public void PlayCompleteRoomAnimation()
        {
            mainAnimator.SetTrigger(Complete);
        }

        #endregion
    }
}

