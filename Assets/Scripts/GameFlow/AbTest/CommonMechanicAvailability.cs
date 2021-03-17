using Drawmasters.Interfaces;


namespace Drawmasters.AbTesting
{
    public class CommonEnabledMechanicAvailability : CommonMechanicAvailability
    {
        protected override bool DefaultValue => true;

        public CommonEnabledMechanicAvailability(string prefsKey) : base(prefsKey)
        {
            
        }
    }
    public class CommonMechanicAvailability : IUaAbTestMechanic
    {
        #region Fields
        
        private readonly string key;
        
        #endregion
        
        
        
        #region Ctor
        
        public CommonMechanicAvailability(string prefsKey)
        {
            key = prefsKey;
        }
        
        #endregion

        protected virtual bool DefaultValue => false; 
        

        #region IUaAbTestMechanic

        public bool IsMechanicAvailable
        {
            get
            {
                bool result = DefaultValue;
                
                #if UNITY_EDITOR || UA_BUILD || DEBUG || DEBUG_TARGET
                    result = true;
                #endif
                
                result &= CustomPlayerPrefs.GetBool(key, DefaultValue);

                return result;
            }
        }

        public bool WasAvailabilityChanged =>
            CustomPlayerPrefs.HasKey(key);
        
        public void ChangeMechanicAvailability(bool isAvailable) =>
            CustomPlayerPrefs.SetBool(key, isAvailable);

        public void ResetAvailability() =>
            CustomPlayerPrefs.DeleteKey(key);

        #endregion
    }
}
