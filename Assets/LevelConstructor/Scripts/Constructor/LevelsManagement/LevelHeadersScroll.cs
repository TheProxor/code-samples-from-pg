using Drawmasters.LevelsRepository;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.LevelConstructor
{
    [RequireComponent(typeof(ScrollRect))]
    public class LevelHeadersScroll : MonoBehaviour
    {
        public event Action<LevelHeader, bool> OnSelect;

        [SerializeField] LevelHeaderPanel prefab = default;

        readonly List<LevelHeaderPanel> panels = new List<LevelHeaderPanel>();
        ScrollRect scrollRect;
        LevelHeaderPanel selected;


        void Awake() => scrollRect = GetComponent<ScrollRect>();


        public void Set(LevelHeader[] headers, LevelHeader selectedHeader)
        {
            panels.ForEach(p => p.Clear());
            selected = null;


            if (headers == null || headers.Length == 0)
            {
                OnSelect?.Invoke(null, true);
                return;
            }

            for (int i = 0; i < headers.Length; i++)
            {
                if (i == panels.Count)
                {
                    panels.Add(Instantiate(prefab, scrollRect.content));
                }

                panels[i].Set(i, headers[i], (header) => Select(header, false));
            }

            if (selectedHeader == null)
            {
                Select(panels[0], true);
                scrollRect.verticalNormalizedPosition = 1.0f;
            }
            else
            {
                int selectedPanelIndex = panels.FindIndex((panel) => panel.Header == selectedHeader);
                scrollRect.verticalNormalizedPosition = 1 - ((float)selectedPanelIndex) / (panels.Count((p) => p.gameObject.activeSelf) - 1);
                Select(panels[selectedPanelIndex], true);
            }
        }

        void Select(LevelHeaderPanel panel, bool isInitialization)
        {
            selected?.Select(false);
            selected = panel;
            selected?.Select(true);

            OnSelect?.Invoke(panel.Header, isInitialization);
        }
    }
}
