using System;
using System.Collections.Generic;
using Drawmasters.Proposal;

namespace Drawmasters
{
    public static class RandomExtentions
    {
        #region Nested types

        [Serializable]
        public class DropWeight
        {
            public int value = default;
            public int weight = default;
        }
        
        #endregion
        
        
        
        #region Filds
        
        private static bool available;
        private static double nextGauss;
        private static readonly Random randomizer;
        
        #endregion
        
        
        
        #region Ctor

        static RandomExtentions()
        {
            randomizer = new Random(Guid.NewGuid().GetHashCode());
        }

        #endregion
        
        
        
        #region Methods
        
        public static double RandomGauss(float sigma)
        {
            return sigma * RandomGauss();
        }
        
        public static double RandomGauss(float mu, float sigma)
        {
            return mu + RandomGauss(sigma);
        }

        public static int RandomWeight(DropWeight[] data)
        {
            
            int[] weights = GetWeights(data);
            int randomIndex = RandomWeight(weights);
            
            int result = randomIndex < 0 ? int.MinValue : data[randomIndex].value;

            return result;
        }

        public static RewardData RandomWeight(RewardData[] data)
        {
            int[] weights = GetWeights(data);
            int randomIndex = RandomWeight(weights);
            
            RewardData result = randomIndex < 0 ? null : data[randomIndex];

            return result;
        }

        private static double RandomGauss()
        {
            if (available)
            {
                available = false;
                return nextGauss;
            }

            double u1 = randomizer.NextDouble();
            double u2 = randomizer.NextDouble();
            double temp1 = Math.Sqrt(-2.0 * Math.Log(u1));
            double temp2 = 2.0 * Math.PI * u2;

            nextGauss = temp1 * Math.Sin(temp2);
            available = true;
            return temp1 * Math.Cos(temp2);
        }
        
        public static int[] GetWeights(DropWeight[] data)
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
        
        public static int[] GetWeights(RewardData[] data)
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
        
        public static int RandomWeight(int[] weights)
        {
            int result = int.MinValue;
            
            int totalWeights = weights.LastObject();
            
            for (int i = 0; i < weights.Length; i++)
            {
                int randomValue = randomizer.Next(totalWeights + 1);
                if (randomValue <= weights[i])
                {
                    result = i;
                    break;
                }
            }

            return result;
        }

        public static List<T> GetRandomItems<T>(List<T> list, int count)
        {
            List<T> result = new List<T>();

            if (list.Count <= count)
            {
                result = list;
            }
            else
            {
                int maxIterationCount = count * 5;
                int findCount = 0;

                for (int i = 0; i < maxIterationCount; i++)
                {
                    int index = UnityEngine.Random.Range(0, count - 1);

                    if (result.Contains(list[index]))
                    {
                        continue;
                    }

                    result.Add(list[index]);

                    findCount++;
                
                    if (findCount >= count)
                    {
                        break;
                    }
                }
            }

            return result;
        }

        #endregion
    }
}