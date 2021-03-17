using System;


namespace Drawmasters
{
    public static class PrefsKeys
    {
        public static class Shop
        {
            public const string BoughtShooterSkinsUpdate = "bought_shooter_skins_update";
            public const string BoughtWeaponSkinsUpdate = "bought_weapons_skins_update";
            public const string BoughtPetSkinsUpdate = "bought_pet_skins_update";
        }

        public static class Application
        {
            public const string ApplicationVersion = "application_version";
            public const string LastBackgroundStateChangeTime = "last_background_state_change_time";
            public const string TimeValidationLastBackgroundEnterRealUtcTime = "time_validation_last_background_enter_real_utc_time"; 
        }


        public static class PlayerInfo
        {
            [Obsolete]
            public const string PlayerData = "player_data";

            public const string PlayerDataUpdated = "player_data_updated";
            public const string PlayerMansionData = "player_mansion_data";

            public const string LevelModesProgress = "level_modes_progress";
            public const string CurrencyCountPrefix = "currency_count_";
            public const string CurrencyCount = "skulls_count"; 
            public const string FirstLauchTime = "first_launch_time";
            public const string LastRealUTCDate = "last_real_utc_date";
                        
            public const string ShooterSkinInfo = "skin_info";
            public const string ChaptersInfo = "chapters_info";

            public const string WeaponSkinInfoUpdate = "weapon_skin_info_update";

            public const string IsBloodEnabled = "is_blood_enabled";
            public const string IsRated = "is_rated";

            public const string OpenModesData = "open_modes_data";
            public const string LastPlayedMode = "last_played_mode";

            public const string HoldedLevels = "holded_levels_new_new";
            public const string RandomedLevelsData = "randomed_levels_data";

            public const string LevelProgressData = "level_progress_data";

            public const string OpenModesInfo = "open_modes_info";

            public const string EarnedCurrentPerLevelPrefix = "earned_currency_per_level_";
            public const string WasBonusLevelProposed = "was_bonus_level_proposed";

            public const string CurrentSkinProgess = "current_skin_progress";

            public const string LastResultSkinProgressType = "last_result_skin_progress_type";
            public const string CurrentSkinTypeToPropose = "current_skin_type_to_propose";

            public const string CurrentPlayerSkin = "current_player_skin";
            public const string CurrentPetSkin = "current_pet_skin";

            public const string WasSpinProposeTutorialShown = "was_spin_propose_tutorial_shown";
        }


        public static class Analytics
        {
            public const string IsFirstLaunch = "is_first_launch";
            public const string FirstLaunchDateTime = "first_launch_date_time";

            public const string TotalVideoShowCount = "total_video_show_count";

            public const string MinutesInGame = "minutes_in_game";

            public const string InterstitialWatchCount = "interstitial_watch_count";
            public const string VideoWatchCount = "video_watch_count";

            public const string LastActiveDay = "last_active_day";

            public const string MatchesCount = "matches_count";
            public const string TodayMatchesCount = "today_matches_count";
            public const string TodayMatchesCountDate = "today_matches_count_date";

            public const string UniqueLevelsCount = "unique_levels_count";
            
            public const string LastVideoWatchTime = "last_video_watch_time";
            public const string LastInterstitialWatchTime = "last_interstitial_watch_time";

            public const string SessionsCount = "sessions_count";

            public const string AttemptsData = "attempts_data";

        }


        public static class Timers
        {
            public const string LiveOpsTimerPostfix = "_live_ops_timer";
        }


        public static class Notifications
        {
            public const string IsNotificationsEnabled = "is_notifications_enabled";
            public const string WasNotificationAllowPopupShowed = "was_notification_allow_popup_showed";
            public const string LastRegisteredNotificationDate = "last_registered_notification_date";

            public const string FreeSpinAvailable = "free_spin_available";
            public const string UserMissedDayOne = "user_missed_day_one";
            public const string UserMissedDayThree = "user_missed_day_three";
            public const string UserMissedDaySeven = "user_missed_day_seven";
            public const string MonopolyNextLiveOps = "monopoly_next_live_ops";
            public const string HitmastersNextLiveOps = "hitmasters_next_live_ops";
            public const string SeasonEventNextLiveOps = "season_event_next_live_ops";

            public const string MonopolyFinishSoonLiveOps = "monopoly_finish_soon_live_ops";
            public const string HitmastersFinishSoonLiveOps = "hitmasters_finish_soon_live_ops";
            public const string SeasonEventFinishSoonLiveOps = "season_event_finish_soon_live_ops"; 
        }


        public static class Tutorial
        {
            public const string Data = "tutorial_data";

            public const string MansionForceShow = "mansion_forceshow";

            public const string MainDrawTutorial = "main_drw_tutorial";
        }


        public static class Proposal
        {
            public const string VideoShooterSkinProposeCountKey = "video_shooter_skin_propose_count";
            public const string VideoShooterSkinWasShownKey = "video_shooter_skin_was_shown";
            
