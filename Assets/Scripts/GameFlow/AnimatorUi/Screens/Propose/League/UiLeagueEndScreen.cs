using System;
using System.Collections;
using System.Collections.Generic;
using Drawmasters.Effects;
using Drawmasters.Utils.Ui;
using Drawmasters.Proposal;
using UnityEngine;
using UnityEngine.UI;
using Modules.General;
using DG.Tweening;
using Drawmasters.ServiceUtil;
using Drawmasters.Utils;
using I2.Loc;
using ntw.CurvedTextMeshPro;
using Spine.Unity;
using static Drawmasters.Proposal.LeagueRewardController;
using System.Linq;
using Drawmasters.Statistics.Data;


namespace Drawmasters.Ui
{
    public class UiLeagueEndScreen : AnimatorScreen
    {
        #region Fields
        
        [SerializeField] private IdleEffect[] idleEffects = default;
        
        [Header("Top Header")]
        [SerializeField] private Localize headerLocalizeText = default;
        [SerializeField] private SkeletonGraphic leagueTypeSkeletonGraphic = default;
        [SerializeField] private TextProOnACircle textProOnACircle;

        [Header("Scroll")] 
        [SerializeField] private RectTransform contentRoot = default;
        [SerializeField] private RectTransform startShowRoot = default;
        [SerializeField] private Localize greatJobLocalizeText = default;

        [Header("Bottom")]
        [SerializeField] private Button nextButton = default;

        private LeagueProposeController controller;

        private ShowAndMoveElementsHelper showAndMoveElementsHelper;

        private readonly List<UiLeagueLeaderBoardElement> elements = new List<UiLeagueLeaderBoardElement>();

        private Coroutine scrollRoutine;

        private LeagueType currentLeagueType;

        #endregion



        #region Properties

        public override ScreenType ScreenType =>
            ScreenType.LeagueEnd;

        #endregion



        #region Overrided methods

        public override void Initialize(Action<AnimatorView> onShowEndCallback = null,
                                        Action<AnimatorView> onHideEndCallback = null,
                                        Action<AnimatorView> onShowBeginCallback = null,
                                        Action<AnimatorView> onHideBeginCallback = null)
        {
            base.Initialize(onShowEndCallback, onHideEndCallback, onShowBeginCallback, onHideBeginCallback);

            controller = GameServices.Instance.ProposalService.LeagueProposeController;
            
            foreach (var idleEffect in idleEffects)
            {
                idleEffect.CreateAndPlayEffect();
            }
            
            currentLeagueType =  controller.LeagueReachController.WasNewLeagueReached ? 
                controller.LeagueReachController.ReachedLeague.GetPreviousLeague() : 
                controller.LeagueReachController.ReachedLeague;
            
            SpineUtility.SafeSetAnimation(leagueTypeSkeletonGraphic,
                controller.VisualSettings.FindShowWhiteAnimationKey(currentLeagueType), 0, false, () =>
                {
                    SpineUtility.SafeSetAnimation(leagueTypeSkeletonGraphic,
                        controller.VisualSettings.FindIdleWhiteAnimationKey(currentLeagueType), 0, true);
                });
        }


        public override void Deinitialize()
        {
            foreach (var element in elements)
            {
                element.Deinitialize();
            }

            elements.Clear();

            foreach (var idleEffect in idleEffects)
            {
                idleEffect.StopEffect();
            }

            showAndMoveElementsHelper?.Deinitialize();
            
            DOTween.Kill(this);
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
            MonoBehaviourLifecycle.StopPlayingCorotine(scrollRoutine);
            
            base.Deinitialize();
        }


        public override void InitializeButtons()
        {
            nextButton.onClick.AddListener(NextButton_OnClick);
        }


        public override void DeinitializeButtons()
        {
            nextButton.onClick.RemoveListener(NextButton_OnClick);
        }

        #endregion



        #region Methods
        
