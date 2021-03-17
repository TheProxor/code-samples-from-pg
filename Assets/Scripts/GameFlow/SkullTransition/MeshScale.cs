using System;
using Drawmasters.Ui;


namespace Drawmasters.Levels.Transition
{
    public abstract class MeshScale : IAction
    {
        #region Fields

        protected readonly AnimatorView view;

        protected Action callback;

        #endregion



        #region Ctor

        public MeshScale(AnimatorView transitionView)
        {
            view = transitionView;
        }

        #endregion



        #region IAction

        public virtual void Run(Action ended)
        {
            callback = ended;
        }

        #endregion
    }
}