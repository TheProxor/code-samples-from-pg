using DG.Tweening;
using Drawmasters.Proposal;
using Spine.Unity;
using TMPro;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class ForcemeterRewardElement : MonoBehaviour
    {
        [SerializeField] private Transform root = default;
        [SerializeField] private Transform scaleIconRoot = default;
        [SerializeField] private SpriteRenderer spriteRenderer = default;
        [SerializeField] private TMP_Text rewardText = default;

        [Header("Animation (spine)")]
        [SerializeField] private SkeletonAnimation forcemeterSkeleton = default;
        [SpineSlot(dataField: "skeletonRenderer", includeNone: true)]
        [SerializeField] private string textSlotName = default;
        [SpineAttachment(slotField: "slotName", dataField: "skeletonRenderer", fallbackToTextField: true, includeNone: true)]
        [SerializeField] private string textAttachmentName = default;

        [Header("Animation (unity)")]
        [SerializeField] private VectorAnimation scaleInCurrencyAnimation = default;
        [SerializeField] private VectorAnimation scaleOutCurrencyAnimation = default;

        public Vector3 IconCurrencyPosition => root.position;
        public RewardData RewardData { get; private set; }



        public void Initialize(RewardData rewardData)
        {
            RewardData = rewardData;

            Sprite spriteToSet = default;
            if (rewardData is IForcemeterReward forcemeterReward)
            {
                spriteToSet = forcemeterReward.GetForcemeterRewardSprite();
            }

            spriteRenderer.sprite = spriteToSet;

            bool isCurrencyReward = rewardData is CurrencyReward;

            SetAttachmentEnabled(isCurrencyReward);
            rewardText.text = isCurrencyReward ? ((CurrencyReward)rewardData).UiRewardText : string.Empty;
        }


        public void Deinitialize()
        {
            DOTween.Kill(this);
        }


        public void PlayReceiveAnimation()
        {
            scaleInCurrencyAnimation.Play((value) => scaleIconRoot.localScale = value, this);
            scaleOutCurrencyAnimation.Play((value) => scaleIconRoot.localScale = value, this);
        }


        private void SetAttachmentEnabled(bool enabled)
        {
            string attachmentName = enabled ? textAttachmentName : null;
            forcemeterSkeleton.skeleton.SetAttachment(textSlotName, attachmentName);
        }
    }
}
