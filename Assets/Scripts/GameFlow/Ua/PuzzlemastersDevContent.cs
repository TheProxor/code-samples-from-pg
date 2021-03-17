using System;
using System.Linq;
using System.Collections.Generic;
using Drawmasters.Levels;
using Drawmasters.Ui;
using Drawmasters.Advertising;
using UnityEngine;
using System.Collections;
using Drawmasters.AbTesting;
using Drawmasters.Interfaces;
using Drawmasters.Levels.Order;
using UaMenu.V2;
using UaMenu.ColorSchemeEditor;
using Drawmasters.Statistics.Data;
using Drawmasters.ServiceUtil;
using Modules.General.InAppPurchase;
using Drawmasters.Proposal;
using Drawmasters.Ui.Enums;
using I2.Loc;
using Modules.General.Abstraction;
using Drawmasters.Pets;

namespace Drawmasters.Ua
{
    public class PuzzlemastersDevContent : V1Bridge
    {
        #region Fields

        public static bool IsUaBuild =>
#if UA_BUILD
                true;
#else
                false;
#endif
        private static readonly HashSet<string> marketingLevelNames =
            new HashSet<string>()
            {
                "a_lvl_for_marketing1"
            };


        private static readonly HashSet<ShooterSkinType> UaShooterSkinTypes =
            new HashSet<ShooterSkinType>()
            {
                ShooterSkinType.Grinch,
                ShooterSkinType.Claus
            };

        public static event Action<WeaponType> OnShouldChangeMonolith;
        public static event Action OnDecalsShowChanged;

        public static event Action OnShouldResetLevelVisual;

        public static event Action<bool> OnSettedPortraitOrientationEnabled;

        private static readonly GameMode[] AllowLoadedModes = { GameMode.Draw, GameMode.UaLandscapeMode };

        private DropdownModel dropdownOptions;
        private SliderModel cameraSizeModel;

        private Coroutine loadLevelRoutine;

        private GameMode modeToLoad;

        private int targetLevelNumber;

        #endregion



        #region Lifecycle

