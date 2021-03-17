namespace Drawmasters.Ui
{
    public static class AnimationKeys
    {
        public static class SkinCard
        {
            public const string Show = "Show";
            public const string Disabled = "Disabled";
        }


        public static class Screen
        {
            public const string Show = "Show";
            public const string Hide = "Hide";
        }


        public static class SpinRouletteScreen
        {
            public const string ChangeSpinButtonsFromFree = "ChangeSpinButton";
            public const string ChangeSpinButtonsToFree = "SpinRouletteChangeButtonsToFree";
        }


        public static class MonopolyScreen
        {
            public const string ChangeRollButtonsFromFree = "ChangeRollButtonsFromFree";
            public const string ChangeRollButtonsToFree = "ChangeRollButtonsToFree";

            public const string RollDiceHide = "RollHide";
            public static readonly string[] RollDice = { "Roll" };
        }


        public static class IngameScreen
        {
            public const string IdleAfterShow = "UiScreenIngameIdle";
        }


        public static class ResultScreen
        {
            public const string CommonIdleBeforeHide = "UiScreenResultIdle";
            public const string CommonToSkinClaim = "CommonToSkinClaim"; 
        }


        public static class RateFeedback
        {
            public const string Switch = "UiPopupRateUsFeedbackSwitch";
            public const string StarPressed = "StarPressed";
        }


        public static class SeasonEvent
        {
            public const string Lock = "Lock";
            public const string Idle = "Idle";
            public const string CanClaim = "CanClaim";

            public const string ShowLockLine = "Show";
            public const string ForceShowLockLine = "ForceShow";
            public const string HideLockLine = "Hide";
            public const string ShortHideLockLine = "ShortHide";
            public const string HidedLockLine = "Hided";

            public const string ShowAnnouncer = "Show";
            public const string BarNextLevel = "NextLevel";
            public const string BarFill = "FillBar";
        }


        public static class TutorialOkButton
        {
            public const string Start = "Start";
            public const string Show = "Show";
            public const string Hide = "Hide";
        }


        public static class TutorialFinger
        {
            public const string Show = "Show";
            public const string Hide = "Hide";
        }


        public static class Common
        {
            public const string Bounce = "Bounce";
        }
    }
}
