﻿using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace BeatSaberPlus.SDK.Game
{
    /// <summary>
    /// BeatMaps client
    /// </summary>
    public class BeatMapsClient
    {
        /// <summary>
        /// Cache folder
        /// </summary>
        private static string m_CacheFolder;
        /// <summary>
        /// Is caching enabled
        /// </summary>
        private static bool m_CacheEnabled = true;
        /// <summary>
        /// BeatMaps client
        /// </summary>
        private static CP_SDK.Network.APIClient m_APIClient;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// BeatMaps client
        /// </summary>
        public static CP_SDK.Network.APIClient APIClient => m_APIClient;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init image provider
        /// </summary>
        internal static void Init()
        {
            m_CacheFolder   = $"UserData/{CP_SDK.ChatPlexSDK.ProductName}/Cache/BeatMaps/";
            m_APIClient     = new CP_SDK.Network.APIClient("https://api.beatsaver.com/", TimeSpan.FromSeconds(30), true, false);

            try
            {
                if (!Directory.Exists(m_CacheFolder))
                    Directory.CreateDirectory(m_CacheFolder);

                m_CacheEnabled = true;
            }
            catch (Exception l_Exception)
            {
                m_CacheEnabled = false;

                CP_SDK.ChatPlexSDK.Logger.Error($"[CP_SDK.Chat][ImageProvider.Init] Error creating cache folder, disabling caching:");
                CP_SDK.ChatPlexSDK.Logger.Error(l_Exception);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public static void GetOnlineByKey(string p_Key, Action<bool, BeatMaps.MapDetail> p_Callback)
        {
            var l_Task = m_APIClient.GetAsync("maps/id/" + p_Key, CancellationToken.None);
            l_Task.ContinueWith((p_APIResult) =>
            {
                try
                {
                    if (   p_APIResult == null
                        || p_APIResult.IsCanceled
                        || p_APIResult.Status != TaskStatus.RanToCompletion
                        || p_APIResult.Result == null)
                    {
                        p_Callback?.Invoke(false, null);
                        return;
                    }

                    var l_Response  = p_APIResult.Result;
                    var l_BeatMap   = null as BeatMaps.MapDetail;

                    if (   !l_Response.IsSuccessStatusCode
                        || !GetObjectFromJsonString(l_Response.BodyString, out l_BeatMap))
                    {
                        p_Callback?.Invoke(false, null);
                        return;
                    }

                    l_BeatMap.Partial = false;
                    p_Callback?.Invoke(true, l_BeatMap);
                }
                catch (Exception l_Exception)
                {
                    CP_SDK.ChatPlexSDK.Logger.Error("[SDK.Game][BeatMapsClient.GetOnlineByKey] Error :");
                    CP_SDK.ChatPlexSDK.Logger.Error(l_Exception);
                    p_Callback?.Invoke(false, null);
                }
            }).ConfigureAwait(false);
        }
        public static void PopulateOnlineByKey(BeatMaps.MapDetail p_BeatMap, Action<bool> p_Callback)
        {
            var l_Task = m_APIClient.GetAsync("maps/id/" + p_BeatMap.id, CancellationToken.None);
            l_Task.ContinueWith((p_APIResult) =>
            {
                try
                {
                    if (   p_APIResult == null
                        || p_APIResult.IsCanceled
                        || p_APIResult.Status != TaskStatus.RanToCompletion
                        || p_APIResult.Result == null)
                    {
                        p_Callback?.Invoke(false);
                        return;
                    }

                    var l_Response = p_APIResult.Result;

                    if (!l_Response.IsSuccessStatusCode)
                    {
                        p_Callback?.Invoke(false);
                        return;
                    }

                    JsonConvert.PopulateObject(l_Response.BodyString, p_BeatMap);
                    p_BeatMap.Partial = false;
                    p_Callback?.Invoke(true);
                }
                catch (Exception l_Exception)
                {
                    CP_SDK.ChatPlexSDK.Logger.Error("[SDK.Game][BeatMapsClient.PopulateOnlineByKey] Error :");
                    CP_SDK.ChatPlexSDK.Logger.Error(l_Exception);
                    p_Callback?.Invoke(false);
                }
            }).ConfigureAwait(false);
        }
        public static void GetOnlineByHash(string p_Hash, Action<bool, BeatMaps.MapDetail> p_Callback)
        {
            var l_Task = m_APIClient.GetAsync("maps/hash/" + p_Hash, CancellationToken.None);
            l_Task.ContinueWith((p_APIResult) =>
            {
                try
                {
                    if (   p_APIResult == null
                        || p_APIResult.IsCanceled
                        || p_APIResult.Status != TaskStatus.RanToCompletion
                        || p_APIResult.Result == null)
                    {
                        p_Callback?.Invoke(false, null);
                        return;
                    }

                    var l_Response  = p_APIResult.Result;
                    var l_BeatMap   = null as BeatMaps.MapDetail;

                    if (   !l_Response.IsSuccessStatusCode
                        || !GetObjectFromJsonString(l_Response.BodyString, out l_BeatMap))
                    {
                        p_Callback?.Invoke(false, null);
                        return;
                    }

                    l_BeatMap.Partial = false;
                    p_Callback?.Invoke(true, l_BeatMap);
                }
                catch (Exception l_Exception)
                {
                    CP_SDK.ChatPlexSDK.Logger.Error("[SDK.Game][BeatMapsClient.GetOnlineByHash] Error :");
                    CP_SDK.ChatPlexSDK.Logger.Error(l_Exception);
                    p_Callback?.Invoke(false, null);
                }
            }).ConfigureAwait(false);
        }
        public static void GetOnlineBySearch(string p_Query, Action<bool, BeatMaps.MapDetail[]> p_Callback)
        {
            var l_Task = m_APIClient.GetAsync("search/text/0?sortOrder=Relevance&q=" + HttpUtility.UrlEncode(p_Query), CancellationToken.None);
            l_Task.ContinueWith((p_APIResult) =>
            {
                try
                {
                    if (   p_APIResult == null
                        || p_APIResult.IsCanceled
                        || p_APIResult.Status != TaskStatus.RanToCompletion
                        || p_APIResult.Result == null)
                    {
                        p_Callback?.Invoke(false, null);
                        return;
                    }

                    var l_Response      = p_APIResult.Result;
                    var l_SearchResult  = null as BeatMaps.SearchResponse;

                    if (   !l_Response.IsSuccessStatusCode
                        || !GetObjectFromJsonString(l_Response.BodyString, out l_SearchResult))
                    {
                        p_Callback?.Invoke(false, null);
                        return;
                    }

                    for (int l_I = 0; l_I < l_SearchResult.docs.Length; ++l_I)
                        l_SearchResult.docs[l_I].Partial = false;

                    p_Callback?.Invoke(true, l_SearchResult.docs);
                }
                catch (Exception l_Exception)
                {
                    CP_SDK.ChatPlexSDK.Logger.Error("[SDK.Game][BeatMapsClient.GetOnlineBySearch] Error :");
                    CP_SDK.ChatPlexSDK.Logger.Error(l_Exception);
                    p_Callback?.Invoke(false, null);
                }
            }).ConfigureAwait(false);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get from cache
        /// </summary>
        /// <param name="p_Key">Key</param>
        /// <returns></returns>
        public static BeatMaps.MapDetail GetFromCacheByKey(string p_Key)
        {
            try
            {
                var l_Path = m_CacheFolder + p_Key + ".json";

                if (!File.Exists(l_Path))
                    return null;

                var l_Content = File.ReadAllText(l_Path, Encoding.UTF8);
                var l_Result = JsonConvert.DeserializeObject<BeatMaps.MapDetail>(l_Content);
                l_Result.Partial = false;

                return l_Result;
            }
            catch (Exception l_Exception)
            {
                CP_SDK.ChatPlexSDK.Logger.Error("[SDK.Game][BeatMapsClient.GetFromCacheByKey] Error :");
                CP_SDK.ChatPlexSDK.Logger.Error(l_Exception);
            }

            return null;
        }
        /// <summary>
        /// Get cover image from cache
        /// </summary>
        /// <param name="p_Key">Key</param>
        /// <returns></returns>
        public static byte[] GetCoverImageFromCacheByKey(string p_Key)
        {
            try
            {
                var l_Path = m_CacheFolder + p_Key + ".jpg";

                if (!File.Exists(l_Path))
                    return null;

                var l_Result = File.ReadAllBytes(l_Path);

                return l_Result;
            }
            catch (Exception l_Exception)
            {
                CP_SDK.ChatPlexSDK.Logger.Error("[SDK.Game][BeatMapsClient.GetCoverImageFromCacheByKey] Error :");
                CP_SDK.ChatPlexSDK.Logger.Error(l_Exception);
            }

            return null;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Cache instance
        /// </summary>
        /// <param name="p_MapDetails">Instance to cache</param>
        public static void Cache(BeatMaps.MapDetail p_MapDetails)
        {
            if (p_MapDetails == null || !p_MapDetails.IsValid())
                return;

            WriteCacheTextFile(p_MapDetails.id + ".json", p_MapDetails);
        }
        /// <summary>
        /// Cache instance cover image
        /// </summary>
        /// <param name="p_MapDetails">Instance to cache</param>
        /// <param name="p_Cover">Cover bytes</param>
        public static void CacheCoverImage(BeatMaps.MapDetail p_MapDetails, byte[] p_Cover)
        {
            if (p_MapDetails == null || !p_MapDetails.IsValid() || p_Cover.Length == 0)
                return;

            WriteCacheFile(p_MapDetails.id + ".jpg", p_Cover);
        }
        /// <summary>
        /// Clear cache
        /// </summary>
        /// <param name="p_Key">Key</param>
        public static void ClearCache(string p_Key)
            => DeleteCacheFile(p_Key);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Write cache text file
        /// </summary>
        /// <param name="p_FileName">Cache ID</param>
        /// <param name="p_Content">Content to write</param>
        /// <returns></returns>
        private static void WriteCacheTextFile(string p_FileName, BeatMaps.MapDetail p_Content)
        {
            if (!m_CacheEnabled)
                return;

            CP_SDK.Unity.MTThreadInvoker.EnqueueOnThread(() =>
            {
                try
                {
                    var l_JSON = JsonConvert.SerializeObject(p_Content);

                    File.WriteAllText(m_CacheFolder + p_FileName, l_JSON, Encoding.UTF8);
                }
                catch (Exception l_Exception)
                {
                    CP_SDK.ChatPlexSDK.Logger.Error("[SDK.Game][BeatMapsClient.WriteCacheTextFile] Error :");
                    CP_SDK.ChatPlexSDK.Logger.Error(l_Exception);
                }
            });
        }
        /// <summary>
        /// Write cache file
        /// </summary>
        /// <param name="p_FileName">Cache ID</param>
        /// <param name="p_Content">Content to write</param>
        /// <returns></returns>
        private static void WriteCacheFile(string p_FileName, byte[] p_Content)
        {
            if (!m_CacheEnabled)
                return;

            CP_SDK.Unity.MTThreadInvoker.EnqueueOnThread(() =>
            {
                try
                {
                    File.WriteAllBytes(m_CacheFolder + p_FileName, p_Content);
                }
                catch (Exception l_Exception)
                {
                    CP_SDK.ChatPlexSDK.Logger.Error("[SDK.Game][BeatMapsClient.WriteCacheFile] Error :");
                    CP_SDK.ChatPlexSDK.Logger.Error(l_Exception);
                }
            });
        }
        /// <summary>
        /// Delete cache file coroutine
        /// </summary>
        /// <param name="p_Key">File to delete</param>
        /// <returns></returns>
        private static void DeleteCacheFile(string p_Key)
        {
            if (!m_CacheEnabled)
                return;

            CP_SDK.Unity.MTThreadInvoker.EnqueueOnThread(() =>
            {
                try
                {
                    if (File.Exists(m_CacheFolder + p_Key + ".jpg"))
                        File.Delete(m_CacheFolder + p_Key + ".jpg");

                    if (File.Exists(m_CacheFolder + p_Key + ".json"))
                        File.Delete(m_CacheFolder + p_Key + ".json");
                }
                catch (Exception l_Exception)
                {
                    CP_SDK.ChatPlexSDK.Logger.Error("[SDK.Game][BeatMapsClient.DeleteCacheFile] Error :");
                    CP_SDK.ChatPlexSDK.Logger.Error(l_Exception);
                }
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Download a song
        /// </summary>
        /// <param name="p_Song">Beat map</param>
        /// <param name="p_Token">Cancellation token</param>
        /// <param name="p_Progress">Progress reporter</param>
        /// <returns></returns>
        public static async Task<(bool, string)> DownloadSong(BeatMaps.MapDetail p_Song, BeatMaps.MapVersion p_Version, CancellationToken p_Token, IProgress<double> p_Progress = null)
        {
            /*
               Code from https://github.com/Kylemc1413/BeatSaverDownloader

               MIT License

               Copyright (c) 2019 andruzzzhka

               Permission is hereby granted, free of charge, to any person obtaining a copy
               of this software and associated documentation files (the "Software"), to deal
               in the Software without restriction, including without limitation the rights
               to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
               copies of the Software, and to permit persons to whom the Software is
               furnished to do so, subject to the following conditions:

               The above copyright notice and this permission notice shall be included in all
               copies or substantial portions of the Software.

               THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
               IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
               FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
               AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
               LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
               OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
               SOFTWARE.
            */

            p_Token.ThrowIfCancellationRequested();

            try
            {
                string l_CustomSongsPath = CustomLevelPathHelper.customLevelsDirectoryPath;

                if (!Directory.Exists(l_CustomSongsPath))
                    Directory.CreateDirectory(l_CustomSongsPath);

                var l_ZIPBytes = await p_Version.ZipBytes(p_Token, p_Progress).ConfigureAwait(false);
                if (l_ZIPBytes == null || l_ZIPBytes.Length == 0)
                    return (false, "");

                CP_SDK.ChatPlexSDK.Logger.Info("[SDK.Game][BeatMapsClient] Downloaded zip!");

                return ExtractZipAsync(p_Token, p_Song, p_Version, l_ZIPBytes, l_CustomSongsPath);
            }
            catch (Exception p_Exception)
            {
                if (p_Exception is TaskCanceledException)
                {
                    CP_SDK.ChatPlexSDK.Logger.Warning("[SDK.Game][BeatMapsClient] Song Download Aborted.");
                    throw p_Exception;
                }
                else
                    CP_SDK.ChatPlexSDK.Logger.Error("[SDK.Game][BeatMapsClient] Failed to download Song!");
            }

            return (false, "");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Extract ZIP archive
        /// </summary>
        /// <param name="p_Song">Beat map</param>
        /// <param name="p_ZIPBytes">Raw ZIP bytes</param>
        /// <param name="p_CustomSongsPath">Extract path</param>
        /// <param name="p_Overwrite">Should overwrite ?</param>
        /// <returns></returns>
        private static (bool, string) ExtractZipAsync(CancellationToken p_Token, BeatMaps.MapDetail p_Song, BeatMaps.MapVersion p_Version, byte[] p_ZIPBytes, string p_CustomSongsPath, bool p_Overwrite = false)
        {
            /*
               Code from https://github.com/Kylemc1413/BeatSaverDownloader

               MIT License

               Copyright (c) 2019 andruzzzhka

               Permission is hereby granted, free of charge, to any person obtaining a copy
               of this software and associated documentation files (the "Software"), to deal
               in the Software without restriction, including without limitation the rights
               to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
               copies of the Software, and to permit persons to whom the Software is
               furnished to do so, subject to the following conditions:

               The above copyright notice and this permission notice shall be included in all
               copies or substantial portions of the Software.

               THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
               IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
               FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
               AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
               LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
               OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
               SOFTWARE.
            */

            p_Token.ThrowIfCancellationRequested();

            Stream l_ZIPStream = new MemoryStream(p_ZIPBytes);

            try
            {
                CP_SDK.ChatPlexSDK.Logger.Info("[SDK.Game][BeatMapsClient] Extracting...");

                /// Create ZIP archive
                ZipArchive l_ZIPArchive = new ZipArchive(l_ZIPStream, ZipArchiveMode.Read);

                /// Prepare base path
                string l_BasePath = p_Song.id + " (" + p_Song.metadata.songName + " - " + p_Song.metadata.levelAuthorName + ")";
                l_BasePath = string.Join("", l_BasePath.Split((Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()).ToArray())));

                /// Build out path
                string l_OutPath = p_CustomSongsPath + "/" + l_BasePath;

                /// Check to avoid overwrite
                if (!p_Overwrite && Directory.Exists(l_OutPath))
                {
                    int l_FolderCount = 1;

                    while (Directory.Exists(l_OutPath + $" ({l_FolderCount})"))
                        ++l_FolderCount;

                    l_OutPath += $" ({l_FolderCount})";
                    l_BasePath += $" ({l_FolderCount})";
                }

                /// Create directory if needed
                if (!Directory.Exists(l_OutPath))
                    Directory.CreateDirectory(l_OutPath);

                CP_SDK.ChatPlexSDK.Logger.Info("[SDK.Game][BeatMapsClient] " + l_OutPath);

                foreach (var l_Entry in l_ZIPArchive.Entries)
                {
                    /// Most likely a folder
                    if (string.IsNullOrEmpty(l_Entry.Name.Trim()) || l_Entry.Name != l_Entry.FullName)
                        continue;

                    /// Name instead of FullName for better security and because song zips don't have nested directories anyway
                    var l_EntryPath = Path.Combine(l_OutPath, l_Entry.Name);

                    /// Either we're overwriting or there's no existing file
                    if (p_Overwrite || !File.Exists(l_EntryPath))
                        l_Entry.ExtractToFile(l_EntryPath, p_Overwrite);
                }

                l_ZIPArchive.Dispose();
                l_ZIPStream.Close();

                return (true, l_BasePath);
            }
            catch (Exception p_Exception)
            {
                l_ZIPStream.Close();

                if (p_Exception is TaskCanceledException)
                    throw p_Exception;

                CP_SDK.ChatPlexSDK.Logger.Error("[SDK.Game][BeatMapsClient] Unable to extract ZIP! Exception");
                CP_SDK.ChatPlexSDK.Logger.Error(p_Exception);
            }

            return (false, "");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get JObject from serialized JSON
        /// </summary>
        /// <param name="p_Serialized">Input</param>
        /// <param name="p_JObject">Result object</param>
        /// <returns></returns>
        private static bool GetObjectFromJsonString<T>(string p_Serialized, out T p_JObject)
            where T : class, new()
        {
            p_JObject = null;
            try
            {
                p_JObject = JsonConvert.DeserializeObject<T>(p_Serialized);
            }
            catch (Exception l_Exception)
            {
                CP_SDK.ChatPlexSDK.Logger.Error("[SDK.Game][BeatMapsClient.GetObjectFromJsonString] Error :");
                CP_SDK.ChatPlexSDK.Logger.Error(l_Exception);
                return false;
            }

            return p_JObject != null;
        }
    }
}
