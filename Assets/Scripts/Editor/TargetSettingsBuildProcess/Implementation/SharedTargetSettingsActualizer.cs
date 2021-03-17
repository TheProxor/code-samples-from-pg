namespace Modules.BuildProcess
{
    public class SharedTargetSettingsActualizer : ConcreteTargetSettingsActualizer
    {
        #region Properties

        protected override bool NeedRemoveUnusedFolders => false;

        #endregion
        
        
        
        #region Ctor
        
        public SharedTargetSettingsActualizer(string folderName) : base(folderName) { }
        
        #endregion
    }
}

