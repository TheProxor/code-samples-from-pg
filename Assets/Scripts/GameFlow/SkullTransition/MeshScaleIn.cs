using System;
using Drawmasters.Ui;


namespace Drawmasters.Levels.Transition
{
    public class MeshScaleIn : MeshScale
    {
        #region Ctor

        public MeshScaleIn(AnimatorView transitionView)
            : base(transitionView) { }

        #endregion



        #region Methods

        public override void Run(Action ended)
        {
            base.Run(callback);

            if (view != null)
            {
                view.HideImmediately();
                view.Show(view => ended?.Invoke(), null);
            }
            else
            {
                CustomDebug.Log("View in null");
                ended?.Invoke();
            }
        }

        #endregion
    }
}