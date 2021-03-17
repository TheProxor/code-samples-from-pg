using System;
using Drawmasters.Utils;
using Spine;
using Spine.Unity;


namespace Drawmasters.Helpers
{
    public class SpineAnimationSequencePlayer
    {
        [Serializable]
        public class Data
        {
            public SkeletonGraphic skeletonGraphic = default;

            [SpineAnimation(dataField = "skeletonGraphic")]
            public string[] animationsSequence = default;
        }



        public void Play(Data data, int animationsIndex = 0, bool shouldLoopEnd = true)
        {
            for (int i = 0; i < data.animationsSequence.Length; i++)
            {
                string animationName = data.animationsSequence[i];

                bool isFirst = i == 0;
                bool isLast = i == data.animationsSequence.Length - 1;

                bool shouldLoop = shouldLoopEnd && isLast;

                if (isFirst)
                {
                    data.skeletonGraphic.Initialize(true);
                    SpineUtility.SafeSetAnimation(data.skeletonGraphic, animationName, animationsIndex, shouldLoop);
                }
                else
                {
                    Animation animationToAdd = data.skeletonGraphic.SkeletonData.FindAnimation(data.animationsSequence[i - 1]);
                    float prevAnimationDelay = animationToAdd == null ? default : animationToAdd.Duration;
                    SpineUtility.SafeAddAnimation(data.skeletonGraphic, animationName, animationsIndex, shouldLoop, default, prevAnimationDelay);
                }
            }
        }
    }
}
