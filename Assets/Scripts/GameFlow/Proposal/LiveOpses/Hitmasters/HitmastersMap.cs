using System.Linq;
using Drawmasters;
using Drawmasters.Proposal;
using Drawmasters.ServiceUtil;
using Drawmasters.Ui.Extensions;
using UnityEngine;
using DG.Tweening;


public class HitmastersMap : MonoBehaviour, IInitializable, IDeinitializable
{
    #region Filds

    [SerializeField] private UILineRenderer path = default;
    [SerializeField] private UILineRenderer finishedPath = default;
    
    [SerializeField] private HitmastersMapPoint[] points = default;

    private HitmastersProposeController controller;
    private Vector2[] allPointsPositions;

    #endregion



    #region Properties

    public int CurrentPointIndex => controller.LiveOpsLevelCounter;

    public int PointsLength => points.Length;

    #endregion



    #region Methods

    public RectTransform GetPointRectTransform(int index) =>
        points[index].transform as RectTransform;
    

    public void PlayLevelCompleteAnimation(int completedIndex)
    {
        if (completedIndex < 0 || completedIndex >= points.Length)
        {
            CustomDebug.Log($"Wrong level completed index {completedIndex}");
            return;
        }

        points[completedIndex].PlayCompletedAnimation();

        bool isLast = completedIndex == points.Length - 1;

        if (!isLast)
        {
            points[completedIndex + 1].PlayUnlockedAnimation();

            Vector2[] passedPointsPositions = allPointsPositions.Take(completedIndex + 1).ToArray();
            finishedPath.SetupPoints(passedPointsPositions);

            controller.VisualSettings.finishLineMoveAnimation.SetupBeginValue(passedPointsPositions.Last());
            controller.VisualSettings.finishLineMoveAnimation.SetupEndValue(allPointsPositions[completedIndex + 1]);

            controller.VisualSettings.finishLineMoveAnimation.Play(value =>
            {
                passedPointsPositions = passedPointsPositions.Add(value);
                finishedPath.SetupPoints(passedPointsPositions);
            }, this);
        }
    }


    public void Initialize()
    {
        controller = GameServices.Instance.ProposalService.HitmastersProposeController;
        GameMode gameMode = controller.LiveOpsGameMode;

        allPointsPositions = points.Select(e => (e.transform as RectTransform).anchoredPosition).ToArray();
        path.SetupPoints(allPointsPositions);

        Vector2[] passedPointsPositions = allPointsPositions.Take(controller.LiveOpsLevelCounter + 1).ToArray();
        finishedPath.SetupPoints(passedPointsPositions);

        for (int i = 0; i < points.Length; i++)
        {
            points[i].Initialize(i, GetPointType(i + 1, gameMode));
        }


        HitmastersMapPoint.MapPointType GetPointType(int index, GameMode mode)
        {
            HitmastersMapPoint.MapPointType type = HitmastersMapPoint.MapPointType.Level;

            if (controller.HitmastersProposals.IsRouletteAvailable(mode, index))
            {
                type = HitmastersMapPoint.MapPointType.Suitcase;
            }
            else if (controller.HitmastersProposals.IsForceMeterAvailable(mode, index))
            {
                type = HitmastersMapPoint.MapPointType.Forcemeter;
            }
            else if (controller.HitmastersProposals.IsPremiumShopAvailable(mode, index))
            {
                type = HitmastersMapPoint.MapPointType.Shop;
            }
            else if (controller.HitmastersProposals.IsBossAvailable(mode, index))
            {
                type = HitmastersMapPoint.MapPointType.Boss;
            }
            
            return type;
        }
    }


    public void Deinitialize()
    {
        DOTween.Kill(this);

        foreach (var point in points)
        {
            point.Deinitialize();
        }
    }
    
    #endregion
}
