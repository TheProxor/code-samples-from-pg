using System;
using Drawmasters.Ui;


namespace Drawmasters.Levels.Transition
{
    public class MeshScaleOut : MeshScale
    {
        #region Ctor

        public MeshScaleOut(AnimatorView transitionView)
            : base(transitionView) { }

        #endregion



        #region Methods

        public override void Run(Action ended)
        {
            base.Run(ended);

            if (view != null)
            {
                view.Hide(view => ended?.Invoke(), null);
            }
        }


        #endregion
    }
}