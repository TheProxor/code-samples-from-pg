using Drawmasters.Interfaces;
using UnityEngine;


namespace Drawmasters.Ui.Transition
{
    public class TransitionViewCreator : ICreate<AnimatorView>
    {
        #region Fields

        private readonly Transform root;

        #endregion



        #region Ctor

        public TransitionViewCreator(Transform _root)
        {
            root = _root;
        }

        #endregion



        #region ICreate

        public AnimatorView CreatedObject { get; private set; }


        public void Create()
        {
            GameObject original = Content.Storage.PrefabByType(CustomPrefabType.TransitionView);

            CreatedObject = Object.Instantiate(original).GetComponent<AnimatorView>();
            CreatedObject.transform.SetParent(root);
            CreatedObject.transform.localPosition = Vector3.zero;
            CreatedObject.gameObject.name = original.name;

            CreatedObject.Initialize();
        }

        #endregion
    }
}
