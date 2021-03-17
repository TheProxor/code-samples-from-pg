using System.Linq;


namespace Drawmasters
{
    public static class SoundGroupKeys
    {
        #region Fields

        private static readonly string[] hitKeys = { AudioKeys.Ingame.HIT_1,
                                                     AudioKeys.Ingame.HIT_2,
                                                     AudioKeys.Ingame.HIT_3,
                                                     AudioKeys.Ingame.HIT_4,
                                                     AudioKeys.Ingame.HIT_5,
                                                     AudioKeys.Ingame.HIT_6,
                                                     AudioKeys.Ingame.HIT_7,
                                                     AudioKeys.Ingame.HIT_8
        };


        private static readonly string[] characterEmotionsKeys = { AudioKeys.Ingame.AUDIO_CHARACTER_AAA_1,
                                                                   AudioKeys.Ingame.AUDIO_CHARACTER_AAA_2,
                                                                   AudioKeys.Ingame.AUDIO_CHARACTER_AAA_3,
                                                                   AudioKeys.Ingame.AUDIO_CHARACTER_KHEE,
                                                                   AudioKeys.Ingame.AUDIO_CHARACTER_OOO_1,
                                                                   AudioKeys.Ingame.AUDIO_CHARACTER_OOO_2,
                                                                   AudioKeys.Ingame.AUDIO_CHARACTER_OUCH,
                                                                   AudioKeys.Ingame.AUDIO_CHARACTER_PHHOUCH
        };

        private static readonly string[] bossEmotionsKeys =      { AudioKeys.Ingame.BOSSPAIN_01,
                                                                   AudioKeys.Ingame.BOSSPAIN_02,
                                                                   AudioKeys.Ingame.BOSSPAIN_03,
                                                                   AudioKeys.Ingame.BOSSPAIN_04,
                                                                   AudioKeys.Ingame.BOSSPAIN_05,
                                                                   AudioKeys.Ingame.BOSSPAIN_06,
                                                                   AudioKeys.Ingame.BOSSPAIN_07,
                                                                   AudioKeys.Ingame.BOSSPAIN_08
        };


        private static readonly string[] bossLaughtKeys =        { AudioKeys.Ingame.BOSSLAUGHTER,
                                                                   AudioKeys.Ingame.BOSSLAUGHTER_02,
                                                                   AudioKeys.Ingame.BOSSLAUGHTER_03
        };


        private static readonly string[] limbChopOffKeys = { AudioKeys.Ingame.BLOOD_FOUNTAIN_1,
                                                             AudioKeys.Ingame.BLOOD_FOUNTAIN_2,
                                                             AudioKeys.Ingame.BLOOD_FOUNTAIN_3
        };


        private static readonly string[] woodBreakKeys = { AudioKeys.Ingame.WOOD_BREAK_01,
                                                           AudioKeys.Ingame.WOOD_BREAK_02,
                                                           AudioKeys.Ingame.WOOD_BREAK_03,
                                                           AudioKeys.Ingame.WOOD_BREAK_04
        };


        private static readonly string[] ricochetKeys = { AudioKeys.Ingame.RICOCHET_01,
                                                          AudioKeys.Ingame.RICOCHET_02,
                                                          AudioKeys.Ingame.RICOCHET_03,
                                                          AudioKeys.Ingame.RICOCHET_04,
                                                          AudioKeys.Ingame.RICOCHET_05,
                                                          AudioKeys.Ingame.RICOCHET_06
        };


        private static readonly string[] bodyFallDownKeys = { AudioKeys.Ingame.BODY_FALL_DOWN_01,
                                                              AudioKeys.Ingame.BODY_FALL_DOWN_02,
                                                              AudioKeys.Ingame.BODY_FALL_DOWN_03,
                                                              AudioKeys.Ingame.BODY_FALL_DOWN_04
        };


        private static readonly string[] boxFallDownKeys = { AudioKeys.Ingame.CUBE_FALL_DOWN_HIT_01,
                                                             AudioKeys.Ingame.CUBE_FALL_DOWN_HIT_02,
                                                             AudioKeys.Ingame.CUBE_FALL_DOWN_HIT_03,
                                                             AudioKeys.Ingame.CUBE_FALL_DOWN_HIT_04
        };


        private static readonly string[] bigSphereFallDownKeys = { AudioKeys.Ingame.SPHERE_BIG_HIT_01,
                                                                   AudioKeys.Ingame.SPHERE_BIG_HIT_02,
                                                                   AudioKeys.Ingame.SPHERE_BIG_HIT_03,
                                                                   AudioKeys.Ingame.SPHERE_BIG_HIT_04
        };

        private static readonly string[] smallSphereFallDownKeys = { AudioKeys.Ingame.SPHERE_SMALL_HIT_01,
                                                                     AudioKeys.Ingame.SPHERE_SMALL_HIT_02,
                                                                     AudioKeys.Ingame.SPHERE_SMALL_HIT_03,
                                                                     AudioKeys.Ingame.SPHERE_SMALL_HIT_04
        };

        private static readonly string[] levelMusicKeys = { AudioKeys.Music.MUSICLOOP01,
                                                            AudioKeys.Music.MUSICLOOP02,
                                                            AudioKeys.Music.MUSICLOOP03,
                                                            AudioKeys.Music.MUSICLOOP04,
                                                            AudioKeys.Music.MUSICLOOP05,
                                                            AudioKeys.Music.MUSICLOOP06
                                                          };

