using Spine;
using Spine.Unity;
using System;
using UnityEngine;


namespace Drawmasters
{
    [Serializable]
    public class DecalInfo
    {
        #region Fields

        [SpineSlot]
        [SerializeField] private string slotName = default;

        [SpineAttachment]
        [SerializeField] private string attachmentName = default;

        #endregion



        #region Methods

        public void ChangeAttachment(SkeletonAnimation workAnimation)
        {
            if (string.IsNullOrEmpty(slotName) ||
                string.IsNullOrEmpty(attachmentName))
            {
                return;
            }

            Attachment attachment = workAnimation.Skeleton.GetAttachment(slotName, attachmentName);
            if (attachment == null)
            {
                return;
            }

            workAnimation.Skeleton.SetAttachment(slotName, attachmentName);
        }

        #endregion
    }
}
