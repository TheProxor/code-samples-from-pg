using UnityEngine;


namespace Drawmasters.Ui
{
    public interface IUiSwipesRect
    {
        RectTransform SwipeRect { get; }
        RectTransform ScalableSwipeRect { get; }

        void SetButtonsEnabled(bool isEnabled);
    }
}