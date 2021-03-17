using Drawmasters.Levels.Objects;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class MarkerInspectorExtension : InspectorExtensionBase
    {
        [SerializeField] private OptionsChoiceUI typeDropdown = null;


        private EditorMarker marker;


        public override void Init(EditorLevelObject levelObject)
        {
            marker = (EditorMarker)levelObject;
            typeDropdown.Init("Type", new List<string>(Enum.GetNames(typeof(MarkerType))), (int)marker.Type);
        }


        protected override void SubscribeOnEvents()
        {
            typeDropdown.OnValueChanged += SetMarkerType;
        }


        protected override void UnsubscribeFromEvents()
        {
            typeDropdown.OnValueChanged -= SetMarkerType;
        }


        private void SetMarkerType(int type)
        {
            marker.Type = (MarkerType)type;
        }
    }
}
