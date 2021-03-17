using Modules.Sound;
using Drawmasters.Ui.Mansion;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.Ui
{
    public class ProposalAnimationHandler : MonoBehaviour
    {
        #region Constants

        private const string ShowProposals = "Show";

        private const string HideProposals = "Hide";

        #endregion



        #region Fields

        [SerializeField] private Animator mainAnimator = default;

        private List<MansionRoom> rooms;
        private UiMansionSwipe swipe;

        private UiMansionHammerPropose uiHammerPropose;
        private UiMansionRewardPropose uiMansionRewardPropose;

        private Guid musicGuid;
        private string prevSfxKey;

        #endregion



        #region Public methods

        public void Initialize(List<MansionRoom> _rooms,
                               UiMansionSwipe _swipe,
                               UiMansionHammerPropose _uiHammerPropose,
                               UiMansionRewardPropose _uiMansionRewardPropose)
        {
            rooms = _rooms;
            swipe = _swipe;

            uiHammerPropose = _uiHammerPropose;
            uiMansionRewardPropose = _uiMansionRewardPropose;

            foreach (var i in rooms)
            {
                i.OnOpened += I_OnOpened;
                i.OnCompleted += I_OnCompleted;
            }

            swipe.OnSwipeBegin += Swipe_OnSwipeBegin;
            swipe.OnSwipeEnd += Swipe_OnSwipeEnd;

            uiMansionRewardPropose.SetEnabled(false);
            uiHammerPropose.SetEnabled(false);

            RefreshMusic();
        }


        public void Deinitialize()
        {
            foreach (var i in rooms)
            {
                i.OnOpened -= I_OnOpened;
                i.OnCompleted -= I_OnCompleted;
            }

            swipe.OnSwipeBegin -= Swipe_OnSwipeBegin;
            swipe.OnSwipeEnd -= Swipe_OnSwipeEnd;

            UnsubscribeFromForceShowEvents();

            SoundManager.Instance.StopSound(musicGuid);
        }


        private void UnsubscribeFromForceShowEvents()
        {
        }

        #endregion



        #region Private methods

        private void EnableProposals()
        {
            MansionRoom room = rooms[swipe.CurrentScreen];
            bool isRoomOpen = room.IsOpen;
            bool isRoomCompleted = room.IsCompleted;

            uiMansionRewardPropose.SetEnabled(isRoomOpen && isRoomCompleted);
            uiHammerPropose.SetEnabled(isRoomOpen && !isRoomCompleted);

            uiMansionRewardPropose.SetupRoom(swipe.CurrentScreen);

            mainAnimator.SetTrigger(ShowProposals);
        }


        private void DisableProposals()
        {
            mainAnimator.SetTrigger(HideProposals);
        }

        #endregion



        #region Events handlers

        private void Swipe_OnSwipeBegin()
        {
            DisableProposals();
        }


        private void Swipe_OnSwipeEnd()
        {
            RefreshMusic();

            MansionRoom roomToPlayAnimation = rooms[swipe.CurrentScreen];

            if (roomToPlayAnimation.ShouldHardScroll)
            {
                roomToPlayAnimation.PlayOpenRoomAnimation();
                roomToPlayAnimation.ShouldHardScroll = false;
            }

            EnableProposals();
        }


        private void I_OnOpened()
        {
            RefreshMusic();
            EnableProposals();
        }


        private void I_OnCompleted()
        {
            EnableProposals();
        }


        private void RefreshMusic()
        {
            MansionRoom currentRoom = rooms[swipe.CurrentScreen];

            string sfxKey;

            if (currentRoom.IsOpen)
            {
                sfxKey = currentRoom.MusicKey;
            }
            else
            {
                sfxKey = AudioKeys.Music.MUSIC_MENU;
            }

            if (prevSfxKey == sfxKey)
            {
                return;
            }

            musicGuid = SoundManager.Instance.PlaySound(sfxKey, isLooping: true);
            prevSfxKey = sfxKey;
        }


        #endregion
    }
}