            public const string VideoWeaponSkinProposeCountKey = "video_weapon_skin_propose_count";
            public const string VideoWeaponSkinWasShownKey = "video_weapon_skin_was_shown";
            
            public static class LiveOpsEvent
            {
                public const string HappyHoursLeagueMainKey = "happy_hours_league_main_key";
                public const string HappyHoursSeasonEventMainKey = "happy_hours_season_event_main_key";

                public const string LastStartedLiveOpsIdKey = "_last_started_liveOps_id_key"; 
            }


            public static class LiveOps
            {
                public const string WasAvailabilityEventSent = "_was_availability_event_sent";

                public const string PostfixDurationTimerKey = "_duration_timer_key";
                public const string PostfixReloadTimerKey = "_reload_timer_key";
                public const string PostfixyNextLiveOpsDateTime = "_next_live_ops_date_time";
                public const string PostfixLevelsDelta = "_levels_delta";
                public const string PostfixShowCounter = "_show_counter";
                public const string PostfixForceShow = "_force_show";
                public const string PostfixTaskCompleted = "_task_completed";
                public const string PostfixLastShowDate = "_last_show_date";
            }

            public static class League
            {
                public const string LeaguePlayerLiveOpsData = "player_live_ops_league_data";

                public const string LeagueShouldShowPreviewScreen = "league_should_show_preview_screen";
                public const string LeagueBotsLiveOpsData = "league_bots_live_ops_league_data";
                public const string LeagueBotsKey = "league_bots_key";
                public const string LeagueSavedRewardDataInfo = "league_saved_reward_data_info";
                public const string LeagueCanClaimRewards = "league_can_claim_rewards";
                public const string LeagueIdKey = "league_id_key";
                public const string LeagueReachedLeague = "league_reached_league";
                public const string LeageWasNewLeagueReached = "league_new_league_was_reached";
                public const string ReachedLeagueLiveOpsIdKey = "reached_league_live_ops_id_key";

                public const string SavedLeagueRewardHoldDataKey = "saved_league_reward_hold_data_key";
                public const string PositionsRewardDataSaveKey = "positions_reward_data_save_key_";
            }

            public const string LeagueMainKey = "league";
            public const string MonopolyMainKey = "monopoly";
            public const string HitmastersSpinOffMainKey = "hitmasters_spin_off";
            public const string SeasonEventMainKey = "season_event";

            // counters
            public const string ForcemeterShowCounter = "forcemeter_show_counter";
            public const string ShopShowCounter = "shop_show_counter";
            public const string PremiumShopShowCounter = "premiumshop_show_counter";
            public const string SpinRouletteShowCounter = "spinroulette_show_counter";

            public const string ResultRewardLevelsCounter = "result_reward_levels_counter";
            public const string ForcemeterLevelsCounter = "forcemeter_levels_counter";

            public const string RouletteLevelsCounter = "roulette_levels_counter";

            public const string RateUsProposingData = "rate_us_proposing_date";
            public const string RateUsProposingMatches = "rate_us_proposing_matches";

            // shop
            public const string WasShopResultShowed = "was_shop_result_showed";
            public const string ShopResultLevelsCounter = "shop_result_levels_counter";


            // premium shop
            public const string WasPremiumShopResultShowed = "was_premium_shop_result_showed";
            public const string PremiumShopResultLevelsCounter = "premium_shop_result_levels_counter";


            public const string LastRouletteFreeSpinDate = "last_roulette_free_spin_date";
            public const string LastSpinRouletteReward = "last_spin_roulette_reward";
            public const string SpinRouletteTimer = "spin_roulette_timer";

            public const string IsRouletteFreeSpinAvailable = "is_roulette_free_spin_available";

            public const string ShooterSkinsPanelButtonTimer = "shooter_skins_panel_button_timer";
            public const string WeaponSkinsPanelButtonTimer = "weapon_skins_panel_button_timer";

            public const string WasBonusLevelProposeShowed = "was_bonus_level_propose_showed";

            // mansion
            public const string MansionRewardRoomTimerPrefix = "mansion_room_reward_";
            public const string MansionRewardRoomNewTimerPrefix = "mansion_room_reward_new_";

            public const string MansionKeysProposeTimer = "mansion_keys_propose_timer";
            public const string MansionHammerProposeTimer = "mansion_hammers_propose_timer";

            public const string MansionRoomGeneratedRewardPrefix = "mansion_room__generated_reward_";
            public const string FreeMansionKey = "free_mansion_key";

            // progress
            public const string ProgresShooterSkinShowsCount = "progres_shooter_skin_shows_count";

            // ui panel
            public const string UiPanelShooterSkinShowsCount = "ui_panel_shooter_skin_shows_count";
            public const string UiPanelWeaponSkinShowsCount = "ui_panel_weapon_skin_shows_count";

            // monopoly
            public const string MonopolyLiveOpsDeskCounter = "monopoly_live_ops_desk_counter";
            public const string MonopolyAutoRoll = "monopoly_auto_roll";
            public const string LastMonopolyReward = "last_monopoly_reward";
            public const string MonopolyBonesProposeTimer = "monopoly_bones_propose_timer";

