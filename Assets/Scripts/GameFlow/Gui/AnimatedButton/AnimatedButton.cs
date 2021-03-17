using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Drawmasters.Ui
{
    public class AnimatedButton : Button
    {
        #region Overrided methods

        public override void OnPointerEnter(PointerEventData eventData){}

        public override void OnPointerExit(PointerEventData eventData){}

        public override void OnSelect(BaseEventData eventData){}

        public override void OnDeselect(BaseEventData eventData){}

        protected override void OnEnable()
        {
            if (s_SelectableCount == s_Selectables.Length)
            {
                Selectable[] temp = new Selectable[s_Selectables.Length * 2];
                Array.Copy(s_Selectables, temp, s_Selectables.Length);
                s_Selectables = temp;
            }
            m_CurrentIndex = s_SelectableCount;
            s_Selectables[m_CurrentIndex] = this;
            s_SelectableCount++;
            //TODO check
            // isPointerDown = false;
        }

        #endregion
    }
}

    
