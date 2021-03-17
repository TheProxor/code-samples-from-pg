using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public abstract class DrawerBase<T> : MonoBehaviour where T : EditorLevelObject
    {
        #region Fields

        protected T selectedObject;

        private Coroutine drawCoroutine;

        #endregion



        #region Unity lifecycle

        protected void OnEnable() => SelectedObjectChange.Subscribe(OnSelectedObjectChange);

        protected void OnDisable() => SelectedObjectChange.Unsubscribe(OnSelectedObjectChange);

        protected void Awake() => Initialize();

        #endregion



        #region Methods

        protected virtual void Initialize() => OnSelectedObjectRemoved();


        protected virtual void OnSelectedObjectRemoved()
        {
            selectedObject = null;

            if (drawCoroutine != null)
            {
                StopCoroutine(drawCoroutine);
                drawCoroutine = null;
            }
        }


        protected virtual void OnSelectedObjectAdded(T newObject) => selectedObject = newObject;


        protected abstract void Draw();
        

        private IEnumerator OnUpdate()
        {
            while (selectedObject != null)
            {
                Draw();

                yield return null;
            }

            OnSelectedObjectRemoved();
        }

        #endregion



        #region Events handlers

        private void OnSelectedObjectChange(List<EditorLevelObject> selectedObjects)
        {
            if (selectedObjects?.LastObject() is T newObject)
            {
                OnSelectedObjectAdded(newObject);

                if (drawCoroutine == null)
                {
                    StartCoroutine(OnUpdate());
                }
            }
            else
            {
                OnSelectedObjectRemoved();
            }
        }

        #endregion
    }
}
