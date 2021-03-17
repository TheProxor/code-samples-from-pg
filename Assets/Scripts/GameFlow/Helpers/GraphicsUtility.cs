using System;
using System.Collections;
using System.Linq;
using UnityEngine;


namespace Drawmasters.Helpers
{
    public static class GraphicsUtility
    {
        #region Methods

        public static Texture2D GetColorTexture(Color color, 
            Texture2D source,
            Material material)
        {
            Material coloredMaterial = new Material(material);
            coloredMaterial.SetColor("_Color", color);            

            RenderTexture temporaryTexture = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);

            RenderTexture.active = temporaryTexture;
            GL.Clear(false, true, Color.clear);
            
            Graphics.Blit(source, temporaryTexture, coloredMaterial);

            Texture2D result = new Texture2D(source.width, source.height, TextureFormat.ARGB32, false)
            {
                anisoLevel = source.anisoLevel,
                wrapMode = source.wrapMode,
                filterMode = source.filterMode
            };
            
            Graphics.CopyTexture(temporaryTexture, 0, 0, result, 0, 0);

            RenderTexture.ReleaseTemporary(temporaryTexture);
            RenderTexture.active = null;

            return result;
        }


        public static Gradient GetReversedGradient(Gradient gradient)
        {
            Gradient result = new Gradient();

            GradientColorKey[] colorKey = gradient.colorKeys;
            GradientAlphaKey[] alphaKey = gradient.alphaKeys;

            int colorIterator = colorKey.Length - 1;

            for (int i = 0; i < colorKey.Length; i++)
            {
                colorKey[i].time = colorKey[colorIterator].time;
                colorIterator--;
            }

            int alphaIterator = alphaKey.Length - 1;

            for (int i = 0; i < alphaKey.Length; i++)
            {
                alphaKey[i].time = alphaKey[alphaIterator].time;
                alphaIterator--;
            }

            result.SetKeys(colorKey, alphaKey);
            return result;
        }


        public static Gradient GetAlphaGradient()
        {
            Gradient gradient = new Gradient();

            GradientColorKey[] colorKey = new GradientColorKey[1];
            colorKey[0].color = Color.white;
            colorKey[0].time = 0.0f;

            GradientAlphaKey[] alphaKey = new GradientAlphaKey[1];
            alphaKey[0].alpha = 0.0f;
            alphaKey[0].time = 0.0f;

            gradient.SetKeys(colorKey, alphaKey);
            return gradient;
        }


        public static Gradient GetSolidGradient(Color color, float alpha = 1.0f)
        {
            Gradient gradient = new Gradient();

            GradientColorKey[] colorKey = new GradientColorKey[2];
            colorKey[0].color = color;
            colorKey[0].time = 0.0f;
            colorKey[1].color = color;
            colorKey[1].time = 1.0f;

            GradientAlphaKey[] alphaKey = new GradientAlphaKey[2];
            alphaKey[0].alpha = alpha;
            alphaKey[0].time = 0.0f;
            alphaKey[1].alpha = alpha;
            alphaKey[1].time = 1.0f;

            gradient.SetKeys(colorKey, alphaKey);
            return gradient;
        }

        
        public static Gradient GetAlphaGradient(Color color, float ofset, float lag)
        {
            Gradient gradient = new Gradient();

            GradientColorKey[] colorKey = new GradientColorKey[4];

            float point1 = ofset + lag;
            float point2 = 0.97f;
            
            point1 = point1 < point2 ? point1 : point2;
            
            colorKey[0].color = Color.black;
            colorKey[0].time = ofset;
            
            colorKey[1].color = color;
            colorKey[1].time = point1;
            
            colorKey[2].color = color;
            colorKey[2].time = point2;

            colorKey[3].color = Color.black;
            colorKey[3].time = 1.0f;
            
            GradientAlphaKey[] alphaKey = new GradientAlphaKey[4];
            
            alphaKey[0].alpha = 0.0f;
            alphaKey[0].time = ofset;
            
            alphaKey[1].alpha = 1.0f;
            alphaKey[1].time = point1;
            
            alphaKey[2].alpha = 1.0f;
            alphaKey[2].time = point2;

            alphaKey[3].alpha = 0.0f;
            alphaKey[3].time = 1.0f;

            gradient.SetKeys(colorKey, alphaKey);
            return gradient;
        }


        public static void SetVertices(this LineRenderer lineRenderer, Vector3[] vertices)
        {
            lineRenderer.positionCount = vertices.Length;
            lineRenderer.SetPositions(vertices);
        }

        #endregion
    }
}