        public PuzzlemastersDevContent(string[] backgrounds, int currentIndex, Color currentColor)
            : base()
        {
            LimbsVisualDamageLevelTargetComponent.isDismemberEnabled = !IsUaBuild;
            ColorSchemeManager.OnColorChanged += ColorSchemeManager_OnColorChanged;

            var services = GameServices.Instance;
            var proposals = services.ProposalService;

            devContent.tabs.AddTab("image");
            devContent.tabs.AddTab("palette");
            devContent.tabs.AddTab("dice");
            devContent.tabs.AddTab("flask");
            devContent.tabs.AddTab("cart");

            {
                devContent.AddTransparencySlider();
            }

            devContent.SetDefaultTabIndex(1);
            {
                AddSeparator("Визуал");

                AddColorSchemeWidget();
                AddButtonWidget("Сбросить визуал уровня", "Сброс", () => OnShouldResetLevelVisual?.Invoke());

                AddSeparator("Бэкграунд");
                AddSwitchWidget("Бэкграунд:", backgrounds, currentIndex, OnChangeBackground);

                AddSeparator("Монолит (Земля)");
                AddSwitchWidget("Монолит (Земля):", Enum.GetValues(typeof(WeaponType)).OfType<WeaponType>().Where(e => e != WeaponType.None).Select(o => o.ToString()).ToArray(), currentIndex, OnChangeMonolith);

                AddSeparator("Деколи");
                AddTumblerWidget("Деколи на монолите (земле):", VisualLevelObject.ShouldShowMonolithDecalsUa, MonolithDecalsChanged);
                AddTumblerWidget("Деколи на уровне:", VisualLevelObject.ShouldShowLevelDecalsUa, DecalsChanged);

                AddSeparator("Эффекты");
                AddTumblerWidget("Шейк камеры:", IngameCamera.AllowShake, isOn => IngameCamera.AllowShake = isOn);
                AddTumblerWidget("Кровь: ", PlayerData.isUaBloodEnabled, OnBloodChanged);

                AddTumblerWidget("Отрыв конечностей", LimbsVisualDamageLevelTargetComponent.isDismemberEnabled, v => LimbsVisualDamageLevelTargetComponent.isDismemberEnabled = v);
                AddTumblerWidget("Убийство персонажей (Игра перезапуститься)", PlayerData.IsUaKillingShootersEnabled, v =>
                {
                    PlayerData.IsUaKillingShootersEnabled = v;

                    MonoBehaviourLifecycle.Exit();
                });

                AddSeparator("Камера:");
                AddTumblerWidget("Портретная камера:", IngameCamera.IsPortrait, v => SetPortraitOrLandscapeCameraEnabled(v));
                AddTumblerWidget("Автоповорот экрана:", Screen.orientation == ScreenOrientation.AutoRotation, SetAutorotationEnabled);

                cameraSizeModel = AddSlider("Размер камеры", 0, -1, 1, (value) =>
                {
                    IngameCamera.Instance.Camera.orthographicSize = value;
                    IngameCamera.IsSizeLocked = true;
                });

                AddButtonWidget("Сбросить размер камеры", "Сброс", () =>
                {
                    IngameCamera.IsSizeLocked = false;
                    IngameCamera.Instance.RefreshCameraSize();
                    RefreshCameraSliderModel();
                });

                RefreshCameraSliderModel();
                IngameCamera.IsSizeLocked = false;
            }

            devContent.SetDefaultTabIndex(2);
            {
                AddSeparator("Игровой контент", 1);

                AddSwitchWidget("Тип главного меню:", 
                    Enum.GetValues(typeof(MainMenuScreenState)).OfType<MainMenuScreenState>().Select(o => o.ToString()).ToArray(), 
                    CurrentMainMenu(), 
                    OnChangeMainMenu, 
                    1);
                
                AddButtonWidget("Открыть все", "Открыть", OnUnlockAll, 1);
                AddButtonWidget("Сбросить прогресс", "Сбросить", OnDeleteAll, 1);
                
                AddSeparator("Реклама:");
                AddTumblerWidget("Мгновенный просмотр интеров", AdsWrapper.IsAvailableForInterstitial,
                    i => AdsWrapper.IsAvailableForInterstitial = i, 1);
                AddTumblerWidget("Мгновенный просмотр видео", AdsWrapper.IsAvailableForVideo,
                    i => AdsWrapper.IsAvailableForVideo = i, 1);

                string[] adReturnOptions = Enum.GetValues(typeof(AdActionResultType)).OfType<AdActionResultType>().Select(o => o.ToString()).ToArray();
                var adReturnOptionsWidget = AddDropdownWidget("Возвращать статус", adReturnOptions, i => AdsWrapper.ResultType = (AdActionResultType)i, (int)AdsWrapper.ResultType, 1);
                adReturnOptionsWidget.Value = (int)AdsWrapper.ResultType;

                AddSeparator(string.Empty, 1);

                AddInputIntWidget("Целевой уровень", value => targetLevelNumber = value, 1);

                AddSeparator(string.Empty, 1);

                AddButtonWidget("Записать целевой уровень в статистику", "Записать", () => OnShouldMoveToLevel(false), 1);

                AddSeparator(string.Empty, 1);

                AddButtonWidget("Перейти на целевой уровень", "Перейти", () => OnShouldMoveToLevel(true), 1);

                AddSeparator(string.Empty, 1);

                AddButtonWidget("Добавить 10 монет", "Добавить", () => GameServices.Instance.PlayerStatisticService.CurrencyData.AddCurrency(CurrencyType.Simple, 10.0f), 1);
                AddButtonWidget("Добавить 10 гемов", "Добавить", () => GameServices.Instance.PlayerStatisticService.CurrencyData.AddCurrency(CurrencyType.Premium, 10.0f), 1);
                AddButtonWidget("Добавить 10 молотков", "Добавить", () => GameServices.Instance.PlayerStatisticService.CurrencyData.AddCurrency(CurrencyType.MansionHammers, 10.0f), 1);
                AddButtonWidget("Добавить 3 костей", "Добавить", () => GameServices.Instance.PlayerStatisticService.CurrencyData.AddCurrency(CurrencyType.RollBones, 3.0f), 1);
                AddButtonWidget("Добавить 5 черепов", "Добавить", () => GameServices.Instance.PlayerStatisticService.CurrencyData.AddCurrency(CurrencyType.Skulls, 5.0f), 1);
                AddButtonWidget("Добавить 3 очка сезона", "Добавить", () => GameServices.Instance.PlayerStatisticService.CurrencyData.AddCurrency(CurrencyType.SeasonEventPoints, 3), 1);
                AddButtonWidget("Сбросить валюту", "Сбросить", OnRemoveAllCurrency, 1);

                AddSeparator("Питомцы", 1);
                AddTumblerWidget("Призыв доступен всегда:", PetsChargeController.UaAbAlwaysCharged.IsMechanicAvailable, v => PetsChargeController.UaAbAlwaysCharged.ChangeMechanicAvailability(v), 1);

                AddSeparator("Время", 1);
                AddTumblerWidget("Чит-времени:", GameServices.Instance.TimeValidator.AllowTimeCheating, value => GameServices.Instance.TimeValidator.AllowTimeCheating = value, 1);

                AddSeparator("Интерфейс");
                AddTumblerWidget("UI/UX:", true, OnChangeUIUX);

                AddSeparator("Анонсеры");
                AddTumblerWidget("Анонсеры:", LevelAnnouncersController.IsEnabled, isOn => LevelAnnouncersController.IsEnabled = isOn);

                AddSeparator("Загрузка уровня");
                
                //hotfix
                AddDropdownWidget("Игровой мод", Enum.GetValues(typeof(GameMode)).OfType<GameMode>().Where(i => Array.Exists(AllowLoadedModes, e => e == i)).Select(o => o.ToString()).ToArray(), SetGameModeToLoad, 0);
                dropdownOptions = AddDropdownWidget("Номер уровня", Array.Empty<string>(), LoadLevel, 0);
                SetGameModeToLoad(0);

                AddDropdownWidget("Язык", LocalizationManager.GetAllLanguages().ToArray(), SetLanguage, 0);
            }

            devContent.SetDefaultTabIndex(3);
            {
                AddSeparator("АБ тестирование");

                AddSeparator("Общее");
                
                AddTumblerWidget("Хардкод Данные:", AbTestService.ShouldUseHardcoadedData, v => AbTestService.UaAbTestMechanic.ChangeMechanicAvailability(v));

                AddSeparator("Оцени нас");
                AddTumblerWidget("Оцени нас каждый уровень:", RateUsProposal.UaAbTestEveryLevel.IsMechanicAvailable, v => RateUsProposal.UaAbTestEveryLevel.ChangeMechanicAvailability(v));

                // TODO OPEN MODES UA LOGIC
                //AddButtonWidget("Сбросить", "Сброс", IngameData.Settings.modesInfo.ClearUaData);

                AddSeparator("Награда x2");

                AddTumblerWidget("Доступность:", proposals.IngameCurrencyMultiplier.AllowToPropose, v => proposals.IngameCurrencyMultiplier.UaAllow = v);
                AddButtonWidget("Доступность к АБтесту ", "Сброс", proposals.IngameCurrencyMultiplier.ClearUaAllowData);

                AddInputIntWidget("Каждые матчей:", levelsDelta => proposals.IngameCurrencyMultiplier.UaLevelsDeltaCount = levelsDelta);
                AddButtonWidget("Матчи к АБ тесту", "Сброс", proposals.IngameCurrencyMultiplier.ClearUaAllowData);

                AddSeparator("Доступность АВ тест механик");

                AddTumblerWidget(
                    "Доступность монополии",
                     GameServices.Instance.ProposalService.MonopolyProposeController.UaAbTestMechanic.IsMechanicAvailable,
                    isAvailable => GameServices.Instance.ProposalService.MonopolyProposeController.UaAbTestMechanic.ChangeMechanicAvailability(isAvailable));

                AddButtonWidget(
                    "Сбросить значение монополии",
                    "Сбросить",
                     GameServices.Instance.ProposalService.MonopolyProposeController.UaAbTestMechanic.ResetAvailability);

                AddTumblerWidget(
                    "Доступность хитмастер лайв опса",
                     GameServices.Instance.ProposalService.HitmastersProposeController.UaAbTestMechanic.IsMechanicAvailable,
                    isAvailable => GameServices.Instance.ProposalService.HitmastersProposeController.UaAbTestMechanic.ChangeMechanicAvailability(isAvailable));

                AddButtonWidget(
                    "Сбросить значение хитмастер лайв опса",
                    "Сбросить",
                    GameServices.Instance.ProposalService.HitmastersProposeController.UaAbTestMechanic.ResetAvailability);

                AddTumblerWidget(
                    "Доступность сизон пасса",
                    GameServices.Instance.ProposalService.SeasonEventProposeController.UaAbTestMechanic.IsMechanicAvailable,
                    isAvailable => GameServices.Instance.ProposalService.SeasonEventProposeController.UaAbTestMechanic.ChangeMechanicAvailability(isAvailable));

                AddButtonWidget(
                    "Сбросить значение сизон пасса",
                    "Сбросить",
                    GameServices.Instance.ProposalService.SeasonEventProposeController.UaAbTestMechanic.ResetAvailability);

                AddTumblerWidget(
                    "Доступность Лиги",
                    GameServices.Instance.ProposalService.LeagueProposeController.UaAbTestMechanic.IsMechanicAvailable,
                    isAvailable => GameServices.Instance.ProposalService.LeagueProposeController.UaAbTestMechanic.ChangeMechanicAvailability(isAvailable));

                AddButtonWidget(
                    "Сбросить значение Лиги",
                    "Сбросить",
                    GameServices.Instance.ProposalService.LeagueProposeController.UaAbTestMechanic.ResetAvailability);

                AddTumblerWidget(
                    "Доступность HappyHours Лиги",
                    GameServices.Instance.ProposalService.HappyHoursLeagueProposeController.UaAbTestMechanic.IsMechanicAvailable,
                    isAvailable => GameServices.Instance.ProposalService.HappyHoursLeagueProposeController.UaAbTestMechanic.ChangeMechanicAvailability(isAvailable));

                AddButtonWidget(
                    "Сбросить значение HappyHours Лиги",
                    "Сбросить",
                    GameServices.Instance.ProposalService.HappyHoursLeagueProposeController.UaAbTestMechanic.ResetAvailability);

                AddTumblerWidget(
                    "Ограниченное рисование",
                    LevelPathController.uaAbTestMechanic.IsMechanicAvailable,
                    isAvailable => LevelPathController.uaAbTestMechanic.ChangeMechanicAvailability(isAvailable));

                AddButtonWidget(
                    "Сбросить значение",
                    "Сбросить",
                    LevelPathController.uaAbTestMechanic.ResetAvailability);


                AddTumblerWidget(
                    "Особняк (игра перезапустится)",
                    GameServices.Instance.ProposalService.MansionProposeController.IsMechanicAvailable,
                    v =>
                    {
                        MansionProposeController.uaAbTestMechanic.ChangeMechanicAvailability(v);

                        MonoBehaviourLifecycle.Exit();
                    });

                AddButtonWidget(
                    "Сбросить особняк(игра перезапустится)",
                    "Сбросить",
                    () =>
                    {
                        MansionProposeController.uaAbTestMechanic.ResetAvailability();

                        MonoBehaviourLifecycle.Exit();
                    });


                AddTumblerWidget("Альтернативный пак уровней (Игра перезапуститься)", LevelOrderService.uaAbTestMechanic.IsMechanicAvailable, v =>
                {
                    LevelOrderService.uaAbTestMechanic.ChangeMechanicAvailability(v);

                    MonoBehaviourLifecycle.Exit();
                });
            }

            devContent.SetDefaultTabIndex(4);
            {
                AddSeparator("Премиальный Магазин");

                AddInputIntWidget("Мин. матчей", (v) => proposals.PremiumShopResultController.UaMinLevel = v);
                AddButtonWidget("Мин матчей к АБ тесту", "Сброс", proposals.PremiumShopResultController.ClearUaMinLevelData);

                AddTumblerWidget("Доступность:", proposals.PremiumShopResultController.AllowToPropose, (v) => proposals.PremiumShopResultController.UaAllow = v);
                AddButtonWidget("Доступность АБ к тесту", "Сброс", proposals.PremiumShopResultController.ClearUaAllowData);

                AddInputIntWidget("Каждые матчей:", (value) => proposals.PremiumShopResultController.UaLevelsDeltaCount = value);
                AddButtonWidget("Матчи к АБ тесту", "Сброс", proposals.PremiumShopResultController.ClearUaLevelsData);

                AddSeparator("Магазин");

                AddInputIntWidget("Мин. матчей", (v) => proposals.ShopResultController.UaMinLevel = v);
                AddButtonWidget("Мин матчей к АБ тесту", "Сброс", proposals.ShopResultController.ClearUaMinLevelData);

                AddTumblerWidget("Доступность:", proposals.ShopResultController.AllowToPropose, (v) => proposals.ShopResultController.UaAllow = v);
                AddButtonWidget("Доступность АБ к тесту", "Сброс", proposals.ShopResultController.ClearUaAllowData);

                AddInputIntWidget("Каждые матчей:", (value) => proposals.ShopResultController.UaLevelsDeltaCount = value);
                AddButtonWidget("Матчи к АБ тесту", "Сброс", proposals.ShopResultController.ClearUaLevelsData);

                AddSeparator("Рулетка");

                AddInputIntWidget("Мин. матчей", (v) => proposals.RouletteRewardController.UaMinLevel = v);
                AddButtonWidget("Мин матчей к АБ тесту", "Сброс", proposals.RouletteRewardController.ClearUaMinLevelData);

                AddTumblerWidget("Доступность:", proposals.RouletteRewardController.AllowToPropose, v => proposals.RouletteRewardController.UaAllow = v);
                AddButtonWidget("Доступность АБ к тесту", "Сброс", proposals.RouletteRewardController.ClearUaAllowData);

                AddInputIntWidget("Каждые матчей:", (value) => proposals.RouletteRewardController.UaLevelsDeltaCount = value);
                AddButtonWidget("Матчи к АБ тесту", "Сброс", proposals.RouletteRewardController.ClearUaLevelsData);


                AddSeparator("Силомер");

                AddInputIntWidget("Мин. матчей", (v) => proposals.ForceMeterController.UaMinLevel = v);
                AddButtonWidget("Мин матчей к АБ тесту", "Сброс", proposals.ForceMeterController.ClearUaMinLevelData);

                AddTumblerWidget("Доступность:", proposals.ForceMeterController.AllowToPropose, v => proposals.ForceMeterController.UaAllow = v);
                AddButtonWidget("Доступность АБ к тесту", "Сброс", proposals.ForceMeterController.ClearUaAllowData);

                AddInputIntWidget("Каждые матчей:", (value) => proposals.ForceMeterController.UaLevelsDeltaCount = value);
                AddButtonWidget("Матчи к АБ тесту", "Сброс", proposals.ForceMeterController.ClearUaLevelsData);
            }
        }

