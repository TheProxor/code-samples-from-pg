using UnityEngine;
using UnityEngine.UI;
using Drawmasters.Proposal;
using Drawmasters.Helpers;
using Drawmasters.Utils;
using Drawmasters.Proposal.Helpers;
using Drawmasters.ServiceUtil;
using Drawmasters.Proposal.Interfaces;
using TMPro;
using System.Linq;
using RewardRoot = Drawmasters.Ui.UiLeagueLeaderBoardRewardsRootElement;
using SelectorKey = Drawmasters.Ui.UiLeagueLeaderBoardElementSelectorKey;


namespace Drawmasters.Ui
{
    public class UiLeagueLeaderBoardElement : MonoBehaviour, IInitializable, IDeinitializable
    {
        #region Fields

        [SerializeField] private LayoutElement layoutElement = default;

        [SerializeField] private UiLeagueLeaderBoardElementSelectorImage[] selectorImages = default;
        [SerializeField] private UiLeagueLeaderBoardElementSelectorOutline[] selectorOutlines = default;
        [SerializeField] private UiLeagueLeaderBoardElementSelectorColor[] selectorColors = default;

        [Header("Place Data")]
        [SerializeField] private TMP_Text numberText = default;

        [SerializeField] private TMP_Text numberTopText = default;
        [SerializeField] private Image numberTopImage = default;

        [Header("Icons")]
        [SerializeField] private Image shooterImage = default;
        [SerializeField] private TMP_Text nickNameText = default;

        [SerializeField] private Image leagueIconImage = default;
        [SerializeField] private GameObject leagueIconArrowRoot = default;

        [SerializeField] private TMP_Text skullsCountText = default;

        [Header("EndLeague")]
        [SerializeField] private RewardRoot endLeaguePetRewardRoot = default;

        [Header("Locked settings")]
        [SerializeField] private RectTransform mask = default;
        [SerializeField] private float maskOffset = default;

        [Header("Helpers")]
        [SerializeField] private RewardRoot[] roots = default;

        private LeagueProposeController controller;

        private LeagueType leagueType;
        private bool isNextLeagueAchived;

        #endregion



        #region Properties

        public string NickName { get; private set; }

        public int Number { get; private set; }

        public LeaderBordItemType CurrentOwnerType { get; private set; }

        public ShooterSkinType ShooterSkinType { get; private set; }

        public float SkullsCount { get; private set; }

        public RectTransform RectTransform { get; private set; }

        public LayoutElement LayoutElement => layoutElement;

        #endregion



        #region Methods

        public void Initialize()
        {
            RectTransform = transform as RectTransform;
            RectTransform.localScale = Vector3.one;

            CommonUtility.SetObjectActive(mask.gameObject, false);
            CommonUtility.SetObjectActive(endLeaguePetRewardRoot.gameObject, endLeaguePetRewardRoot.RewardData != null);
        }


        public void Deinitialize()
        {
            foreach (var root in roots)
            {
                root.Deintiailize();
            }
        }


        public void EnableTopMask()
        {
            CommonUtility.SetObjectActive(mask.gameObject, true);

            float offsetY = (layoutElement.preferredHeight - mask.rect.height) * 0.5f + maskOffset;
            mask.anchoredPosition = mask.anchoredPosition.SetY(offsetY);
            mask.eulerAngles = mask.eulerAngles.SetZ(180.0f);
        }


        public void EnableBottomMask()
        {
            CommonUtility.SetObjectActive(mask.gameObject, true);

            float offsetY = (-layoutElement.preferredHeight + mask.rect.height) * 0.5f - maskOffset;
            mask.anchoredPosition = mask.anchoredPosition.SetY(offsetY);
            mask.eulerAngles = mask.eulerAngles.SetZ(0.0f);
        }


        public void CopyFrom(UiLeagueLeaderBoardElement elementToLock)
        {
            SetupController(elementToLock.controller);

            SetupNumber(elementToLock.Number);
            SetupNickName(elementToLock.NickName);
            SetupSkullsCount(elementToLock.SkullsCount);
            SetupOwnerType(elementToLock.CurrentOwnerType);
            SetupShooterType(elementToLock.ShooterSkinType);
            SetupLeagueType(elementToLock.leagueType);
            SetupNextLeagueAchived(elementToLock.isNextLeagueAchived);
            SetupRewardVisual(elementToLock.leagueType, elementToLock.Number - 1, elementToLock.controller.CurrentLiveOpsId);
            RefreshVisual();
        }


