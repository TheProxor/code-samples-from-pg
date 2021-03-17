using System;
using Drawmasters.Levels;
using Drawmasters.Notifications;
using Drawmasters.Proposal;
using Modules.General;
using System.Collections.Generic;
using Modules.Notification;

namespace Drawmasters.ServiceUtil
{
    // public static class BackgroundMonitor
    // {
    //     public static event Action<bool> OnBackground; 
    //     
    //     public static void WakeUp(){}
    //     
    //     static BackgroundMonitor()
    //     {
    //         LLApplicationStateRegister.OnApplicationEnteredBackground += 
    //             LLApplicationStateRegister_OnApplicationEnteredBackground;
    //     }
    //
    //     private static void LLApplicationStateRegister_OnApplicationEnteredBackground(bool b)
    //     {
    //         CustomDebug.Log("On monitor");
    //         
    //         OnBackground?.Invoke(b);
    //     }
    // }
    
    public class NotificationService : INotificationService
    {
        #region Fields

        private readonly List<Notification> notifications = new List<Notification>();

        private const string FreeSpinAvailableText = "The free spin is available now. Don't miss your chance!";
        private const string UserMissedDayOneText = "New ways to kill. Try them all!";
        private const string UserMissedDayThreeText = "New level - new victim! Comeback and play!";
        private const string UserMissedDaySevenText = "it’s time to finish what you started. Enemies are waiting!";
        private const string LiveOpsNextStartText = "A new event has already started, come in and enjoy it.";
        private const string LiveOpsFinishSoonText = "Event will finish in 1 day. Get rewards before it’s too late!";

        #endregion



        #region Properties

        // public bool WasQueryShowed =>
        //     CustomPlayerPrefs.HasKey(PrefsKeys.Notifications.WasNotificationAllowPopupShowed);


        private static bool IsNotificationQueryAvailable =>
            // !WasQueryShowed &&
            GameServices.Instance.CommonStatisticService.UniqueLevelsFinishedCount >= 
            GameServices.Instance.AbTestService.CommonData.showAskNotificationsAfterLevel &&
            GameServices.Instance.AbTestService.CommonData.isNotificationsEnabled;

        #endregion



        #region Ctor

        public NotificationService(ILiveOpsFinishSoonNotification monopolyFinishSoonNotification,
                                   ILiveOpsFinishSoonNotification hitmastersFinishSoonNotification,
                                   ILiveOpsFinishSoonNotification seasonEventFinishSoonNotification,
                                   ILiveOpsNextStartNotification monopolyNextStartNotification,
                                   ILiveOpsNextStartNotification hitmastersNextStartNotification,
                                   ILiveOpsNextStartNotification seasonEventNextStartNotification)
        {
            notifications.Add(new FreeSpinNotification(PrefsKeys.Notifications.FreeSpinAvailable, FreeSpinAvailableText));
            notifications.Add(new UserMissingNotification(1, PrefsKeys.Notifications.UserMissedDayOne, UserMissedDayOneText));
            notifications.Add(new UserMissingNotification(3, PrefsKeys.Notifications.UserMissedDayThree, UserMissedDayThreeText));
            notifications.Add(new UserMissingNotification(7, PrefsKeys.Notifications.UserMissedDaySeven, UserMissedDaySevenText));

            notifications.Add(new LiveOpsStartNextNotification(monopolyNextStartNotification, PrefsKeys.Notifications.MonopolyNextLiveOps, LiveOpsNextStartText));
            notifications.Add(new LiveOpsStartNextNotification(hitmastersNextStartNotification, PrefsKeys.Notifications.HitmastersNextLiveOps, LiveOpsNextStartText));
            notifications.Add(new LiveOpsStartNextNotification(seasonEventNextStartNotification, PrefsKeys.Notifications.SeasonEventNextLiveOps, LiveOpsNextStartText));

            notifications.Add(new LiveOpsFinishSoonNotification(monopolyFinishSoonNotification, PrefsKeys.Notifications.MonopolyFinishSoonLiveOps, LiveOpsFinishSoonText));
            notifications.Add(new LiveOpsFinishSoonNotification(hitmastersFinishSoonNotification, PrefsKeys.Notifications.HitmastersFinishSoonLiveOps, LiveOpsFinishSoonText));
            notifications.Add(new LiveOpsFinishSoonNotification(seasonEventFinishSoonNotification, PrefsKeys.Notifications.SeasonEventFinishSoonLiveOps, LiveOpsFinishSoonText));

            // BackgroundMonitor.OnBackground += OnApplicationEnteredBackground;
            LLApplicationStateRegister.OnApplicationEnteredBackground += OnApplicationEnteredBackground;
            Level.OnLevelStateChanged += Level_OnLevelStateChanged;

            // if (!WasQueryShowed)
            {
                NotificationManager.Instance.AreNotificationsEnabled = true; 
            }

            NotificationManager.Instance.Initialize();

            UnregisterAllNotifications();
        }

        #endregion



        #region Methods

        public void RegisterAllNotifications()
        {
            foreach (var notification in notifications)
            {
                if (notification.AllowToRegister)
                {
                    NotificationManager.Instance.RegisterLocalNotification(notification.GenerateData());
                }
            }
        }


        public void UnregisterAllNotifications()
        {
            foreach (var notification in notifications)
            {
                NotificationManager.Instance.UnregisterLocalNotification(notification.Id);
            }
        }


        public void MarkQueryShowed() =>
                    CustomPlayerPrefs.SetBool(PrefsKeys.Notifications.WasNotificationAllowPopupShowed, true);

        #endregion



        #region Events handlers

        protected void OnApplicationEnteredBackground(bool isEnteredBackground)
        {
            if (isEnteredBackground)
            {
                RegisterAllNotifications();
            }
            else
            {
                UnregisterAllNotifications();
            }
        }


        protected void Level_OnLevelStateChanged(LevelState state)
        {
            // if (state == LevelState.Initialized)
            // {
            //     bool isSceneMode = GameServices.Instance.LevelEnvironment.Context.IsSceneMode;
            //     if (!isSceneMode && IsNotificationQueryAvailable)
            //     {
            //         NotificationManager.Instance.AreNotificationsEnabled = true;
            //         MarkQueryShowed();
            //     }
            // }
        }

        #endregion
    }
}
