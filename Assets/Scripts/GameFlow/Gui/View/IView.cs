namespace Drawmasters
{
    public interface IView
    {
        #region Properties

        ViewType Type { get; }

        int SortingOrder
        {
            get;
            set;
        }


        float ZPosition
        {
            get;
            set;
        }

        #endregion



        #region Methods

        void SetVisualOrderSettings();


        void ResetVisualOrderSettings();

        #endregion
    }
}
