using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Drawmasters.Levels.Order;
using Drawmasters.ServiceUtil;

namespace Drawmasters.LevelsRepository
{
    public static class LevelsContainer
    {
        #region Fields

        public const string ResourcePath = "LevelsRepository";
        public const string HeadersFolder = "Headers";
        public const string BodiesFolder = "Bodies";

        private static readonly Dictionary<GameMode, LevelHeader[]> modeToHeaders = new Dictionary<GameMode, LevelHeader[]>();
        private static readonly Dictionary<GameMode, LevelHeader[]> modeToEnabledHeaders = new Dictionary<GameMode, LevelHeader[]>();

        #endregion



        #region Ctor

        static LevelsContainer() => LoadData();

        #endregion
        
        
        
        #region Public methods

        public static LevelHeader GetSceneHeader(GameMode mode)
        {
            LevelHeader result = default;
            LevelHeader[] headersForMode = GetAllHeaders(modeToHeaders, mode);

            if (headersForMode != null)
            {
                result = headersForMode[0];
            }

            return result;
        }


        public static LevelHeader GetHeader(GameMode mode, int index)
        {
            LevelHeader result = default;
            LevelHeader[] headersForMode = GetAllHeaders(modeToHeaders, mode);

            if (headersForMode != null)
            {
                int clampedIndex = index % headersForMode.Length;

                result = headersForMode[clampedIndex];
            }

            return result;
        }


        public static LevelHeader GetHeader(GameMode mode, string id)
        {
            LevelHeader result = default;
            LevelHeader[] headersForMode = GetAllHeaders(modeToHeaders, mode);

            if (headersForMode != null)
            {
                result = headersForMode.Find(h => h.title.Equals(id));
            }

            return result;
        }


        public static LevelHeader GetSublevelHeader(ILevelOrderService levelOrderService, GameMode mode, int index)
        {
            LevelHeader result = default;
            LevelHeader[] headersForMode = GetAllHeaders(modeToHeaders, mode);

            SublevelData? data = levelOrderService.FindData(mode, index);

            string id = data.Value.NameId;

            if (headersForMode != null)
            {
                result = headersForMode.Find(h => h.title.Equals(id));

                if (result == null)
                {
                    CustomDebug.Log($"Cannot find mode with id:{id}, mode:{mode}, index:{index}");
                    
                    result = headersForMode.Find(h => h.levelType == LevelType.Simple);
                }
            }

            return result;
        }


        public static int GetLevelIndex(LevelHeader header)
        {
            if (header == null)
            {
                CustomDebug.Log("Header is NULL");

                return -1;
            }

            var headersForMode = GetAllHeaders(modeToHeaders, header.mode);
            return Array.IndexOf(headersForMode, header);
        }


        public static int GetEnabledLevelsCount(GameMode mode)
        {
            var headersForMode = GetAllHeaders(modeToEnabledHeaders, mode);

            return headersForMode == null ? default : headersForMode.Length;
        }


        public static void LoadData()
        {
            modeToHeaders.Clear();
            modeToEnabledHeaders.Clear();
            LevelHeader[] headers = Resources.LoadAll<LevelHeader>(Path.Combine(ResourcePath, HeadersFolder));

            foreach (var m in Enum.GetValues(typeof(GameMode)))
            {
                GameMode mode = (GameMode)m;
                modeToHeaders.Add(mode, headers.Where(h => h.mode == mode).OrderBy(h => h.title).ToArray());
                modeToEnabledHeaders.Add(mode, modeToHeaders[mode].Where(h => !h.isDisabled).ToArray());
            }
        }


        public static Levels.Level.Data GetLevelData(LevelHeader header)
        {
            if (header == null)
            {
                CustomDebug.Log("Trying load body for NULL Header");
            }

            LevelBody body = Resources.Load<LevelBody>(Path.Combine(ResourcePath, BodiesFolder, header.name));

            if (body == null || body.data == null)
            {
                CustomDebug.Log($"Missing body for header. Header = {header}");
            }

            return body.data;
        }


        public static LevelType GetLevelType(GameMode mode, string levelId)
        {
            LevelType result = LevelType.None;
            
            LevelHeader[] headers = GetAllHeaders(modeToHeaders, mode);
            LevelHeader header = Array.Find(headers, h => h.title == levelId);

            if (header != null)
            {
                result = header.levelType;
            }

            return result;
        }
        
        #endregion



        #region Private methods
        
        private static LevelHeader[] GetAllHeaders(Dictionary<GameMode, LevelHeader[]> dictionary, GameMode mode)
        {
            LevelHeader[] result = default;

            if (dictionary.TryGetValue(mode, out LevelHeader[] headers))
            {
                result = headers;
            }
            else
            {
                CustomDebug.Log("No level headers was found for game mode " + mode);
            }

            return result;
        }
        
        #endregion
    }
}