        public void FillLeaderBord(PositionRewardData[] uiLeagueRewardData)
        {
            LeaderBoardItem[] items = controller.LeaderBoard.TournamentResult;
            
            foreach (var item in items)
            {
                string nickName = item.NickName;
                LeaderBordItemType itemType = item.ItemType;

                float skulls = item.SkullsCount;
                ShooterSkinType skinType = item.SkinType;
                bool isNextLeagueAchived = controller.LeaderBoard.IsNextLeagueAchived(item.Id);
                int position = controller.LeaderBoard.GetPosition(item.Id) + 1;
                elements.Add(CreateElement(position, nickName, skulls, itemType, skinType, isNextLeagueAchived));
            }

            foreach (var element in elements)
            {
                element.Initialize();
                CanvasGroup canvas = element.GetComponent<CanvasGroup>();
                if (canvas != null)
                {
                    canvas.alpha = 0;
                }
            }

            InitialLeaderBord(RefreshVisual);

            CommonUtility.SetObjectActive(nextButton.gameObject, false);
            CommonUtility.SetObjectActive(greatJobLocalizeText.gameObject, false);

            RefreshVisual();

            UiLeagueLeaderBoardElement CreateElement(int number, string nickName, float skullsCount, LeaderBordItemType ownerType,
                ShooterSkinType shooterSkinType, bool isNextLeagueAchieved)
            {
                UiLeagueLeaderBoardElement elementToAdd =
                    Content.Management.CreateUiLeagueEndElement(contentRoot);
                elementToAdd.SetupController(controller);
                elementToAdd.SetupOwnerType(ownerType);

                elementToAdd.SetupLeagueType(currentLeagueType);
                elementToAdd.SetupNumber(number);
                elementToAdd.SetupShooterType(shooterSkinType);
                elementToAdd.SetupNickName(nickName);
                elementToAdd.SetupSkullsCount(skullsCount);
                elementToAdd.SetupNextLeagueAchived(isNextLeagueAchieved);

                PositionRewardData foundData = Array.Find(uiLeagueRewardData, e => e.boardPosition == number - 1);
                RewardData[] allRewards = foundData == null ? Array.Empty<RewardData>() : foundData.rewardDataSerialization.Data;

                RewardData[] rewardForElements = allRewards.Where(e => e.Type != RewardType.PetSkin).ToArray();

                // TODO in merge utility.
                List<RewardData> mergedRewards = rewardForElements.Where(e => e.Type != RewardType.Currency).ToList();
                foreach (var cur in PlayerCurrencyData.PlayerTypes)
                {
                    RewardData[] allCurrencyReward = rewardForElements.ToList().Where(e => e.Type == RewardType.Currency).ToArray();

                    if (allCurrencyReward.Any())
                    {
                        float mergedValue = allCurrencyReward.Select(e => (e as CurrencyReward).value).Sum();
                        RewardData mergedRewardData = new CurrencyReward() { currencyType = cur, value = mergedValue };

                        mergedRewards.Add(mergedRewardData);
                    }
                }
              
                elementToAdd.SetupRewardVisual(mergedRewards.ToArray(), false);

                RewardData petRewardData = allRewards.Find(e => e.Type == RewardType.PetSkin);
                elementToAdd.SetupEndLeagueIcon(petRewardData);

                elementToAdd.RefreshVisual();

                return elementToAdd;
            }
        }


        private void InitialLeaderBord(Action callback)
        {
            scrollRoutine = MonoBehaviourLifecycle.PlayCoroutine(PerfomAction());

            IEnumerator PerfomAction()
            {
                bool isMoving = false;
                
                yield return new WaitForEndOfFrame();

                foreach (var element in elements)
                {
                    
                    isMoving = true;

                    CanvasGroup canvas = element.GetComponent<CanvasGroup>();
                    
                    Scheduler.Instance.CallMethodWithDelay(this, () =>
                     {
                         showAndMoveElementsHelper = showAndMoveElementsHelper ?? new ShowAndMoveElementsHelper(
                             controller.VisualSettings.leagueEndElementMoveAnimation, 
                             controller.VisualSettings.leagueEndElementAlpfaAnimation);

                         Vector2 finishPosition = new Vector2(element.RectTransform.anchoredPosition.x, 
                             element.RectTransform.anchoredPosition.y);
                         
                         showAndMoveElementsHelper.MoveLayoutElement(startShowRoot.anchoredPosition, finishPosition, 
                             element.LayoutElement, canvas, () =>
                         {
                             isMoving = false;
                         });
                     }, controller.VisualSettings.startElementsMoveDelay);
                    
                    yield return new WaitUntil(() => !isMoving);
                }
                
                CanvasGroup buttonCanvas = nextButton.GetComponent<CanvasGroup>();
                buttonCanvas.alpha = 0;
                
                CommonUtility.SetObjectActive(nextButton.gameObject, true);
                
                controller.VisualSettings.leagueEndButtonAlpfaAnimation.Play(value => 
                    buttonCanvas.alpha = value, this);

                CommonUtility.SetObjectActive(greatJobLocalizeText.gameObject, elements.Count == 3);
                
                callback?.Invoke();
            }
        }
        

        private void RefreshVisual()
        {
            string key = controller.VisualSettings.FindHeaderKey(currentLeagueType);
            headerLocalizeText.SetTerm(key);
            
            textProOnACircle.enabled = false;
            Scheduler.Instance.CallMethodWithDelay(this, () => textProOnACircle.enabled = true, CommonUtility.OneFrameDelay);
        }
        
        #endregion



        #region Events handlers

        private void NextButton_OnClick()
        {
            DeinitializeButtons();

            Hide();
        }

        #endregion
    }
}
