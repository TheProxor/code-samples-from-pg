using UnityEngine;
using UnityEngine.UI;
using Drawmasters.Statistics.Data;
using Drawmasters.ServiceUtil;

namespace Drawmasters.Ua
{
    public class UaGuiSelector : MonoBehaviour
    {
        #region Unity lifecycle        

        private void OnEnable()
        {
            Image selectorImage = gameObject.GetComponent<Image>();

            PlayerData playerData = GameServices.Instance.PlayerStatisticService.PlayerData;

            if (selectorImage != null)
            {
                selectorImage.enabled = playerData.IsBloodEnabled;
            }
        }        

        #endregion
    }
}
