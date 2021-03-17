using System;
using System.Collections.Generic;
using System.Linq;
using Drawmasters.Proposal;
using UnityEngine;



namespace Drawmasters
{
    [CreateAssetMenu(fileName = "BotSettings",
        menuName = NamingUtility.MenuItems.Settings + "BotSettings")]
    public class BotSettings : ScriptableObject
    {
        #region Helpers

        [Serializable]
        public class Data
        {
            public LeagueType leagueType = default;
            public BotData[] bots = default;
            public BotAvatar[] Avatars = default;
        }
        
        
        [Serializable]
        public class BotData
        {
            [Sirenix.OdinInspector.ReadOnly]
            public string id = Guid.NewGuid().ToString();
            public BotType type = default;

        }
        
        
        [Serializable]
        public class BotTypeInfo
        {
            public BotType botType = default;
            public float difficultyFactor = default;
        }

        
        [Serializable]
        public class BotAvatar
        {
            public ShooterSkinType avatar = default;
            public int weight = default;
        }

        #endregion
        
        
        
        #region Fields

        [SerializeField] private BotTypeInfo[] botTypeInfo = default;
        [SerializeField] private Data[] data = default;
        
        public float defaultPlayerTimeFactor = 10;
        
        public float randomSigma = 1;
        
        public int сycleTimeSec = 60;
        
        public int blockSkullMin = 30;
        public int blockSkullMax = 50;
        public int unblockBotSkull = 0;

        public int offlineSkullMin = 70;
        public int offlineBotSkullMax = 100;

        public int countBotsPerSession = 5;
        
        [SerializeField] private string[] nicknames = default;

        public float botsRefreshPeriod = default;

        #endregion



        #region Public methods

        public string RandomNickname(List<string> usedNames)
        {
            var list = nicknames.Where(x => !usedNames.Contains(x)).ToList();
            return list.RandomObject();
        }

        
        public float GetDifficultyFactor(BotType botType)
        {
            float result = 1f;

            BotTypeInfo data = Array.Find(botTypeInfo, x => x.botType == botType);
            if (data != null)
            {
                result = data.difficultyFactor;
            }

            return result;
        }
        
        
        public BotData[] GetBots(LeagueType type)
        {
            BotData[] result = new BotData[0];

            Data item = Array.Find(data, x => x.leagueType == type);
            if (item != null)
            {
                result = item.bots;
            }

            return result;
        }


        public ShooterSkinType RandomBotSkinType(LeagueType type)
        {
            ShooterSkinType result = default;
            
            Data item = Array.Find(data, x => x.leagueType == type);
            if (item != null)
            {
                int[] weights = GetWeights(item.Avatars);
                
                int randomIndex = RandomExtentions.RandomWeight(weights);
            
                BotAvatar botAvatar = randomIndex < 0 ? null : item.Avatars[randomIndex];
                
                if (botAvatar != null)
                {
                    result = botAvatar.avatar;
                }
            }
            
            return result;
            
            
            int[] GetWeights(BotAvatar[] data)
            {
                int[] weight = new int[data.Length];
                for (int i = 0; i < weight.Length; i++)
                {
                    weight[i] = data[i].weight;
                    if (i > 0)
                    {
                        weight[i] += weight[i - 1];
                    }
                }

                return weight;
            }
        }

        #endregion
        
        
        
        #region Editor methods
                
        [Sirenix.OdinInspector.Button]
        private void NicknameDataSetter()
        {
            List<string> allList = new List<string>();

            var loadedNames = Utils.ResourcesUtility.FindInstance<TextAsset>("Assets/Scripts/GameFlow/Proposal/League/Bots/names.txt");
            string[] list = loadedNames.text.Split('\n');
            foreach (var line in list)
            {
                if (!string.IsNullOrWhiteSpace(line) && !allList.Contains(line))
                {
                    allList.Add(line);
                }
            }
            nicknames = allList.ToArray();
        }
        
        #endregion
    }
}