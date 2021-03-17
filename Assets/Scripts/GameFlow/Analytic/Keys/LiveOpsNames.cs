using System;
using System.Text.RegularExpressions;
using Drawmasters.Proposal;

namespace Drawmasters
{
    public static class LiveOpsNames
    {
        private static string ConvertStringFormat(string input)
        {
            string result = Regex.Replace(input, @"\p{Lu}", m => "_" + m.Value.ToLowerInvariant());
            result = (char.ToUpperInvariant(result[0]) + result.Substring(1)).Trim('_');
            return result;
        }


        public static class Hitmasters
        {
            public const string Name = "hitmasters";
            
            public static string GetEventName(GameMode mode)
            {
                string result = ConvertStringFormat(mode.ToString());
                return result;
            }
        }
        
        
        public static class Monopoly
        {
            public const string Name = "monopoly";
            
            public static string GetEventName(RewardData reward)
            {
                if (reward == null)
                {
                    return string.Empty;
                }
                string result;

                switch (reward)
                {
                    case CurrencyReward currency:
                        result = ConvertStringFormat(currency.currencyType.ToString());
                        break;

                    case ShooterSkinReward shooterSkinReward:
                        result = ConvertStringFormat(shooterSkinReward.skinType.ToString());
                        break;
                    
                    case WeaponSkinReward weaponSkinReward:
                        result = ConvertStringFormat(weaponSkinReward.skinType.ToString());
                        break;

                    default:
                        result = string.Empty;
                        break;
                }
                
                return result;
            }
        }
        
        
        public static class SeasonEvent
        {
            public const string Name = "season_event";
            
            private const string gems = "gems";

            public static string GetEventName(PetSkinType mode)
            {
                if (mode == PetSkinType.None)
                {
                    return gems;
                }
                else
                {
                    string result = ConvertStringFormat(mode.ToString());
                    return result;
                }
            }
        }
        
        
        public static class Tournament
        {
            public const string Name = "tournament";
            
            public static string GetEventName(LeagueType mode)
            {
                string result = ConvertStringFormat(mode.ToString());
                return result;
            }
        }
    }
}