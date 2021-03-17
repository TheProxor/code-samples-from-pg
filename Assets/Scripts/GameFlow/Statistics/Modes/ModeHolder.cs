namespace Drawmasters.Prefs
{
    public class ModeHolder : InfoHolder<ModeInfo, GameMode>
    {
        public ModeHolder(string _prefsKey) :
            base(_prefsKey)
        {
        }
    }
}