        #endregion



        #region Overrided methods

        public static bool IsUaSkinType(ShooterSkinType shooterSkinType) =>
            UaShooterSkinTypes.Contains(shooterSkinType);


        private void OnChangeBackground(string backgroundName) =>
            LevelsManager.Instance.Level.ChangeBackground(backgroundName);
        

        private void OnChangeBackgroundColor(Color backgroundColor) =>
            LevelsManager.Instance.Level.ChangeBackgroundColor(backgroundColor);
        

        private void OnChangeUIUX(bool isActive) =>
            IngameScreen.IsUiEnabled = isActive;
        

        private void OnUnlockAll()
        {
            foreach (var currencyType in PlayerCurrencyData.PlayerTypes)
            {
                float valueToAdd = float.MaxValue * 0.0000000000000000000000000000000001f; // to avoid int converting
                GameServices.Instance.PlayerStatisticService.CurrencyData.AddCurrency(currencyType, valueToAdd);
            }

            var i = GameServices.Instance.ShopService;

            i.ShooterSkins.OpenAll();
            i.WeaponSkins.OpenAll();
            i.PetSkins.OpenAll();

            GameServices.Instance.PlayerStatisticService.ModesData.OpenAll();

            if (!SubscriptionManager.Instance.IsSubscriptionActive)
            {
                IngameData.Settings.subscriptionRewardSettings.CancelSubscriptionReward();
            }
        }


