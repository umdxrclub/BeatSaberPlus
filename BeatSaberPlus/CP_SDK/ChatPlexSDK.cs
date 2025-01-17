﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace CP_SDK
{
    /// <summary>
    /// ChatPlex SDK main class
    /// </summary>
    public static class ChatPlexSDK
    {
        /// <summary>
        /// Render pipeline
        /// </summary>
        public enum ERenderPipeline
        {
            BuiltIn,
            URP
        }
        /// <summary>
        /// Generic scene enum
        /// </summary>
        public enum EGenericScene
        {
            None,
            Menu,
            Playing
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Module list
        /// </summary>
        private static List<IModuleBase> m_Modules = new List<IModuleBase>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Logger instance
        /// </summary>
        public static Logging.ILogger Logger { get; private set; }

        /// <summary>
        /// Product name
        /// </summary>
        public static string ProductName { get; private set; } = string.Empty;
        /// <summary>
        /// Network user agent
        /// </summary>
        public static string NetworkUserAgent { get; private set; } = string.Empty;
        /// <summary>
        /// Render pipeline
        /// </summary>
        public static ERenderPipeline RenderPipeline { get; private set; } = ERenderPipeline.BuiltIn;
        /// <summary>
        /// Active scene type
        /// </summary>
        public static EGenericScene ActiveGenericScene { get; private set; } = EGenericScene.None;

        /// <summary>
        /// On scene change
        /// </summary>
        public static event Action<EGenericScene> OnGenericSceneChange;
        /// <summary>
        /// On menu scene loaded
        /// </summary>
        public static event Action OnGenericMenuSceneLoaded;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Configure
        /// </summary>
        /// <param name="p_Logger">Logger instance</param>
        /// <param name="p_ProductName">Product name</param>
        /// <param name="p_RenderPipeline">Rendering pipeline</param>
        internal static void Configure(Logging.ILogger p_Logger, string p_ProductName, ERenderPipeline p_RenderPipeline)
        {
            Logger = p_Logger;

            ProductName         = p_ProductName;
            NetworkUserAgent    = $"ChatPlexSDK_{p_ProductName}/{Application.version}";
            RenderPipeline      = p_RenderPipeline;
        }
        /// <summary>
        /// When the assembly is loaded
        /// </summary>
        internal static void OnAssemblyLoaded()
        {
            InstallWEBPCodecs();

            /// Init config
            Chat.Service.Init();
            OBS.Service.Init();
            //VoiceAttack.Service.Acquire();
        }
        /// <summary>
        /// On assembly exit
        /// </summary>
        internal static void OnAssemblyExit()
        {
            try
            {
                Chat.Service.Release(true);
                VoiceAttack.Service.Release(true);
                OBS.Service.Release(true);
            }
            catch (Exception l_Exception)
            {
                Logger.Error("[CP_SDK][ChatPlexSDK.OnAssemblyExit] Error:");
                Logger.Error(l_Exception);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#if CP_SDK_UNITY
        /// <summary>
        /// When unity is ready
        /// </summary>
        internal static void OnUnityReady()
        {
            try
            {
                Unity.MTCoroutineStarter.Initialize();
                Unity.MTMainThreadInvoker.Initialize();
                Unity.MTThreadInvoker.Initialize();

                /// Init fonts
                Unity.FontManager.Init();
            }
            catch (Exception p_Exception)
            {
                Logger.Error("[CP_SDK][ChatPlexSDK.OnUnityReady] Error:");
                Logger.Error(p_Exception);
            }
        }
        /// <summary>
        /// When unity is exiting
        /// </summary>
        internal static void OnUnityExit()
        {
            try
            {
                Unity.MTThreadInvoker.Stop();
            }
            catch (Exception p_Exception)
            {
                Logger.Error("[CP_SDK][ChatPlexSDK.OnUnityExit] Error:");
                Logger.Error(p_Exception);
            }
        }
#endif

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init all the available modules
        /// </summary>
        internal static void InitModules()
        {
            try
            {
                Logger.Debug("[CP_SDK][ChatPlexSDK.InitModules] Init modules.");

                foreach (var l_Assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        foreach (var l_Type in l_Assembly.GetTypes())
                        {
                            if (!l_Type.IsClass || l_Type.ContainsGenericParameters)
                                continue;

                            if (!typeof(IModuleBase).IsAssignableFrom(l_Type))
                                continue;

                            var l_Module = (IModuleBase)Activator.CreateInstance(l_Type);

                            Logger.Debug("[CP_SDK][ChatPlexSDK.InitModules] - " + l_Module.Name);

                            /// Add plugin to the list
                            m_Modules.Add(l_Module);

                            try                                 { l_Module.CheckForActivation(EIModuleBaseActivationType.OnStart);                                                               }
                            catch (Exception p_InitException)   { Logger.Error("[CP_SDK][ChatPlexSDK.InitModules] Error on module init " + l_Module.Name); Logger.Error(p_InitException);   }
                        }
                    }
                    catch (Exception l_Exception)
                    {
                        Logger.Error("[CP_SDK][ChatPlexSDK.InitModules] Failed to find modules in " + l_Assembly.FullName);
                        Logger.Error(l_Exception);
                    }
                }

                m_Modules.Sort((x, y) => x.Name.CompareTo(y.Name));
            }
            catch (Exception p_Exception)
            {
                Logger.Error("[CP_SDK][ChatPlexSDK.InitModules] Error:");
                Logger.Error(p_Exception);
            }
        }
        /// <summary>
        /// Stop modules
        /// </summary>
        internal static void StopModules()
        {
            for (int l_I = 0; l_I < m_Modules.Count; l_I++)
            {
                try
                {
                    var l_Module = m_Modules[l_I];
                    l_Module.OnApplicationExit();
                }
                catch (Exception p_Exception)
                {
                    Logger.Error("[CP_SDK][ChatPlexSDK.StopModules] Error:");
                    Logger.Error(p_Exception);
                }
            }
        }
        /// <summary>
        /// Get modules
        /// </summary>
        /// <returns></returns>
        internal static List<IModuleBase> GetModules()
            => new List<IModuleBase>(m_Modules);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On generic menu scene
        /// </summary>
        internal static void Fire_OnGenericMenuSceneLoaded()
        {
            try
            {
                UI.LoadingProgressBar.TouchInstance();
            }
            catch (Exception l_Exception)
            {
                Logger.Error("[CP_SDK][ChatPlexSDK.Fire_OnGenericMenuSceneLoaded] Error :");
                Logger.Error(l_Exception);
            }

            ActiveGenericScene = EGenericScene.Menu;

            for (int l_I = 0; l_I < m_Modules.Count; l_I++)
            {
                var l_Module = m_Modules[l_I];

                try                                 { l_Module.CheckForActivation(EIModuleBaseActivationType.OnMenuSceneLoaded);                                                                }
                catch (Exception p_InitException)   { Logger.Error("[CP_SDK][ChatPlexSDK.Fire_OnGenericMenuSceneLoaded] Error on module init " + l_Module.Name); Logger.Error(p_InitException); }
            }

            try
            {
                OnGenericMenuSceneLoaded?.Invoke();
            }
            catch (Exception l_Exception)
            {
                Logger.Error("[CP_SDK][ChatPlexSDK.OnGenericMenuScene] Error :");
                Logger.Error(l_Exception);
            }
        }
        /// <summary>
        /// On generic menu scene
        /// </summary>
        internal static void Fire_OnGenericMenuScene()
        {
            ActiveGenericScene = EGenericScene.Menu;

            try
            {
                OnGenericSceneChange?.Invoke(EGenericScene.Menu);

                Chat.Service.StartServices();
            }
            catch (Exception l_Exception)
            {
                Logger.Error("[CP_SDK][ChatPlexSDK.OnGenericMenuScene] Error :");
                Logger.Error(l_Exception);
            }
        }
        /// <summary>
        /// On generic play scene
        /// </summary>
        internal static void Fire_OnGenericPlayingScene()
        {
            ActiveGenericScene = EGenericScene.Playing;

            try
            {
                OnGenericSceneChange?.Invoke(EGenericScene.Playing);
            }
            catch (Exception l_Exception)
            {
                Logger.Error("[CP_SDK][ChatPlexSDK.OnGenericPlayingScene] Error:");
                Logger.Error(l_Exception);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Install WEBP codecs
        /// </summary>
        private static void InstallWEBPCodecs()
        {
            try
            {
                /// Installing WEBP codec
                if (!Directory.Exists("Libs/Natives/"))
                    Directory.CreateDirectory("Libs/Natives/");

                if (!File.Exists("Libs/Natives/libwebp.dll"))
                    File.WriteAllBytes("Libs/Natives/libwebp.dll", Misc.Resources.FromRelPath(Assembly.GetExecutingAssembly(), "CP_SDK._Resources.libwebp.dll"));
                if (!File.Exists("Libs/Natives/libwebpdemux.dll"))
                    File.WriteAllBytes("Libs/Natives/libwebpdemux.dll", Misc.Resources.FromRelPath(Assembly.GetExecutingAssembly(), "CP_SDK._Resources.libwebpdemux.dll"));
            }
            catch (Exception l_Exception)
            {
                Logger.Error("[CP_SDK][ChatPlexSDK.InstallWEBPCodecs] Error:");
                Logger.Error(l_Exception);
            }
        }
    }
}



