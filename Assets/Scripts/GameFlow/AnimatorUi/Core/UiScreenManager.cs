using System;
using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.Ui
{
    public class UiScreenManager : SingletonMonoBehaviour<UiScreenManager>
    {
        #region Fields

        private const float DefaultViewsDeletion = 0.04f;

        private readonly List<AnimatorScreen> activeScreens = new List<AnimatorScreen>();

        #endregion



        #region Methods

        //TODO improve
        public static bool IsTouchable()
        {
            // Predominantly for editor case
            if (!HasFoundInstance)
            {
                return true;
            }

            return Instance.activeScreens != null &&
                   Instance.activeScreens.Count == 1 &&
                   Instance.activeScreens[0] != null &&
                   Instance.activeScreens[0] is IngameScreen ingame &&
                   ingame.ScreenType == ScreenType.Ingame;
        }


        public void HideAll(bool isImmediately = false)
        {
            for (int i = activeScreens.Count - 1; i >= 0; i--)
            {
                AnimatorScreen screen = activeScreens[i];

                if (isImmediately)
                {
                    screen.HideImmediately();
                }
                else
                {
                    screen.Hide();
                }
            }
        }


        public AnimatorScreen ShowPopup(OkPopupType popupType,
                                        Action<AnimatorView> onShowed = null,
                                        Action<AnimatorView> onHided = null,
                                        bool isForceHideIfExist = false,
                                        Action<AnimatorView> onShowBegin = null,
                                        Action<AnimatorView> onHideBegin = null)
        {
            onShowBegin += view =>
            {
                OkayScreen screen = view as OkayScreen;

                DialogPopupSettings.DialogSettingsContainer settings = IngameData.Settings.dialogPopupSettings.GetSettings(popupType);

                if (settings != null)
                {
                    screen.ChangeText(settings.contentText);
                    screen.ChangeHeader(settings.headerText);
                }
            };

            return ShowScreen(ScreenType.OkayScreen, onShowed, onHided, onShowBegin, onHideBegin, isForceHideIfExist);
        }


        public AnimatorScreen ShowScreen(ScreenType type,
                                         Action<AnimatorView> onShowed = null,
                                         Action<AnimatorView> onHided = null,
                                         Action<AnimatorView> onShowBegin = null,
                                         Action<AnimatorView> onHideBegin = null,
                                         bool isForceHideIfExist = true)
        {
            bool isScreenExist = IsScreenActive(type);

            if (isScreenExist)
            {
                if (isForceHideIfExist)
                {
                    AnimatorScreen existScreen = LoadedScreen<AnimatorScreen>(type);

                    if (existScreen != null)
                    {
                        existScreen.HideImmediately();
                    }
                }
                else
                {
                    CustomDebug.Log($"Trying to show already active screen with type {type}");

                    return null;
                }
            }

            GameObject prefabToShow = ScreenPrefabByType(type);

            if (prefabToShow == null)
            {
                CustomDebug.Log($"Screen prefab missing for type {type}");

                return null;
            }

            AnimatorScreen screenToShow = Instantiate(prefabToShow).GetComponent<AnimatorScreen>();

            CommonUtility.SetObjectActive(screenToShow.gameObject, false);

            activeScreens.Add(screenToShow);

            onHided += hidedView =>
                {
                    ViewManager.Instance.RemoveLastViewInfo(hidedView);

                    RemoveScreenFromActiveList(screenToShow);
                };

            screenToShow.Initialize(onShowed, 
                                    onHided, 
                                    onShowBegin, 
                                    onHideBegin);

            ViewManager.Instance.AddViewInfo(screenToShow);

            screenToShow.Show();

            return screenToShow;
        }


        public void HideScreen(ScreenType type, Action<AnimatorView> callback = null)
        {
            var foundScreen = LoadedScreen<AnimatorScreen>(type);

            if (foundScreen != null)
            {
                foundScreen.Hide(callback, null);
            }
        }


        public void HideScreenImmediately(ScreenType type, Action<AnimatorView> callback = null)
        {
            var foundScreen = LoadedScreen<AnimatorScreen>(type);

            if (foundScreen != null)
            {
                foundScreen.HideImmediately(callback);
            }
        }


        public bool IsScreenActive(ScreenType type) => LoadedScreen<AnimatorScreen>(type) != null;


        public T LoadedScreen<T>(ScreenType type) where T : AnimatorScreen
        {
            AnimatorScreen result = activeScreens.Find(element => element.ScreenType == type);

            return (result as T);
        }


        private GameObject ScreenPrefabByType(ScreenType type)
            => Content.Storage.PrefabByType(type);


        private void RemoveScreenFromActiveList(AnimatorScreen screen)
        {
            if (activeScreens.Count > 0)
            {
                activeScreens.Remove(screen);
                screen.Deinitialize();
                
                //TODO check maxim.ak
                Destroy(screen.gameObject, DefaultViewsDeletion);
            }
        }

        #endregion
    }
}