        private void OnRemoveAllCurrency()
        {
            CurrencyType[] currencyTypes = Enum.GetValues(typeof(CurrencyType))
                                      .Cast<CurrencyType>()
                                      .Where(e => e != CurrencyType.None)
                                      .ToArray();

            foreach (var currencyType in currencyTypes)
            {
                float currentCurrency = GameServices.Instance.PlayerStatisticService.CurrencyData.GetEarnedCurrency(currencyType);
                GameServices.Instance.PlayerStatisticService.CurrencyData.TryRemoveCurrency(currencyType, currentCurrency);
            }
        }

        #endregion



        #region Methods

        private void RefreshCameraSliderModel()
        {
            float cameraSize = IngameCamera.IsPortrait ?
                IngameData.Settings.ingameCameraSettings.portraitCameraSize :
                IngameData.Settings.ingameCameraSettings.landscapeCameraSize;

            cameraSizeModel.Max = 3 * cameraSize;
            cameraSizeModel.Min = 1;

            cameraSizeModel.Value = cameraSize;
        }


        private void SetGameModeToLoad(int result)
        {
            modeToLoad = AllowLoadedModes[result];

            if (!Enum.IsDefined(typeof(GameMode), modeToLoad))
            {
                modeToLoad = GameMode.None;
            }

            ILevelOrderService orderService = GameServices.Instance.LevelOrderService;

            int levelsCount = orderService.FindLevelsCount(modeToLoad);

            List<string> allowedIndexToLoad = new List<string>(levelsCount);
            for (int i = 0; i < levelsCount; i++)
            {
                SublevelData? data = orderService.FindDataWithoutOverflowCheck(modeToLoad, i);
                if (data == null)
                {
                    //TODO log
                    continue;
                }
                
                bool isMarketingLevel = marketingLevelNames.Contains(data.Value.NameId);
                string levelName = isMarketingLevel ? 
                    $"{i + 1}:MarketingLevel:{data.Value.NameId}" :
                    $"{i + 1}:{data.Value.NameId}";

                allowedIndexToLoad.Add(levelName);
            }
            
            dropdownOptions.Options = allowedIndexToLoad.ToArray();
        }