        public void SetupNumber(int i)
        {
            Number = i;
            numberText.text = Number.ToString();
            numberTopText.text = Number.ToString();

            bool isPlaceDataExists = controller.VisualSettings.IsPlaceDataExists(Number - 1);
            if (isPlaceDataExists)
            {
                numberTopImage.sprite = controller.VisualSettings.FindPlaceSprite(Number - 1);
                numberTopImage.SetNativeSize();

                Color targetOutlineColor = controller.VisualSettings.FindOutlineColor(Number - 1);
                numberTopText.SetupUnderlayColor(targetOutlineColor, true);
            }

            CommonUtility.SetObjectActive(numberTopImage.gameObject, isPlaceDataExists);
            CommonUtility.SetObjectActive(numberTopText.gameObject, isPlaceDataExists);
            CommonUtility.SetObjectActive(numberText.gameObject, !isPlaceDataExists);
        }


        public void SetupNickName(string _nickName)
        {
            NickName = _nickName;
            nickNameText.text = NickName;
        }


        public void SetupSkullsCount(float _skullsCount)
        {
            SkullsCount = _skullsCount;
            skullsCountText.text = SkullsCount.ToShortFormat();
        }


        public void RefreshVisual()
        {
            SelectorKey selectorKey = new SelectorKey(CurrentOwnerType, isNextLeagueAchived);

            foreach (var selector in selectorImages)
            {
                selector.Select(selectorKey);
            }

            foreach (var selector in selectorOutlines)
            {
                selector.Select(selectorKey);
            }

            foreach (var selector in selectorColors)
            {
                selector.Select(selectorKey);
            }
        }


        public void SetupNextLeagueAchived(bool _isNextLeagueAchived)
        {
            isNextLeagueAchived = _isNextLeagueAchived;
            CommonUtility.SetObjectActive(leagueIconArrowRoot, isNextLeagueAchived);
        }


        public void SetupController(LeagueProposeController _controller) =>
            controller = _controller;
        

        public void SetupOwnerType(LeaderBordItemType ownerType) =>
            CurrentOwnerType = ownerType;


        public void SetupShooterType(ShooterSkinType shooterSkinType)
        {
            ShooterSkinType = shooterSkinType;

            shooterImage.sprite = IngameData.Settings.shooterSkinsSettings.GetSkinUiSprite(shooterSkinType);
            shooterImage.SetNativeSize();
        }


        public void SetupLeagueType(LeagueType _leagueType)
        {
            leagueType = _leagueType;

            leagueIconImage.sprite = controller.VisualSettings.FindElementLeagueIconSprite(leagueType, CurrentOwnerType);
            leagueIconImage.SetNativeSize();
        }


        public void SetupRewardVisual(RewardData[] data, bool shouldHideMainReward)
        {
            RewardData mainRewardData = controller.MainRewardData;

            RewardData foundMainRewardData = data.Find(e => e.IsEqualsReward(mainRewardData));
            RewardData[] rewardToShow = shouldHideMainReward ?
                data.Where(e => e != foundMainRewardData).ToArray() : data;

            for (int i = 0; i < roots.Length; i++)
            {
                bool shouldShow = i < rewardToShow.Length;
                CommonUtility.SetObjectActive(roots[i].gameObject, shouldShow);

                if (shouldShow)
                {
                    roots[i].SetupController(controller);
                    roots[i].SetupOwnerType(CurrentOwnerType);
                    roots[i].SetupIsNextLeagueAchived(isNextLeagueAchived);
                    roots[i].SetupRewardData(rewardToShow[i]);
                    roots[i].RefreshVisual();
                }
            }
        }

        public void SetupEndLeagueIcon(RewardData rewardData)
        {
            if (rewardData == null)
            {
                return;
            }

            CommonUtility.SetObjectActive(endLeaguePetRewardRoot.gameObject, true);

            endLeaguePetRewardRoot.SetupController(controller);
            endLeaguePetRewardRoot.SetupOwnerType(CurrentOwnerType);
            endLeaguePetRewardRoot.SetupIsNextLeagueAchived(isNextLeagueAchived);
            endLeaguePetRewardRoot.SetupRewardData(rewardData);
            endLeaguePetRewardRoot.RefreshVisual();
        }

        public void SetupRewardVisual(LeagueType league, int positionIndex, string liveOpsId)
        {
            ILeagueRewardController leagueRewardController = GameServices.Instance.ProposalService.LeagueProposeController.RewardController;
            UiLeagueRewardData data = leagueRewardController.GetLeagueRewardPreviewData(liveOpsId, league, positionIndex);

            if (data == null)
            {
                CustomDebug.Log($"Can't setup reward visual for {this}. UiLeagueRewardData is null. league = {league}, position = {positionIndex}");

                foreach (var r in roots)
                {
                    CommonUtility.SetObjectActive(r.gameObject, false);
                }

                return;
            }

            SetupRewardVisual(data.RewardData, positionIndex == 0);
        }

        #endregion
    }
}
