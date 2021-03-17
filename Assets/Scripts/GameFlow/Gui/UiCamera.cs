using Drawmasters.Interfaces;
using Drawmasters.Levels.Transition;
using Drawmasters.Ui.Transition;
using UnityEngine;


namespace Drawmasters.Ui
{
    public class UiCamera : SingletonMonoBehaviour<UiCamera>, IInitializable
    {
        #region Fields

        [SerializeField] private Camera uiCamera = default;

        #endregion



        #region Properties

        public Camera Camera => uiCamera;

        public IAction ScaleIn { get; private set; }

        public IAction ScaleOut { get; private set; }

        private ICreate<AnimatorView> SkullCreator { get; set; }

        #endregion



        #region IInitializable

        public void Initialize()
        {
            SkullCreator = new TransitionViewCreator(uiCamera.transform);
            SkullCreator.Create();

            ScaleIn = new MeshScaleIn(SkullCreator.CreatedObject);
            ScaleOut = new MeshScaleOut(SkullCreator.CreatedObject);
        }
        #endregion

    }
}
