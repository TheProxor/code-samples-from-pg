using Core;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class ChangeState : GlobalEvent<ChangeState, LevelEditor.State> { }

    public class SelectedObjectChange : GlobalEvent<SelectedObjectChange, List<EditorLevelObject>> { }

    public class LinkSettingRequest : GlobalEvent<LinkSettingRequest, bool> { }

    public class LinkSettingFinished : GlobalEvent<LinkSettingFinished> { }

    public class LinkSettingRefreshed : GlobalEvent<LinkSettingRefreshed> { }

    public class StartCameraPositionChange : GlobalEvent<StartCameraPositionChange, Vector3, Action<Vector3>> { }

    public class EditorLevelStageChange : GlobalEvent<EditorLevelStageChange, int> { }
}
