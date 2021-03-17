using Drawmasters.Levels;
using Drawmasters.ServiceUtil;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.Ui
{
    public class PathProgressBar : MonoBehaviour, IInitializable, IDeinitializable
    {
        #region Fields

        [SerializeField] private GameObject rootBar = default;
        [SerializeField] private Image fillImage = default;

        private LevelPathController pathController;

        #endregion



        #region IInitialize

        public void Initialize()
        {
            pathController = GameServices.Instance.LevelControllerService.Path;
            
            if (pathController.IsControllerEnabled)
            {
                pathController.OnPathChanged += PathController_OnPathChanged; 
            }
            
            CommonUtility.SetObjectActive(rootBar, pathController.IsControllerEnabled);
            
            ChangeFillProgess(1f);
        }


        #endregion


        #region IDeinitialize
        public void Deinitialize()
        {
            if (pathController.IsControllerEnabled)
            {
                pathController.OnPathChanged -= PathController_OnPathChanged;
            }
        }
        
        #endregion



        #region Private methods

        private void ChangeFillProgess(float progress)
        {
            fillImage.fillAmount = progress;
        }

        #endregion
        
        
        
        #region Events handlers
        private void PathController_OnPathChanged(float pathFactor)
        {
            ChangeFillProgess(1f - pathFactor);
        }
        
        #endregion
    }
}

