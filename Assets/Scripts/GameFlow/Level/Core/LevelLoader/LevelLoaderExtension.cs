using System;
using Drawmasters.Levels.Data;
using Drawmasters.ServiceUtil;
using Drawmasters.Tutorial;

namespace Drawmasters.Levels
{
    public static class LevelLoaderExtension
    {
        public static ILevelLoader DefineLoader(this ILevelLoader thisLoader,
                                        Action onTutorialStart,
                                        bool isReload)
        {
            ILevelLoader chosenLoader = default;

            LevelContext context = GameServices.Instance.LevelEnvironment.Context;

            TutorialType tutorialType = TutorialManager.FindGameTutorial(context.Mode);

            bool existGameModeTutorial = tutorialType != TutorialType.None &&
                                         !TutorialManager.WasTutorialCompleted(tutorialType);

            bool canProposeRateUs = GameServices.Instance.ProposalService.RateUsProposal.CanPropose;
            TutorialType drawTutorialType = TutorialManager.DrawTutorialType;

            TutorialType petTutorial = GameServices.Instance.PetsService.TutorialController.NextAllowedTutorialType;

            if (context.IsEditor)
            {
                chosenLoader = new EditorLevelLoader();
            }
            else if (!context.Mode.IsHitmastersLiveOps() &&
                     !TutorialManager.WasTutorialCompleted(drawTutorialType))
            {
                chosenLoader = new FingerTutorialLevelLoader(drawTutorialType, ScreenType.TutorialFinger);
            }
            else if (context.IsBonusLevel &&
                    !TutorialManager.WasTutorialCompleted(TutorialType.DrawBonus))
            {
                chosenLoader = new FingerTutorialLevelLoader(TutorialType.DrawBonus, ScreenType.TutorialFinger);
            }
            else if (existGameModeTutorial)
            {
                chosenLoader = new TutorialLevelLoader(tutorialType, onTutorialStart);
            }
            else if (canProposeRateUs)
            {
                chosenLoader = new RateUsLevelLoader();
            }
            else if (!context.Mode.IsHitmastersLiveOps() &&
                     petTutorial != TutorialType.None)
            {
                chosenLoader = new FingerTutorialLevelLoader(petTutorial, ScreenType.PetsChargeTutorial);
            }
            else if (isReload)
            {
                chosenLoader = new ReloadLoader();
            }
            else
            {
                chosenLoader = new CommonLoader();
            }

            return chosenLoader;
        }
    }
}

