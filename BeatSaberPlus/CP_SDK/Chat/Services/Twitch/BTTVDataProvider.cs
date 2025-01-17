﻿using CP_SDK.Chat.Interfaces;
using CP_SDK.Chat.Models;
using CP_SDK.Chat.SimpleJSON;
using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading.Tasks;

namespace CP_SDK.Chat.Services.Twitch
{
    /// <summary>
    /// BTTV data provider
    /// </summary>
    public class BTTVDataProvider : IChatResourceProvider<ChatResourceData>
    {
        /// <summary>
        /// HTTP Client
        /// </summary>
        private HttpClient m_HTTPClient = new HttpClient();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Resource cache
        /// </summary>
        public ConcurrentDictionary<string, ChatResourceData> Resources { get; } = new ConcurrentDictionary<string, ChatResourceData>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Try request resources
        /// </summary>
        /// <param name="p_Category">Category / Channel</param>
        /// <returns></returns>
        public async Task TryRequestResources(string p_Category, string p_Token)
        {
            bool l_IsGlobal = string.IsNullOrEmpty(p_Category);

            try
            {
                ChatPlexSDK.Logger.Debug($"Requesting BTTV {(l_IsGlobal ? "global " : "")}emotes{(l_IsGlobal ? "." : $" for channel {p_Category}")}.");

                using (var l_Message = new HttpRequestMessage(HttpMethod.Get, l_IsGlobal ? "https://api.betterttv.net/3/cached/emotes/global" : $"https://api.betterttv.net/3/cached/users/twitch/{p_Category}"))
                {
                    var l_Response = await m_HTTPClient.SendAsync(l_Message).ConfigureAwait(false);
                    if (!l_Response.IsSuccessStatusCode)
                    {
                        ChatPlexSDK.Logger.Error($"Unsuccessful status code when requesting BTTV {(l_IsGlobal ? "global " : "")}emotes{(l_IsGlobal ? "." : " for channel " + p_Category)}. {l_Response.ReasonPhrase}");
                        return;
                    }

                    var l_JSON = JSON.Parse(await l_Response.Content.ReadAsStringAsync().ConfigureAwait(false));
                    if ((l_IsGlobal && !l_JSON.IsArray) || (!l_IsGlobal && !l_JSON["channelEmotes"].IsArray))
                    {
                        ChatPlexSDK.Logger.Error("emotes was not an array.");
                        return;
                    }

                    var l_JSONEmotes = l_IsGlobal ? l_JSON.AsArray : l_JSON["channelEmotes"].AsArray;

                    int l_Count = 0;
                    foreach (JSONObject l_Object in l_JSONEmotes)
                    {
                        string l_URI        = $"https://cdn.betterttv.net/emote/{l_Object["id"].Value}/2x";
                        string l_Identifier = l_Object["code"].Value;

                        Resources.TryAdd(l_Identifier, new ChatResourceData()
                        {
                            Uri         = l_URI,
                            Animation   = l_Object["imageType"].Value == "gif" ? Animation.EAnimationType.GIF : Animation.EAnimationType.NONE,
                            Category    = EChatResourceCategory.Emote,
                            Type        = l_IsGlobal ? "BTTVGlobalEmote" : "BTTVChannelEmote"
                        });
                        l_Count++;
                    }

                    if (!l_IsGlobal)
                    {
                        l_JSONEmotes = l_JSON["sharedEmotes"].AsArray;

                        foreach (JSONObject l_Object in l_JSONEmotes)
                        {
                            string l_URI        = $"https://cdn.betterttv.net/emote/{l_Object["id"].Value}/2x";
                            string l_Identifier = l_Object["code"].Value;

                            Resources.TryAdd(l_Identifier, new ChatResourceData()
                            {
                                Uri         = l_URI,
                                Animation   = l_Object["imageType"].Value == "gif" ? Animation.EAnimationType.GIF : Animation.EAnimationType.NONE,
                                Type        = "BTTVChannelEmote"
                            });
                            l_Count++;
                        }
                    }

                    ChatPlexSDK.Logger.Debug($"Success caching {l_Count} BTTV {(l_IsGlobal ? "global " : "")}emotes{(l_IsGlobal ? "." : " for channel " + p_Category)}.");
                    return;
                }
            }
            catch (Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"An error occurred while requesting BTTV {(l_IsGlobal ? "global " : "")}emotes{(l_IsGlobal ? "." : " for channel " + p_Category)}.");
                ChatPlexSDK.Logger.Error(l_Exception);
            }

            return;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Try get a resource
        /// </summary>
        /// <param name="p_Identifier">Resource ID</param>
        /// <param name="p_Category">Channel / Category</param>
        /// <param name="p_Data">Result data</param>
        /// <returns></returns>
        public bool TryGetResource(string p_Identifier, string p_Category, out ChatResourceData p_Data)
        {
            if (!string.IsNullOrEmpty(p_Category) && Resources.TryGetValue($"{p_Category}_{p_Identifier}", out p_Data))
                return true;

            if (Resources.TryGetValue(p_Identifier, out p_Data))
                return true;

            p_Data = null;
            return false;
        }
    }
}
