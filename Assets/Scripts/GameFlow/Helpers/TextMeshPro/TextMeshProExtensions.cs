using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


namespace Drawmasters.Utils
{
    public static class TextMeshProExtensions
    {
        /// <summary>
        /// Allow to apply underlay and outline in short way. Allocates new material.
        /// </summary>
        public static void SetupUnderlayColor(this TMP_Text text, Color color, bool shouldApplyOutline)
        {

            Material savedMaterial = text.fontMaterial;

            if (shouldApplyOutline)
            {
                text.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, color);
            }

            text.fontMaterial.SetColor(ShaderUtilities.ID_UnderlayColor, color);
            text.fontMaterial = new Material(savedMaterial);
        }
    }
}
