using Drawmasters.Ui;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Drawmasters
{
    public class ViewManager : SingletonMonoBehaviour<ViewManager>
    {
        #region Fields

        public const int OrderOffset = 10;

        public static Action OnViewInfosChanged;

        private const float ZOffset = 100f;

        private const int DirectionMultiplier = -1;

        private const int AdditionalSortingOrderValueForOverlay = 1;
        private const int AdditionalZDistanceForOverlay = 1;

        private readonly List<AnimatorView> views = new List<AnimatorView>();

        #endregion



        #region Properties

        public int SortingOrderForOverlay { get; private set; }

        public float ZDistanceForOverlay { get; private set; }

        #endregion



        #region Methods

        public void AddViewInfo(AnimatorView viewInfo)
        {
            views.Add(viewInfo);

            RecalculateViewInfos();            
        }


        public void RemoveLastViewInfo(AnimatorView animatorView)
        {
            bool isContain = views.Contains(animatorView);

            if (isContain)
            {
                views.Remove(animatorView);
            }

            RecalculateViewInfos();
        }


        float CalculateZPosition(int index) => ZOffset * index * DirectionMultiplier;


        int CalculateSortingOrder(int itemIndex) => OrderOffset * itemIndex;


        void RecalculateViewInfos()
        {
            views.RemoveAll(view => view.IsNull());
                        
            for (int i = 0; i < views.Count; i++)
            {
                int order = i + 1;

                views[i].SortingOrder = CalculateSortingOrder(order);
                views[i].ZPosition = CalculateZPosition(order);

                views[i].SetVisualOrderSettings();
            }

            if (views.Count > 0)
            {
                SortingOrderForOverlay = views.Max(element => element.SortingOrder) + AdditionalSortingOrderValueForOverlay;
                ZDistanceForOverlay = views.Min(element => element.ZPosition) + (AdditionalZDistanceForOverlay * DirectionMultiplier);
            }
        }

        #endregion
    }
}
