using UnityEngine;


namespace Drawmasters.Proposal
{
    public class RewardDataSerialization
    {
        private readonly string saveKey;

        public RewardDataSerialization(string _saveKey)
        {
            saveKey = _saveKey;
        }


        public RewardData Data
        {
            get
            {
                RewardData result = default;
                string jsonItem =
                    CustomPlayerPrefs.GetObjectValue<string>(saveKey);
                if (string.IsNullOrEmpty(jsonItem))
                {
                    CustomDebug.Log("Empty or NULL value");
                }
                else
                {
                    result = RewardData.FromJson<RewardData>(jsonItem);
                    if (result == null)
                    {
                        CustomDebug.Log("Deseralized object NULL");
                    }
                }

                return result;
            }

            set
            {
                string jsonItem = default;
                if (value == null)
                {
                    CustomDebug.Log("Try serialize NULL value");
                }
                else
                {
                    jsonItem = JsonUtility.ToJson(value);
                    if (string.IsNullOrEmpty(jsonItem))
                    {
                        CustomDebug.Log("Json is NULL or empty");
                    }
                }

                CustomPlayerPrefs.SetObjectValue(saveKey, jsonItem);
            }
        }
    }
}