        private void MarkFillTextureChanged() =>
            LevelObjectMonolith.UseColoredFillTexture = true;


        private void MarkEdgeTextureChanged() =>
            LevelObjectMonolith.UseColoredEdgeTexture = true;


        private void OnChangeMonolith(string s)
        {
            if (Enum.TryParse(s, out WeaponType type))
            {
                LevelObjectMonolith.UseColoredEdgeTexture = false;
                LevelObjectMonolith.UseColoredFillTexture = false;

                LevelObjectMonolith.UaWeaponTypeMonolith = type;

                OnShouldChangeMonolith?.Invoke(type);
            }
        }

        private void OnChangeMainMenu(int index, string s)
        {
            if (Enum.TryParse(s, out MainMenuScreenState type))
            {
                IUaAbTestMechanic uaWithoutScrollLiveOps =
                    new CommonMechanicAvailability(PrefsKeys.AbTest.UaMainMenuWithoutScrollLiveOps);
                uaWithoutScrollLiveOps.ChangeMechanicAvailability(type == MainMenuScreenState.WithoutScrollLiveOps);
                
                IUaAbTestMechanic uaWithScrollLiveOps =
                    new CommonMechanicAvailability(PrefsKeys.AbTest.UaMainMenuWithScrollLiveOps);
                uaWithScrollLiveOps.ChangeMechanicAvailability(type == MainMenuScreenState.WithScrollLiveOps);
                
                IUaAbTestMechanic uaCombinedCollection =
                    new CommonMechanicAvailability(PrefsKeys.AbTest.UaMainMenuCombinedCollection);
                uaCombinedCollection.ChangeMechanicAvailability(type == MainMenuScreenState.CombinedCollection);
            }
        }

