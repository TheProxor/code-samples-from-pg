namespace Drawmasters.Utils
{
    public static class PrefsUtility
    {
        // TODO: naming
        public static bool TryDefineRestoreNeedInt(string prefsKey, out int previousValue)
        {
            previousValue = -1;
            bool wasValueExists = CustomPlayerPrefs.HasKey(prefsKey);
            if (wasValueExists)
            {
                previousValue = CustomPlayerPrefs.GetInt(prefsKey);
                CustomPlayerPrefs.DeleteKey(prefsKey);
                return true;
            }

            return wasValueExists;
        }


        public static bool TryRestoreInt(string oldPrefsKey, string targetPrefsKey)
        {
            bool wasValueExists = CustomPlayerPrefs.HasKey(oldPrefsKey);
            if (wasValueExists)
            {
                int restoredValue = CustomPlayerPrefs.GetInt(oldPrefsKey);

                CustomPlayerPrefs.DeleteKey(PrefsKeys.Proposal.HotmastersLevelsDelta);
                CustomPlayerPrefs.SetInt(targetPrefsKey, restoredValue);
            }

            return wasValueExists;
        }
    }
}
