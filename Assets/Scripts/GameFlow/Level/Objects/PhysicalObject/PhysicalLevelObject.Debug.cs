namespace Drawmasters.Levels
{
    public partial class PhysicalLevelObject
    {
        #if UNITY_EDITOR

        #region Methods

        [Sirenix.OdinInspector.Button]
        private void DebugRefreshData()
        {
            RefreshData();
        }

        #endregion

        #endif
    }
}