        private int CurrentMainMenu()
        {
            int result = default;
            
            IUaAbTestMechanic uaWithoutScrollLiveOps =
                new CommonMechanicAvailability(PrefsKeys.AbTest.UaMainMenuWithoutScrollLiveOps);
            if (uaWithoutScrollLiveOps.IsMechanicAvailable)
            {
                result = (int)MainMenuScreenState.WithoutScrollLiveOps;
                return result;
            }
            
            IUaAbTestMechanic uaWithScrollLiveOps =
                new CommonMechanicAvailability(PrefsKeys.AbTest.UaMainMenuWithScrollLiveOps);
            if (uaWithScrollLiveOps.IsMechanicAvailable)
            {
                result = (int)MainMenuScreenState.WithScrollLiveOps;
                return result;
            }
                
            IUaAbTestMechanic uaCombinedCollection =
                new CommonMechanicAvailability(PrefsKeys.AbTest.UaMainMenuCombinedCollection);
            if (uaCombinedCollection.IsMechanicAvailable)
            {
                result = (int)MainMenuScreenState.CombinedCollection;
                return result;
            }

            result = (int)GameServices.Instance.AbTestService.CommonData.mainMenuScreenState;
            return result;
        }


        private void MonolithDecalsChanged(bool value)
        {
            VisualLevelObject.ShouldShowMonolithDecalsUa = value;
            OnDecalsShowChanged?.Invoke();
        }


