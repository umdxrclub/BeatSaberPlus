﻿using BeatSaberPlus.SDK.Chat.Models;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace BeatSaberPlus.SDK.Chat
{
    /// <summary>
    /// Image provider for twitch
    /// </summary>
    public class ImageProvider
    {
        /// <summary>
        /// Animated gif byte pattern for fast lookup
        /// </summary>
        private static byte[] ANIMATED_GIF_PATTERN = new byte[] { 0x4E, 0x45, 0x54, 0x53, 0x43, 0x41, 0x50, 0x45, 0x32, 0x2E, 0x30 };
        /// <summary>
        /// Is caching enabled
        /// </summary>
        private static bool m_CacheEnabled = true;
        /// <summary>
        /// Forced emote height
        /// </summary>
        private static int m_ForcedHeight = 110;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Cache folder
        /// </summary>
        private static string m_CacheFolder = "UserData/BeatSaberPlus/Cache/Chat/";
        /// <summary>
        /// Cached image info
        /// </summary>
        private static ConcurrentDictionary<string, Unity.EnhancedImage> m_CachedImageInfo = new ConcurrentDictionary<string, Unity.EnhancedImage>();
        /// <summary>
        /// Cached emote info
        /// </summary>
        private static ConcurrentDictionary<string, Unity.EnhancedImage> m_CachedEmoteInfo = new ConcurrentDictionary<string, Unity.EnhancedImage>();
        /// <summary>
        /// Download queue
        /// </summary>
        private static ConcurrentDictionary<string, Action<byte[]>> m_ActiveDownloads = new ConcurrentDictionary<string, Action<byte[]>>();
        /// <summary>
        /// Cache for sprite sheets
        /// </summary>
        private static ConcurrentDictionary<string, Texture2D> m_CachedSpriteSheets = new ConcurrentDictionary<string, Texture2D>();
        /// <summary>
        /// Cached images info
        /// </summary>
        private static ReadOnlyDictionary<string, Unity.EnhancedImage> m_CachedImageInfoProxy = null;
        /// <summary>
        /// Cached emotes info
        /// </summary>
        private static ReadOnlyDictionary<string, Unity.EnhancedImage> m_CachedEmoteInfoProxy = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Cached images info
        /// </summary>
        public static ReadOnlyDictionary<string, Unity.EnhancedImage> CachedImageInfo { get {
            if (m_CachedImageInfoProxy == null)
                m_CachedImageInfoProxy = new ReadOnlyDictionary<string, Unity.EnhancedImage>(m_CachedImageInfo);

            return m_CachedImageInfoProxy;
        } }
        /// <summary>
        /// Cached emotes info
        /// </summary>
        public static ReadOnlyDictionary<string, Unity.EnhancedImage> CachedEmoteInfo { get {
            if (m_CachedEmoteInfoProxy == null)
                m_CachedEmoteInfoProxy = new ReadOnlyDictionary<string, Unity.EnhancedImage>(m_CachedEmoteInfo);

            return m_CachedEmoteInfoProxy;
        } }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Clear cache
        /// </summary>
        internal static void ClearCache()
        {
            if (m_CachedImageInfo.Count > 0)
            {
                foreach (var l_Current in m_CachedImageInfo.Values)
                    MonoBehaviour.Destroy(l_Current.Sprite);

                m_CachedImageInfo.Clear();
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Pre-cache animated image
        /// </summary>
        /// <param name="p_Category">Category</param>
        /// <param name="p_URI">URI of the image</param>
        /// <param name="p_ID">ID of the image</param>
        /// <param name="p_Finally">A callback that occurs after the resource is retrieved. This will always occur even if the resource is already cached.</param>
        /// <returns></returns>
        public static void PrecacheAnimatedImage(Interfaces.EChatResourceCategory p_Category, string p_URI, string p_ID, Animation.AnimationType p_Animation, Action<Unity.EnhancedImage> p_Finally = null)
        {
            TryCacheSingleImage(p_Category, p_ID, p_URI, p_Animation, p_Finally);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Try to cache single image
        /// </summary>
        /// <param name="p_Category">Category</param>
        /// <param name="p_ID">ID of the image</param>
        /// <param name="p_URI">The resource location</param>
        /// <param name="p_Animation">Is and animated image</param>
        /// <param name="p_Finally">A callback that occurs after the resource is retrieved. This will always occur even if the resource is already cached.</param>
        /// <returns></returns>
        public static void TryCacheSingleImage(Interfaces.EChatResourceCategory p_Category, string p_ID, string p_URI, Animation.AnimationType p_Animation, Action<Unity.EnhancedImage> p_Finally = null)
        {
            if (m_CachedImageInfo.TryGetValue(p_ID, out var p_Info))
            {
                p_Finally?.Invoke(p_Info);
                return;
            }

            if (string.IsNullOrEmpty(p_URI))
            {
                Logger.Instance.Error($"[SDK.Chat][ImageProvider.DownloadContent] URI is null or empty in request for resource {p_URI}. Aborting!");

                m_CachedSpriteSheets[p_URI] = null;
                p_Finally?.Invoke(null);

                return;
            }

            string l_CacheID = "";
            if (m_CacheEnabled)
                l_CacheID = "Emote_" + SDK.Cryptography.SHA1.GetHashString(p_URI) + ".dat";

            Task.Run(() => LoadFromCacheOrDownload(p_URI, l_CacheID, (p_Bytes) =>
            {
                if (p_Animation == Animation.AnimationType.MAYBE_GIF)
                {
                    if (p_Bytes.Length > 3 && p_Bytes[0] == 0x47 && ContainBytePattern(p_Bytes, ANIMATED_GIF_PATTERN))
                        p_Animation = Animation.AnimationType.GIF;
                    else
                        p_Animation = Animation.AnimationType.NONE;
                }

                if (p_Animation != Animation.AnimationType.NONE)
                {
                    SDK.Unity.EnhancedImage.FromRawAnimated(p_ID, p_Animation, p_Bytes, (p_Result) =>
                    {
                        if (p_Result != null)
                        {
                            m_CachedImageInfo[p_ID] = p_Result;
                            if (p_Category == Interfaces.EChatResourceCategory.Emote) m_CachedEmoteInfo[p_ID] = p_Result;
                        }

                        p_Finally?.Invoke(p_Result);
                    }, m_ForcedHeight);
                }
                else
                {
                    SDK.Unity.EnhancedImage.FromRawStatic(p_ID, p_Bytes, (p_Result) =>
                    {
                        if (p_Result != null)
                        {
                            m_CachedImageInfo[p_ID] = p_Result;
                            if (p_Category == Interfaces.EChatResourceCategory.Emote) m_CachedEmoteInfo[p_ID] = p_Result;
                        }

                        p_Finally?.Invoke(p_Result);
                    }, m_ForcedHeight);
                }
            }));
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Load from cache or download
        /// </summary>
        /// <param name="p_URI">The resource location</param>
        /// <param name="p_CacheID">Cache ID</param>
        /// <param name="p_Finally">A callback that occurs after the resource is retrieved. This will always occur even if the resource is already cached.</param>
        private static void LoadFromCacheOrDownload(string p_URI, string p_CacheID, Action<byte[]> p_Finally)
        {
            if (m_CacheEnabled)
            {
                if (File.Exists(m_CacheFolder + p_CacheID))
                {
                    var l_Content = File.ReadAllBytes(m_CacheFolder + p_CacheID);
                    p_Finally?.Invoke(l_Content);
                    return;
                }
            }

            SDK.Unity.MainThreadInvoker.Enqueue(() => SharedCoroutineStarter.instance.StartCoroutine(DownloadContent(p_URI, p_CacheID, p_Finally)));
        }
        /// <summary>
        /// Retrieves the requested content from the provided Uri. Don't yield to this function, as it may return instantly if the download is already queued when you request it.
        /// <para>
        /// The <paramref name="p_Finally"/> callback will *always* be called for this function. If it returns an empty byte array, that should be considered a failure.
        /// </para>
        /// </summary>
        /// <param name="p_URI">The resource location</param>
        /// <param name="p_CacheID">Cache ID</param>
        /// <param name="p_Finally">A callback that occurs after the resource is retrieved. This will always occur even if the resource is already cached.</param>
        /// <param name="p_IsRetry">Is a retry attempt</param>
        private static IEnumerator DownloadContent(string p_URI, string p_CacheID, Action<byte[]> p_Finally, bool p_IsRetry = false)
        {
            if (!p_IsRetry && m_ActiveDownloads.TryGetValue(p_URI, out var _))
            {
                Logger.Instance.Info($"[SDK.Chat][ImageProvider.DownloadContent] Request already active for {p_URI}");
                yield break;
            }

            if (!m_ActiveDownloads.ContainsKey(p_URI))
                m_ActiveDownloads.TryAdd(p_URI, p_Finally);

            m_ActiveDownloads[p_URI] -= p_Finally;
            m_ActiveDownloads[p_URI] += p_Finally;

            using (UnityWebRequest l_Request = UnityWebRequest.Get(p_URI))
            {
                yield return l_Request.SendWebRequest();

                /// Failed to download due to HTTP error, don't retry
                if (l_Request.isHttpError)
                {
                    m_ActiveDownloads[p_URI]?.Invoke(null);
                    m_ActiveDownloads.TryRemove(p_URI, out var _);
                    yield break;
                }

                if (l_Request.isNetworkError)
                {
                    if (!p_IsRetry)
                    {
                        Logger.Instance.Error($"[SDK.Chat][ImageProvider.DownloadContent] A network error occurred during request to {p_URI}. Retrying in 3 seconds... {l_Request.error}");
                        yield return new WaitForSeconds(3);

                        SharedCoroutineStarter.instance.StartCoroutine(DownloadContent(p_URI, p_CacheID, p_Finally, true));

                        yield break;
                    }

                    m_ActiveDownloads[p_URI]?.Invoke(null);
                    m_ActiveDownloads.TryRemove(p_URI, out var _);

                    yield break;
                }

                var l_Data = l_Request.downloadHandler.data;

                if (m_CacheEnabled && l_Data != null && l_Data.Length > 0)
                    SharedCoroutineStarter.instance.StartCoroutine(WriteCacheFile(p_CacheID, l_Data));

                m_ActiveDownloads[p_URI]?.Invoke(l_Data);
                m_ActiveDownloads.TryRemove(p_URI, out var _);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On sprite sheet cached
        /// </summary>
        /// <param name="p_ID">ID of the sprite sheet</param>
        /// <param name="p_Texture">Result texture</param>
        /// <param name="p_Rect">Sheet rect</param>
        /// <param name="p_Finally">A callback that occurs after the resource is retrieved. This will always occur even if the resource is already cached.</param>
        /// <param name="p_ForcedHeight">Forced height</param>
        private static void CacheSpriteSheetImage(string p_ID, Texture2D p_Texture, ImageRect p_Rect, Action<Unity.EnhancedImage> p_Finally = null, int p_ForcedHeight = -1)
        {
            var l_Result = Unity.EnhancedImage.FromSpriteSheetImage(p_ID, p_Texture, new Rect(p_Rect.x, p_Rect.y, p_Rect.width, p_Rect.height), p_ForcedHeight);

            if (l_Result != null)
                m_CachedImageInfo[p_ID] = l_Result;

            p_Finally?.Invoke(l_Result);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Write cache file
        /// </summary>
        /// <param name="p_CacheID">Cache ID</param>
        /// <param name="p_Content">Content to write</param>
        /// <returns></returns>
        private static IEnumerator WriteCacheFile(string p_CacheID, byte[] p_Content)
        {
            /// Wait until menu scene
            yield return new WaitUntil(() => Game.Logic.ActiveScene == Game.Logic.SceneType.Menu);

            Task.Run(() =>
            {
                if (!Directory.Exists(m_CacheFolder))
                    Directory.CreateDirectory(m_CacheFolder);

                File.WriteAllBytes(m_CacheFolder + p_CacheID, p_Content);
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Fast lookup for byte pattern
        /// </summary>
        /// <param name="p_Array">Input array</param>
        /// <param name="p_Pattern">Lookup pattern</param>
        /// <returns></returns>
        static bool ContainBytePattern(byte[] p_Array, byte[] p_Pattern)
        {
            var l_PatternPosition = 0;
            for (int l_I = 0; l_I < p_Array.Length; ++l_I)
            {
                if (p_Array[l_I] != p_Pattern[l_PatternPosition])
                {
                    l_PatternPosition = 0;
                    continue;
                }

                l_PatternPosition++;
                if (l_PatternPosition == p_Pattern.Length)
                    return true;
            }

            return l_PatternPosition == p_Pattern.Length;
        }
    }
}