        private static readonly string[] levelBossMusicKeys = { AudioKeys.Music.MUSIC_BOSS_02
                                                              };


        private static readonly string[] levelBonusMusicKeys = { AudioKeys.Music.MUSIC_BONUSGAME_01
                                                              };


        private static readonly string[] minigunShotKeys = { AudioKeys.Ingame.WEAPON_MACHINEGUN_01,
                                                             AudioKeys.Ingame.WEAPON_MACHINEGUN_02,
                                                             AudioKeys.Ingame.WEAPON_MACHINEGUN_03,
                                                             AudioKeys.Ingame.WEAPON_MACHINEGUN_04,
                                                             AudioKeys.Ingame.WEAPON_MACHINEGUN_05,
                                                             AudioKeys.Ingame.WEAPON_MACHINEGUN_06
                                                            };


        private static readonly string[] scaryEnemiesKeys = { AudioKeys.Ingame.ENEMY_VOICE_SCARED_01,
                                                              AudioKeys.Ingame.ENEMY_VOICE_SCARED_02,
                                                              AudioKeys.Ingame.ENEMY_VOICE_SCARED_03,
                                                              AudioKeys.Ingame.ENEMY_VOICE_SCARED_04,
                                                              AudioKeys.Ingame.ENEMY_VOICE_SCARED_05,
                                                              AudioKeys.Ingame.ENEMY_VOICE_SCARED_06,
                                                              AudioKeys.Ingame.ENEMY_VOICE_SCARED_07,
                                                              AudioKeys.Ingame.ENEMY_VOICE_SCARED_08
                                                            };


        private static readonly string[] coinsClaimKeys = { AudioKeys.Ingame.COIN_01,
                                                            AudioKeys.Ingame.COIN_02,
                                                            AudioKeys.Ingame.COIN_03,
                                                            AudioKeys.Ingame.COIN_04,
                                                            AudioKeys.Ingame.COIN_05,
                                                            AudioKeys.Ingame.COIN_06
        };


        private static string LastRandomedHitKey;
        private static string LastRandomedEmotionKey;
        private static string LastRandomedBossEmotionKey;
        private static string LastRandomedLimbChopOffKey;
        private static string LastRandomedWoodBreakKey;
        private static string LastRandomedRicochetKey;
        private static string LastRandomedBodyFallDownKey;
        private static string LastRandomedBoxFallDownKey;
        private static string LastRandomedBigSphereFallDownKey;
        private static string LastRandomedSmallSphereFallDownKeysKey;
        private static string LastRandomLevelMusicKey;
        private static string LastRandomBossLevelMusicKey;
        private static string LastRandomBonusLevelMusicKey;
        private static string LastMinigunShotKey;
        private static string LastScaryEnemiesKey;
        private static string LastCoinClaimKey;

        #endregion



        #region Properties

        public static string RandomHitKey => RandomKey(hitKeys, ref LastRandomedHitKey);


        public static string RandomCharacterEmotionKey => RandomKey(characterEmotionsKeys, ref LastRandomedEmotionKey);


        public static string RandomBossEmotionKey => RandomKey(bossEmotionsKeys, ref LastRandomedBossEmotionKey);


        public static string RandomLimbChopOffKey => RandomKey(limbChopOffKeys, ref LastRandomedLimbChopOffKey);


        public static string RandomWoodBreakKey => RandomKey(woodBreakKeys, ref LastRandomedWoodBreakKey);


        public static string RandomRicochetKey => RandomKey(ricochetKeys, ref LastRandomedRicochetKey);


        public static string RandomBodyFallDownKey => RandomKey(bodyFallDownKeys, ref LastRandomedBodyFallDownKey);


        public static string RandomBoxFallDownKeys => RandomKey(boxFallDownKeys, ref LastRandomedBoxFallDownKey);


        public static string RandomBigSphereFallDownKeys => RandomKey(bigSphereFallDownKeys, ref LastRandomedBigSphereFallDownKey);


        public static string RandomSmallSphereFallDownKeys => RandomKey(smallSphereFallDownKeys, ref LastRandomedSmallSphereFallDownKeysKey);

        public static string RandomLevelMusicKey => RandomKey(levelMusicKeys, ref LastRandomLevelMusicKey);

        public static string RandomBossLevelMusicKey => RandomKey(levelBossMusicKeys, ref LastRandomBossLevelMusicKey);

        public static string RandomBonusLevelMusicKey => RandomKey(levelBonusMusicKeys, ref LastRandomBonusLevelMusicKey);

        public static string RandomMinigunShotKey => RandomKey(minigunShotKeys, ref LastMinigunShotKey);

        public static string RandomScaryEnemiesKey => RandomKey(scaryEnemiesKeys, ref LastScaryEnemiesKey);

        public static string RandomCoinClaimKey => RandomKey(coinsClaimKeys, ref LastCoinClaimKey);

        #endregion



        #region Methods

        public static string GetBossLaughterKey(int stage)
        {
            string result = default;

            int index = stage % bossLaughtKeys.Length;

            if (index < bossLaughtKeys.Length)
            {
                result = bossLaughtKeys[index];
            }

            return result;
        }

        private static string RandomKey(string[] array, ref string lastRandomedKey)
        {
            if (array.Length == 1)
            {
                return array.First();
            }

            string exceptedKey = lastRandomedKey;
            string randomedKey = array
                                    .Where(e => e != exceptedKey)
                                    .ToArray()
                                    .RandomObject();

            lastRandomedKey = randomedKey;
            return randomedKey;
        }

        #endregion
    }
}
