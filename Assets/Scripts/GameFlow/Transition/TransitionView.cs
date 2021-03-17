using UnityEngine;


namespace Drawmasters.Ui
{
    public class TransitionView : AnimatorView
    {
        #region Abstract implemention

        public override ViewType Type => ViewType.TransitionView;

        protected override string ShowKey => AnimationKeys.Screen.Show;

        protected override string HideKey => AnimationKeys.Screen.Hide;

        public override void SetVisualOrderSettings() { }

        public override void ResetVisualOrderSettings() { }

        protected override void InitPosition()
        {
            transform.localScale = Vector3.one;
        }

        #endregion
    }
}