        private void DecalsChanged(bool value)
        {
            VisualLevelObject.ShouldShowLevelDecalsUa = value;
            OnDecalsShowChanged?.Invoke();
        }



        private void LoadLevel(int result)
        {
            if (loadLevelRoutine != null)
            {
                return;
            }

            loadLevelRoutine = MonoBehaviourLifecycle.PlayCoroutine(LoadLevelRoutine());

            bool isPortrait = modeToLoad != GameMode.UaLandscapeMode;
            SetPortraitOrLandscapeCameraEnabled(isPortrait);


            IEnumerator LoadLevelRoutine()
            {
                yield return null; // Cuz "OnPointerEnter" for Dropdown called in Dropdown.cs
                                   // So handler with "EventSystem.current.SetSelectedGameObject(this.gameObject)" cases exception if disable system instantly

                GameManager.Instance.SetGamePaused(false, GameManager.Instance);
                EventSystemController.SetSystemEnabled(false, this);
                TouchManager.Instance.IsEnabled = false;

                UiScreenManager.Instance.HideAll(true);
                LevelsManager.Instance.ClearLevel();

                PlayerData playerData = GameServices.Instance.PlayerStatisticService.PlayerData;

                playerData.SetModeInfo(modeToLoad, result);
                int index = playerData.GetModeCurrentIndex(modeToLoad);
                playerData.RecordLastPlayedMode(modeToLoad);

                FromLevelToLevel.PlayTransition(() =>
                {
                    LevelsManager.Instance.UnloadLevel();
                    LevelsManager.Instance.LoadLevel(modeToLoad, index);
                }, () =>
                {
                    //UiScreenManager.Instance.ShowScreen(ScreenType.Ingame);
                    LevelsManager.Instance.PlayLevel();
                });

                yield return new WaitForSecondsRealtime(6.0f); // for tutorial screen showed

                EventSystemController.SetSystemEnabled(true, this);
                TouchManager.Instance.IsEnabled = true;
                loadLevelRoutine = null;
                GameManager.Instance.SetGamePaused(true, GameManager.Instance);
            }
        }


        private void SetPortraitOrLandscapeCameraEnabled(bool isPortrait)
        {
            // hack Hot fixed. Some devices (7+, Xr) don't adapt to horizontal mode for dev ui
            //Screen.orientation = isPortrait ? ScreenOrientation.Portrait : ScreenOrientation.LandscapeLeft;

            IngameCamera.IsPortrait = isPortrait;

            if (IngameCamera.HasFoundInstance)
            {
                IngameCamera.Instance.RefreshCameraSize();
            }

            RefreshCameraSliderModel();
            OnSettedPortraitOrientationEnabled?.Invoke(isPortrait);
        }