            // hitmasters spin off
            public const string HitmastersSpinOffLiveOpsLevelCounter = "hitmasters_spin_off_last_live_ops_level_counter"; // legacy?
            public const string HitmastersSpinOffLiveOpsGameMode = "hitmasters_spin_off_last_live_ops_game_mode";
            public const string HitmastersSpinOffLastReward = "hitmasters_spin_off_last_reward";
            public const string HitmastersShouldScrollMapOnShow = "hitmasters_should_scroll_map_on_show";
            public const string HitmastersShouldShowPreviewScreen = "hitmasters_should_show_preview_screen";
            public const string HotmastersLevelsDelta = "hitmasters_levels_delta";


            // season event
            public const string LastSeasonEventRewardPrefix = "last_season_event_reward_prefix_";
            public const string SeasonEventRewardSaveData = "season_event_reward_save_data";
            public const string IsSeasonPassBought = "is_season_pass_bought";
            public const string SeasonEventShouldShowPreviewScreen = "season_event_should_show_preview_screen";
            public const string WasOldSeasonEventActiveOnStart = "was_old_season_event_active_on_start";

            // season event old
            [Obsolete] public const string LastSeasonEventReward = "last_season_event_reward";
            [Obsolete] public const string LastSeasonEventPassReward = "last_season_event_pass_reward";
            [Obsolete] public const string SeasonEventFinishedLevelsCountOnStart = "season_event_finished_levels_count_on_start";
            [Obsolete] public const string SeasonEventRewardCounter = "season_event_reward_counter";
        }
        
        public static class Offer
        {
            public static class GoldenTicket
            {
                public const string MainKey = "golden_ticket";
            }


            public static class Subscription
            {
                public const string MainKey = "offer_subscription_main_key";
            }
            

            public static class BaseOffer
            {
                public const string PostfixyNextRunOfferDateTime = "_next_run_offer_date_time";
            }

        }
        
        public static class AbTest
        {
            //Common
            public const string UaAbTestMechanicKeyPrefix = "ua_ab_test_mechanic_";

            // level result
            public const string UaLevelsResultRewardDeltaCount = "ua_level_result_reward_delta_count";
            public const string UaLevelsResultRewardAllow = "ua_level_result_reward_allow";

            // roulette
            public const string UaRouletteDeltaCount = "ua_roulette_delta_count";
            public const string UaRouletteAllow = "ua_roulette_allow";
            public const string UaRouletteMinLevel = "ua_roulette_min_level";

            // shop result
            public const string UaShopResultDeltaCount = "ua_shop_result_delta_count";
            public const string UaShopResultAllow = "ua_shop_result_allow";
            public const string UaShopResultMinLevel = "ua_shop_result_min_level";

            // premium shop result
            public const string UaPremiumShopResultDeltaCount = "ua_premium_shop_result_delta_count";
            public const string UaPremiumShopResultAllow = "ua_premium_shop_result_allow";
            public const string UaPremiumShopResultMinLevel = "ua_premium_shop_result_min_level";

            // forcemeter
            public const string UaForcemeterDeltaCount = "ua_forcemeter_delta_count";
            public const string UaForcemeterAllow = "ua_forcemeter_allow";
            public const string UaForcemeterMinLevel = "ua_forcemeter_min_level";

            public const string UaAnnouncersEnabled = "ua_announcers_enabled";

            //infinity drawing
            public const string UaInfinityDrawingAvailable = "ua_infinity_drawing_available";
            
            //main menu
            public const string UaMainMenuWithScrollLiveOps = "ua_main_menu_with_scroll_liveops";
            public const string UaMainMenuWithoutScrollLiveOps = "ua_main_menu_without_scroll_liveops";
            public const string UaMainMenuCombinedCollection = "ua_main_menu_combined_collection";
        }


        public static class Dev
        {
            public const string DevLeagueEnabled = "dev_league_enabled";
            public const string DevMonopolyEnabled = "dev_monopoly_enabled";
            public const string DevMansionEnabled = "dev_mansion_enabled";
            public const string DevSeasonEventEnabled = "dev_season_event_enabled";
            public const string DevAlternativeLevelsPackEnabled = "dev_alternative_levels_pack_enabled";
            public const string DevHitmastersSpinOffEnabled = "dev_hitmasters_spin_off_enabled";
            public const string DevRateUsEveryLevel = "dev_rate_us_every_level";
            public const string DevAbTestHardcodedData = "dev_ab_test_hardcoded_data";

            public class Pet
            {
                public const string AlwaysCharged = "dev_pet_always_charged";
            }
        }


        public static class Utils
        {
            public const string StartTimeKeyPostfix = "start_time_key_";
            public const string FinishTimeKeyPostfix = "finish_time_key_";
        }


        public static class Subscription
        {
            public const string WasShowed = "was_start_subscrption_showed";
            public const string WasOldUsersRewardCancelCheck = "was_old_users_reward_cancel_check";
            public const string OldUsersLevelsFinishedCount = "old_users_levels_finished_count";
        }
    }
}
