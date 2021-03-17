using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;


namespace Drawmasters.Proposal
{
    [CreateAssetMenu(fileName = "ChestSettings", 
        menuName = NamingUtility.MenuItems.ProposalSettings + "ChestSettings")]
    public class ChestSettings : SerializedScriptableObject
    {
        #region Fields

        [SerializeField] private Dictionary<ChestType, ChestData> chestsData = default;

        #endregion
        
        
        
        #region Public methods

        public ChestData GetChestData(ChestType chestType)
        {
            if (!chestsData.TryGetValue(chestType, out ChestData result))
            {
                CustomDebug.Log($"Cannot find data for chest. Chest type: {chestType}");
            }

            return result;
        }
        
        
        #endregion
    }
}