        private void ColorSchemeManager_OnColorChanged(ColorScheme ColorScheme, string id)
        {
            if (string.Equals(PuzzlemastersColorSchemeLinks.BackgroundColorId, id, StringComparison.Ordinal))
            {
                string colorID = string.Concat(id, ".0");
                OnChangeBackgroundColor(ColorScheme.GetColor(colorID));
            }
            else if (string.Equals(PuzzlemastersColorSchemeLinks.MonolithFillColorId, id, StringComparison.Ordinal))
            {
                MarkFillTextureChanged();
            }
            else if (string.Equals(PuzzlemastersColorSchemeLinks.MonolithEdgeColorId, id, StringComparison.Ordinal))
            {
                MarkEdgeTextureChanged();
            }
        }


        private void SetAutorotationEnabled(bool isEnabled)
        {
            Screen.autorotateToPortrait = isEnabled;
            Screen.autorotateToLandscapeLeft = isEnabled;
            Screen.autorotateToLandscapeRight = isEnabled;
            Screen.autorotateToPortraitUpsideDown = isEnabled;
            
            Screen.orientation = isEnabled ? 
                ScreenOrientation.AutoRotation : 
                ScreenOrientation.Portrait;
        }


        private void OnBloodChanged(bool isEnabled)
        {
            PlayerData playerData = GameServices.Instance.PlayerStatisticService.PlayerData;

            PlayerData.isUaBloodEnabled = isEnabled;

            playerData.IsBloodEnabled = isEnabled;
        }


        private void OnDeleteAll()
        {
            CustomPlayerPrefs.DeleteAll();

            MonoBehaviourLifecycle.Exit();
        }


        private void OnShouldMoveToLevel(bool completeLevels)
        {
            if (targetLevelNumber <= 0)
            {
                return;
            }

            PlayerData playerData = GameServices.Instance.PlayerStatisticService.PlayerData;

            int currentLevelIndex = playerData.GetModeCurrentIndex(GameMode.Draw);

            int currentLevelsFinishedCount = GameServices.Instance.CommonStatisticService.GetLevelsFinishedCount(GameMode.Draw);
            GameServices.Instance.CommonStatisticService.SetLevelsFinishedCount(GameMode.Draw, targetLevelNumber - 1);

            if (completeLevels)
            {
                int levelIndex = CalculateLevels();
                playerData.SetModeInfo(GameMode.Draw, levelIndex);
            }

            FromLevelToLevel.PlayTransition(() =>
            {
                LevelsManager.Instance.UnloadLevel();
                LevelsManager.Instance.LoadScene(GameServices.Instance.PlayerStatisticService.PlayerData.LastPlayedMode, GameMode.MenuScene);

                UiScreenManager.Instance.HideAll(true);
                UiScreenManager.Instance.ShowScreen(ScreenType.MainMenu, isForceHideIfExist: true);
                GameServices.Instance.MusicService.InstantRefreshMusic();
            });

            devContent.SetActive(false);

            int CalculateLevels()
            {
                int result = 0;

                LevelsOrder.AbTestReplaceData[] abTestSublevelsReplaceData = GameServices.Instance.AbTestService.CommonData.abTestSublevelsReplaceData;
                ModeData[] modesData = IngameData.Settings.levelsOrder.LoadModesData(abTestSublevelsReplaceData);
                ModeData modeDataDraw = modesData.FirstOrDefault(x => x.mode == GameMode.Draw);

                int globalLevel = 0;
                foreach (var chapter in modeDataDraw.chapters)
                {
                    for(int i = 0; i < chapter.levels.Count && globalLevel < targetLevelNumber - 1; i++, globalLevel++)
                    {
                        result += chapter.levels[i].sublevels.Count;
                    }
                }

                return result;
            }
        }


        private void SetLanguage(int index)
        {
            string currentLanguage;
            string[] Languages = LocalizationManager.GetAllLanguages().ToArray();

            if (index < 0 || index >= Languages.Length)
            {
                currentLanguage = string.Empty;
            }
            else
            {
                currentLanguage = Languages[index];
            }

            if (LocalizationManager.HasLanguage(currentLanguage))
            {
                LocalizationManager.CurrentLanguage = currentLanguage;
            }
        }

        #endregion
    }
}