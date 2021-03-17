using System.Collections.Generic;
using System;


namespace Drawmasters.Levels
{
    public class RopeStageComponent : RopeComponent
    {
        #region Fields

        public static event Action<Rope> OnObjectsCame;

        private LevelObject hookObject;
        private LevelObject mainObject;

        private List<LevelObject> objects;
        private int camedObjectsCount;

        #endregion



        #region Methods

        public override void Initialize(Rope _rope)
        {
            base.Initialize(_rope);

            rope.OnLinksSet += Rope_OnLinksSet;
            objects = new List<LevelObject>();
        }


        public override void Enable()
        {
            RopeCreateComponent.OnRopeGenerated += RopeCreateComponent_OnRopeGenerated;

            foreach (var o in objects)
            {
                o.OnStageCame += Object_OnStageCame;
            }
        }


        public override void Disable()
        {
            foreach (var o in objects)
            {
                if (o != null)
                {
                    o.OnStageCame -= Object_OnStageCame;
                }
            }

            objects.Clear();

            rope.OnLinksSet -= Rope_OnLinksSet;

            RopeCreateComponent.OnRopeGenerated -= RopeCreateComponent_OnRopeGenerated;
        }

        #endregion



        #region Events handlers

        private void RopeCreateComponent_OnRopeGenerated(Rope anotherRope, List<RopeSegment> _ropeSegments)
        {
            if (rope == anotherRope)
            {
                camedObjectsCount = 0;
            }
        }


        private void Rope_OnLinksSet(List<LevelObject> linkedObjects)
        {
            if (linkedObjects.Count > 0)
            {
                mainObject = linkedObjects[0];
                objects.Add(mainObject);
            }

            if (linkedObjects.Count > 1)
            {
                hookObject = linkedObjects[1];
                objects.Add(hookObject);
            }
        }

        private void Object_OnStageCame()
        {
            camedObjectsCount++;

            if (camedObjectsCount == objects.Count)
            {
                OnObjectsCame?.Invoke(rope);
            }
        }

        #endregion
    }
}
