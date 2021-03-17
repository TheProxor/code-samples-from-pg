using System;
using UnityEngine;
using Drawmasters.Ui;


namespace Drawmasters.Proposal
{
    [CreateAssetMenu(fileName = "MonopolyVisualSettings",
                  menuName = NamingUtility.MenuItems.ProposalSettings + "MonopolyVisualSettings")]
    public class MonopolyVisualSettings : ScriptableObject
    {
        #region Nested types

        [Serializable]
        public class RollData
        {
            public int rollNumber = default;
            public Vector3 diceNumbersRotation = default;
            public Sprite announcerSprite = default;
        }

        [Serializable]
        public class DeskCurrencyIconData
        {
            public MonopolyDeskElement.ColorType type = default;
            public CurrencySkinsIconData[] data = default;
        }

        [Serializable]
        public class ReloadTime
        {
            public int day = default;
            public int hours = default;
            public int minutes = default;


            public float Seconds
            {
                get
                {
                    DateTime now = DateTime.Now;
                    DateTime nextDateTime = DateTime.Now.AddDays(day).Date + LiveOpsReloadTime;
                    return (float)nextDateTime.Subtract(now).TotalSeconds;
                }
            }

            private TimeSpan LiveOpsReloadTime =>
                new TimeSpan(hours, minutes, 0);

        }

        #endregion



        #region Fields

        public FactorAnimation materialBlendAnimation = default;

        [Header("Spin Roulette Common Settings")]

        public float characterStartMoveDelay = default;
        public float delayBetweenSteps = default;
        public float delayBetweenStepsNewLaps = default;
        public float autoRolldelays = default;

        [Header("Visual")]
        [SerializeField] private DeskCurrencyIconData[] deskData = default;

        [SerializeField] private CurrencySkinsIconData[] lapsCurrencyIconData = default;
        [SerializeField] private ShooterSkinsIconData[] lapsShooterSkinsIconData = default;
        [SerializeField] private WeaponSkinsIconData[] lapsWeaponSkinsIconData = default;

        [Header("Animation")]
        public ColorAnimation materialTextOutlineAnimation = default;
        public ColorAnimation materialTextAnimation = default;
        public VectorAnimation laspScaleAnimation = default;
        public float lapsFillDuration = default;

        public VectorAnimation deskElementBumpAnimation = default;

        public VectorAnimation menuShowMoveAnimation = default;

        public VectorAnimation numberShowScaleAnimation = default;
        public FactorAnimation numberShowAlphaAnimation = default;

        [Header("Timer")]
        public double secondsRestToStartAnimateTimer = default;
        public VectorAnimation timerBounceAnimation = default;

        [Header("Dice")]
        public RollData[] rollData = default;

        [Header("Character")]
        public AnimationCurve characterMoveHorizontalCurveY = default;
        public AnimationCurve characterMoveHorizontalCurveX = default;

        public AnimationCurve characterMoveVerticalCurveY = default;
        public AnimationCurve characterMoveVerticalCurveX = default;
        public AnimationCurve characterMoveVerticalCurveYUp = default;

        public VectorAnimation characterMoveAnimation = default;

        public float lastRewardApplyDelay = default;

        [Header("Tutorial")]
        public float tutorialInactivityDelay = default;
        public Vector3 tutorialTapZonePosition = default;
        public Vector2 tutorialTapZoneSize = default;

        [Header("Localization")]
        public string lapsKey = string.Empty;

        #endregion



        #region Methods

        public AnimationCurve FindCharacterCurveX(MonopolyCharacter.JumpDirection jumpDirection)
        {
            AnimationCurve curve;

            switch (jumpDirection)
            {
                case MonopolyCharacter.JumpDirection.Horizontal:
                    curve = characterMoveHorizontalCurveX;
                    break;

                case MonopolyCharacter.JumpDirection.Vectical:
                    curve = characterMoveVerticalCurveX;
                    break;

                default:
                    curve = characterMoveVerticalCurveX;
                    break;
            }

            return curve;
        }


        public AnimationCurve FindCharacterCurveY(MonopolyCharacter.JumpDirection jumpDirection)
        {
            AnimationCurve curve;

            switch (jumpDirection)
            {
                case MonopolyCharacter.JumpDirection.Horizontal:
                    curve = characterMoveHorizontalCurveY;
                    break;

                case MonopolyCharacter.JumpDirection.Vectical:
                    curve = characterMoveVerticalCurveYUp;
                    break;

                default:
                    curve = characterMoveVerticalCurveY;
                    break;
            }

            return curve;
        }


