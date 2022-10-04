﻿using CP_SDK.Logging;
using CP_SDK_WebSocketSharp;
using CP_SDK_WebSocketSharp.Net;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace BeatSaberPlus.SDK.Game
{
    /// <summary>
    /// UserPlatform helper
    /// </summary>
    public class UserPlatform
    {
        /// <summary>
        /// User ID cache
        /// </summary>
        private static string m_UserID = null;
        /// <summary>
        /// User name cache
        /// </summary>
        private static string m_UserName = null;
        /// <summary>
        /// The actual user name cache
        /// </summary>
        private static string m_ActualUserName = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get User ID
        /// </summary>
        /// <returns></returns>
        public static string GetUserID()
        {
            if (m_UserID != null)
                return m_UserID;

            FetchPlatformInfos();

            return m_UserID;
        }
        /// <summary>
        /// Get User ID
        /// </summary>
        /// <returns></returns>
        public static string GetUserName()
        {

            if (m_UserName != null)
                return m_UserName;

            CP_SDK.ChatPlexSDK.Logger.Info("The current working directory is " + Directory.GetCurrentDirectory());
            if (File.Exists("./displayname.txt"))
            {
                var lines = File.ReadAllLines("./displayname.txt");
                if (lines.Length > 0)
                {
                    m_UserName = lines[0];
                }
            }

            // File could not be read
            if (m_UserName == null)
            {
                FetchPlatformInfos();
                m_UserName = m_ActualUserName;
            }

            return m_UserName;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Find platform informations
        /// </summary>
        private static void FetchPlatformInfos()
        {
            try
            {
                var l_PlatformLeaderboardsModels = Resources.FindObjectsOfTypeAll<PlatformLeaderboardsModel>();
                var l_FieldAccessor = typeof(PlatformLeaderboardsModel).GetField("_platformUserModel", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                foreach (var l_Current in l_PlatformLeaderboardsModels)
                {
                    var l_PlatformUserModel = l_FieldAccessor.GetValue(l_Current) as IPlatformUserModel;
                    if (l_PlatformUserModel == null)
                        continue;

                    var l_Task = l_PlatformUserModel.GetUserInfo();
                    l_Task.Wait();

                    var l_PlayerID = l_Task.Result.platformUserId;
                    if (!string.IsNullOrEmpty(l_PlayerID))
                    {
                        m_UserID    = l_PlayerID;
                        m_ActualUserName  = l_Task.Result.userName;
                        return;
                    }
                }
            }
            catch (System.Exception l_Exception)
            {
                CP_SDK.ChatPlexSDK.Logger.Error("[SDK.Game][UserPlatform] Unable to find user platform informations");
                CP_SDK.ChatPlexSDK.Logger.Error(l_Exception);
            }
        }
    }
}
