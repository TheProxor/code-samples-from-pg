using System;
using System.Collections.Generic;


namespace Drawmasters.Advertising
{
    public class PlacementGroup : IDeinitializable
    {
        #region Fields

        public event Action<IBannerPlacement> OnPlacementReady;
        public event Action<IBannerPlacement> OnPlacementShouldHide;

        private readonly IReadOnlyCollection<IBannerPlacement> placements;

        #endregion



        #region Ctor

        public PlacementGroup(IBannerPlacement[] _placements)
        {
            placements = _placements;

            foreach(var p in placements)
            {
                p.OnPlacementReady += P_OnPlacementReady;
                p.OnPlacementShouldHide += P_OnPlacementShouldHide;
            }
        }

        

        #endregion



        #region IDeinitialize

        public void Deinitialize()
        {
            foreach (var p in placements)
            {
                p.OnPlacementReady -= P_OnPlacementReady;
                p.OnPlacementShouldHide -= P_OnPlacementShouldHide;
            }
        }

        #endregion



        #region Events handlers

        private void P_OnPlacementReady(IBannerPlacement readyPlacement) =>
            OnPlacementReady?.Invoke(readyPlacement);


        private void P_OnPlacementShouldHide(IBannerPlacement readyPlacement) =>
            OnPlacementShouldHide?.Invoke(readyPlacement);

        #endregion
    }
}

