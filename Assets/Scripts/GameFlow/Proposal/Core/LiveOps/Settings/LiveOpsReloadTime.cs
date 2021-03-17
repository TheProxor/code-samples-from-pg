using System;
using UnityEngine;
using Newtonsoft.Json;


namespace Drawmasters.Proposal
{
    [Serializable]
    public class LiveOpsReloadTime
    {
        private const float MinOffsetSeconds = 0.0f;

        public int day = default;
        public int hours = default;
        public int minutes = default;


        [JsonIgnore]
        public float ReloadSeconds
        {
            get
            {
                DateTime now = DateTime.Now;
                DateTime nextDateTime = DateTime.Now.Date + ReloadTimeSpan;

                float offsetSeconds = (float)nextDateTime.Subtract(now).TotalSeconds;
                return Mathf.Max(MinOffsetSeconds, offsetSeconds); 
            }
        }

        private TimeSpan ReloadTimeSpan =>
            new TimeSpan(day, hours, minutes, 0);
    }
}
