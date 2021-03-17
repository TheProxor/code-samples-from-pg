using System;
using Spine.Unity;
using UnityEngine;


namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName = "SkinsColorSettings",
                     menuName = NamingUtility.MenuItems.IngameSettings + "SkinsColorSettings")]
    public class SkinsColorSettings : ScriptableObjectData<SkinsColorSettings.Data, SkeletonDataAsset>
    {
        #region Nested types

        [Serializable]
        public class Data : ScriptableObjectBaseData<SkeletonDataAsset>
        {
            public AttachmentsGroups[] attachmentsGroups = default;
        }


        [Serializable]
        public class AttachmentsGroups
        {
            [SpineSlot(dataField: "skeletonRenderer")] public string slotName = default;

            [SerializeField] private AttachmentsColors[] attachmentsData = default;

            public string FindAttachmentName(ShooterColorType type)
            {
                AttachmentsColors foundData = Array.Find(attachmentsData, e => e.colorType == type);
                return foundData == null ? default : foundData.attachmentName;
            }
        }


        [Serializable]
        private class AttachmentsColors
        {
            public ShooterColorType colorType = default;

            [SpineAttachment(slotField: "slotName", dataField: "skeletonRenderer", fallbackToTextField: true)]
            public string attachmentName = default;
        }

        #endregion



        #region Fields

        #pragma warning disable 0414

        [Tooltip("Use for serialization only")]
        [SerializeField] private SkeletonDataAsset skeletonRenderer = default;

        #pragma warning restore 0414

        #endregion



        #region Methods

        public AttachmentsGroups[] FindAttachmentsGroups(SkeletonDataAsset dataAsset)
        {
            Data settings = FindData(dataAsset);
            return settings == null ? default : settings.attachmentsGroups;
        }

        #endregion
    }
}