        public Sprite FindRollAnnouncer(int number)
        {
            RollData foundData = FindRollData(number);
            return foundData == null ? default : foundData.announcerSprite;
        }


        public Vector3 FindDiceNumbersRotation(int number)
        {
            RollData foundData = FindRollData(number);
            return foundData == null ? default : foundData.diceNumbersRotation;
        }


        public Sprite FindLapsCurrencyActiveIcon(CurrencyType type)
        {
            CurrencySkinsIconData foundData = FindCurrencyLapsIconData(type);
            return foundData == null ? default : foundData.activeIcon;
        }


        public Texture FindLapsCurrencyClaimedIcon(CurrencyType type)
        {
            CurrencySkinsIconData foundData = FindCurrencyLapsIconData(type);
            return foundData == null || foundData.claimedIcon == null ? default : foundData.claimedIcon.texture;
        }


        public Sprite FindDeskCurrencyActiveIcon(MonopolyDeskElement.ColorType elementType, CurrencyType type)
        {
            CurrencySkinsIconData foundData = FindCurrencyDeskIconData(elementType, type);
            return foundData == null ? default : foundData.activeIcon;
        }


        public Texture FindDeskCurrencyClaimedIcon(MonopolyDeskElement.ColorType elementType, CurrencyType type)
        {
            CurrencySkinsIconData foundData = FindCurrencyDeskIconData(elementType, type);
            return foundData == null || foundData.claimedIcon == null ? default : foundData.claimedIcon.texture;
        }


        public Sprite FindLapsShooterSkinActiveIcon(ShooterSkinType type)
        {
            ShooterSkinsIconData foundData = FindShooterLapsSkinsIconData(type);
            return foundData == null ? default : foundData.activeIcon;
        }


        public Texture FindLapsShooterSkinClaimedIcon(ShooterSkinType type)
        {
            ShooterSkinsIconData foundData = FindShooterLapsSkinsIconData(type);
            return foundData == null || foundData.claimedIcon == null ? default : foundData.claimedIcon.texture;
        }


        public Sprite FindLapsWeaponSkinActiveIcon(WeaponSkinType type)
        {
            WeaponSkinsIconData foundData = FindWeaponLapsSkinsIconData(type);
            return foundData == null ? default : foundData.activeIcon;
        }


        public Texture FindLapsWeaponSkinClaimedIcon(WeaponSkinType type)
        {
            WeaponSkinsIconData foundData = FindWeaponLapsSkinsIconData(type);
            return foundData == null || foundData.claimedIcon == null ? default : foundData.claimedIcon.texture;
        }


        private CurrencySkinsIconData FindCurrencyDeskIconData(MonopolyDeskElement.ColorType elementType, CurrencyType type)
        {
            CurrencySkinsIconData result = default;
            DeskCurrencyIconData foundData = Array.Find(deskData, e => e.type == elementType);
            AssertLog(foundData == null, $"No data found for type {elementType} in {this}");

            if (foundData != null && foundData.data != null)
            {
                result  = Array.Find(foundData.data, e => e.type == type);
            }

            AssertLog(foundData == null, $"No data found for type {type} in {this}");

            return result;
        }


        private CurrencySkinsIconData FindCurrencyLapsIconData(CurrencyType type)
        {
            CurrencySkinsIconData foundData = Array.Find(lapsCurrencyIconData, e => e.type == type);
            AssertLog(foundData == null, $"No data found for type {type} in {this}");

            return foundData;
        }


        private WeaponSkinsIconData FindWeaponLapsSkinsIconData(WeaponSkinType type)
        {
            WeaponSkinsIconData foundData = Array.Find(lapsWeaponSkinsIconData, e => e.type == type);
            AssertLog(foundData == null, $"No data found for type {type} in {this}");

            return foundData;
        }


        private ShooterSkinsIconData FindShooterLapsSkinsIconData(ShooterSkinType type)
        {
            ShooterSkinsIconData foundData = Array.Find(lapsShooterSkinsIconData, e => e.type == type);
            AssertLog(foundData == null, $"No data found for type {type} in {this}");

            return foundData;
        }

        private RollData FindRollData(int number)
        {
            RollData foundData = Array.Find(rollData, e => e.rollNumber == number);
            AssertLog(foundData == null, $"No data found for number {number} in {this}");

            return foundData;
        }


        private void AssertLog(bool assertCondition, string log)
        {
            if (assertCondition)
            {
                CustomDebug.Log(log);
            }
        }

        #endregion
    }
}
