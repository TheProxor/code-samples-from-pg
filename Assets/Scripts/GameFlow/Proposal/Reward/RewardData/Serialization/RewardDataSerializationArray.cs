using UnityEngine;
using System;
using System.Collections.Generic;


namespace Drawmasters.Proposal
{
    public class RewardDataSerializationArray
    {
        private readonly string saveKey;

        public RewardDataSerializationArray() { }

        public RewardDataSerializationArray(string _saveKey) : this()
        {
            saveKey = _saveKey;
        }


        public RewardData[] Data
        {
            get
            {
                List<RewardData> result = new List<RewardData>();

                string[] jsonArray = CustomPlayerPrefs.GetObjectValue<string[]>(saveKey);

                if (jsonArray == null)
                {
                    CustomDebug.Log("jsonArray is null");
                    return Array.Empty<RewardData>();
                }

                foreach (var i in jsonArray)
                {
                    if (string.IsNullOrEmpty(i))
                    {
                        CustomDebug.Log("Empty or NULL value");
                    }
                    else
                    {
                        RewardData data = RewardData.FromJson<RewardData>(i);

                        if (data == null)
                        {
                            CustomDebug.Log("Deseralized object NULL");
                        }
                        else
                        {
                            result.Add(data);
                        }
                    }
                }

                return result.ToArray();
            }
            set
            {
                List<string> jsonList = new List<string>();

                foreach (var i in value)
                {
                    if (i == null)
                    {
                        CustomDebug.Log("Try serialize NULL value");
                    }
                    else
                    {
                        string json = JsonUtility.ToJson(i);

                        if (string.IsNullOrEmpty(json))
                        {
                            CustomDebug.Log("Json is NULL or empty");
                        }
                        else
                        {
                            jsonList.Add(json);
                        }
                    }
                }

                CustomPlayerPrefs.SetObjectValue(saveKey, jsonList.ToArray());
            }
        }
    }
}
