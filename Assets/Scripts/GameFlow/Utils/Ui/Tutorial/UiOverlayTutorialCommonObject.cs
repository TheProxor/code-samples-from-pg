using UnityEngine;


namespace Drawmasters.Ui
{
    public class UiOverlayTutorialCommonObject : IUiOverlayTutorialObject
    {
        public UiOverlayTutorialCommonObject(GameObject _gameObject) =>
            OverlayTutorialObject = _gameObject;


       public GameObject OverlayTutorialObject { get; }
    }
}
