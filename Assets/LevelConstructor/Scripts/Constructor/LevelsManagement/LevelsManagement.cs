using System;
using System.Text;
using System.Collections.Generic;
using Drawmasters.LevelsRepository;
using Drawmasters.LevelsRepository.Editor;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class LevelsManagement : MonoBehaviour
    {
        public static event Action<LevelHeader> OnOpeneLevelRequest;

        [SerializeField] LevelSelectorCanvas selector = default;


        void OnEnable() => LevelSelectorCanvas.OnOpenLevelClick += OnOpenLevelRequest;


        void OnDisable() => LevelSelectorCanvas.OnOpenLevelClick -= OnOpenLevelRequest;


        public void Init(LevelHeader header) => selector.Init(header);


        void OnOpenLevelRequest(LevelHeader headerToOpen)
        {
            OnOpeneLevelRequest?.Invoke(headerToOpen);
        }
    }
}
