using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Drawmasters.Ui.Extensions
{
    public class UIPrimitiveBase : MaskableGraphic
    {
        #region Filds

        static protected Material s_ETC1DefaultUI = default;

        List<Vector2> outputList = new List<Vector2>();

        [SerializeField] private Sprite m_Sprite;

        [NonSerialized] private Sprite m_OverrideSprite;

        [SerializeField] protected float m_Resolution;

        [SerializeField] private bool m_useNativeSize;

        #endregion

        

        #region Propertis

        public Sprite sprite
        {
            get => m_Sprite;
            set
            {
                m_Sprite = value;
                GeneratedUVs();
                SetAllDirty();
            }
        }

        public Sprite overrideSprite
        {
            get => activeSprite;
            set
            {
                m_OverrideSprite = value;
                GeneratedUVs();
                SetAllDirty();
            }
        }

        protected Sprite activeSprite => m_OverrideSprite != null ? m_OverrideSprite : sprite;

        public float Resolution
        {
            get => m_Resolution;
            set
            {
                m_Resolution = value;
                SetAllDirty();
            }
        }

        public bool UseNativeSize
        {
            get => m_useNativeSize;
            set
            {
                m_useNativeSize = value;
                SetAllDirty();
            }
        }

        static public Material defaultETC1GraphicMaterial
        {
            get
            {
                if (s_ETC1DefaultUI == null)
                {
                    s_ETC1DefaultUI = Canvas.GetETC1SupportedCanvasMaterial();
                }

                return s_ETC1DefaultUI;
            }
        }

        public override Texture mainTexture
        {
            get
            {
                if (activeSprite == null)
                {
                    if (material != null && material.mainTexture != null)
                    {
                        return material.mainTexture;
                    }

                    return s_WhiteTexture;
                }

                return activeSprite.texture;
            }
        }

        public float pixelsPerUnit
        {
            get
            {
                float spritePixelsPerUnit = 100;
                if (activeSprite)
                {
                    spritePixelsPerUnit = activeSprite.pixelsPerUnit;
                }

                float referencePixelsPerUnit = 100;
                if (canvas)
                {
                    referencePixelsPerUnit = canvas.referencePixelsPerUnit;
                }

                return spritePixelsPerUnit / referencePixelsPerUnit;
            }
        }

        public override Material material
        {
            get
            {
                if (m_Material != null)
                {
                    return m_Material;
                }

                if (activeSprite && activeSprite.associatedAlphaSplitTexture != null)
                {
                    return defaultETC1GraphicMaterial;
                }

                return defaultMaterial;
            }
            set => base.material = value;
        }

        #endregion


        
        #region Life cicle

        protected UIPrimitiveBase()
        {
            useLegacyMeshGeneration = false;
        }

        #endregion

        
        
        #region Methods
        
        protected UIVertex[] SetVbo(Vector2[] vertices, Vector2[] uvs)
        {
            UIVertex[] vbo = new UIVertex[4];
            for (int i = 0; i < vertices.Length; i++)
            {
                var vert = UIVertex.simpleVert;
                vert.color = color;
                vert.position = vertices[i];
                vert.uv0 = uvs[i];
                vbo[i] = vert;
            }
            return vbo;
        }

        protected Vector2[] IncreaseResolution(Vector2[] input)
        {
            return IncreaseResolution(new List<Vector2>(input)).ToArray();
        }

        protected List<Vector2> IncreaseResolution(List<Vector2> input)
        {
            outputList.Clear();

            float totalDistance = 0, increments = 0;
            for (int i = 0; i < input.Count - 1; i++)
            {
                totalDistance += Vector2.Distance(input[i], input[i + 1]);
            }

            ResolutionToNativeSize(totalDistance);
            increments = totalDistance / m_Resolution;
            var incrementCount = 0;
            for (int i = 0; i < input.Count - 1; i++)
            {
                var p1 = input[i];
                outputList.Add(p1);
                var p2 = input[i + 1];
                var segmentDistance = Vector2.Distance(p1, p2) / increments;
                var incrementTime = 1f / segmentDistance;
                for (int j = 0; j < segmentDistance; j++)
                {
                    outputList.Add(Vector2.Lerp(p1, (Vector2) p2, j * incrementTime));
                    incrementCount++;
                }
                outputList.Add(p2);
            }
            return outputList;
        }

        protected virtual void GeneratedUVs()
        {
        }

        protected virtual void ResolutionToNativeSize(float distance)
        {
        }
        
        #endregion

        
        
        #region onEnable

        protected override void OnEnable()
        {
            base.OnEnable();
            SetAllDirty();
        }

        #endregion
    }
}