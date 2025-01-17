﻿using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberPlus_ChatIntegrations.Models;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System;
using UnityEngine;

namespace BeatSaberPlus_ChatIntegrations.Actions
{
    internal class EmoteRainBuilder
    {
        internal static List<Interfaces.IActionBase> BuildFor(Interfaces.IEventBase p_Event)
        {
            switch (p_Event)
            {
                default:
                    break;
            }

            return new List<Interfaces.IActionBase>()
            {
                new EmoteRain_CustomRain(),
                new EmoteRain_EmoteBombRain(),
                new EmoteRain_SubRain()
            };
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class EmoteRain_CustomRain : Interfaces.IAction<EmoteRain_CustomRain, Models.Actions.EmoteRain_CustomRain>
    {
        public override string Description => "Make rain custom emotes";

        private BSMLParserParams m_ParserParams;
        private CP_SDK.Unity.EnhancedImage m_LoadedImage = null;
        private string m_LoadedImageID = "";
        private string m_LoadedImageName = "";

#pragma warning disable CS0414
        [UIComponent("File_DropDown")]
        protected DropDownListSetting m_File_DropDown = null;
        [UIValue("File_DropDownOptions")]
        private List<object> m_File_DropDownOptions = new List<object>() { "Loading...", };
        [UIComponent("CountIncrement")]
        protected IncrementSetting m_CountIncrement = null;
        [UIObject("InfoPanel_Background")]
        private GameObject m_InfoPanel_Background = null;

        [UIObject("EmoteRainNotEnabledModal")]
        private GameObject m_EmoteRainNotEnabledModal = null;
#pragma warning restore CS0414

        public override sealed void BuildUI(Transform p_Parent)
        {
            string l_BSML = CP_SDK.Misc.Resources.FromPathStr(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            m_ParserParams = BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            /// Change opacity
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_InfoPanel_Background, 0.75f);
            BeatSaberPlus.SDK.UI.ModalView.SetOpacity(m_EmoteRainNotEnabledModal, 0.75f);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_CountIncrement, l_Event, null, Model.Count, false);
            BeatSaberPlus.SDK.UI.DropDownListSetting.Setup(m_File_DropDown, l_Event, true);

            var l_Files = Directory.GetFiles(ChatIntegrations.s_EMOTE_RAIN_ASSETS_PATH, "*.png")
                   .Union(Directory.GetFiles(ChatIntegrations.s_EMOTE_RAIN_ASSETS_PATH, "*.gif"))
                   .Union(Directory.GetFiles(ChatIntegrations.s_EMOTE_RAIN_ASSETS_PATH, "*.apng")).ToArray();

            bool l_ChoiceExist = false;
            var l_Choices = new List<object>();
            l_Choices.Add("<i>None</i>");
            foreach (var l_CurrentFile in l_Files)
            {
                var l_Filtered = Path.GetFileName(l_CurrentFile);
                l_Choices.Add(l_Filtered);

                if (l_Filtered == Model.BaseValue)
                    l_ChoiceExist = true;
            }

            m_File_DropDownOptions  = l_Choices;
            m_File_DropDown.values  = l_Choices;
            m_File_DropDown.Value   = l_ChoiceExist ? Model.BaseValue : l_Choices[0];
            m_File_DropDown.UpdateChoices();

            if (!ModulePresence.ChatEmoteRain)
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: ChatEmoteRain module is missing!");
        }
        private void OnSettingChanged(object p_Value)
        {
            Model.BaseValue = (string)m_File_DropDown.Value;
            Model.Count     = (uint)m_CountIncrement.Value;

            if ((string)p_Value == "<i>None</i>")
                Model.BaseValue = "";
        }

        [UIAction("click-test-btn-pressed")]
        private void OnTestButton()
        {
            if (ChatPlexMod_ChatEmoteRain.ChatEmoteRain.Instance == null || !ChatPlexMod_ChatEmoteRain.ChatEmoteRain.Instance.IsEnabled)
            {
                m_ParserParams.EmitEvent("ShowEmoteRainNotEnabledModal");
                return;
            }

            MakeItRain();
        }

        public override IEnumerator Eval(EventContext p_Context)
        {
            if (!ModulePresence.ChatEmoteRain)
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, ChatEmoteRain module is missing!");
                yield break;
            }

            if (ChatPlexMod_ChatEmoteRain.ChatEmoteRain.Instance != null && ChatPlexMod_ChatEmoteRain.ChatEmoteRain.Instance.IsEnabled)
                MakeItRain();
            else
                p_Context.HasActionFailed = true;

            yield return null;
        }

        private void MakeItRain()
        {
            EnsureLoaded((p_Loaded) =>
            {
                if (m_LoadedImage == null)
                    return;

                CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() => ChatPlexMod_ChatEmoteRain.ChatEmoteRain.Instance.EmitEnhancedImage(m_LoadedImage, Model.Count));
            });
        }
        private void EnsureLoaded(Action<bool> p_Callback)
        {
            CP_SDK.Unity.MTThreadInvoker.EnqueueOnThread(() =>
            {
                if (Model.BaseValue == "None")
                {
                    p_Callback?.Invoke(false);
                    return;
                }

                if (m_LoadedImageName != Model.BaseValue)
                {
                    m_LoadedImageName = Model.BaseValue;

                    string l_Path = Path.Combine(ChatIntegrations.s_EMOTE_RAIN_ASSETS_PATH, Model.BaseValue);
                    if (File.Exists(l_Path))
                    {
                        m_LoadedImageID = "$BSP$CI$_" + Model.BaseValue;
                        CP_SDK.Unity.EnhancedImage.FromFile(l_Path, m_LoadedImageID, (p_Result) =>
                        {
                            m_LoadedImage = p_Result;
                            p_Callback?.Invoke(m_LoadedImage != null);
                        });
                    }
                    else
                        p_Callback?.Invoke(false);
                }

                p_Callback?.Invoke(true);
            });
        }
    }

    public class EmoteRain_EmoteBombRain : Interfaces.IAction<EmoteRain_EmoteBombRain, Models.Actions.EmoteRain_EmoteBombRain>
    {
        public override string Description => "Trigger a massive emote bomb rain";

        private BSMLParserParams m_ParserParams;

#pragma warning disable CS0414
        [UIComponent("KindIncrement")]
        protected IncrementSetting m_KindIncrement = null;
        [UIComponent("CountIncrement")]
        protected IncrementSetting m_CountIncrement = null;

        [UIObject("EmoteRainNotEnabledModal")]
        private GameObject m_EmoteRainNotEnabledModal = null;
#pragma warning restore CS0414

        public override sealed void BuildUI(Transform p_Parent)
        {
            string l_BSML = CP_SDK.Misc.Resources.FromPathStr(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            m_ParserParams = BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            /// Change opacity
            BeatSaberPlus.SDK.UI.ModalView.SetOpacity(m_EmoteRainNotEnabledModal, 0.75f);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_KindIncrement,  l_Event, null, Model.EmoteKindCount, false);
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_CountIncrement, l_Event, null, Model.CountPerEmote,  false);

            if (!ModulePresence.ChatEmoteRain)
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: ChatEmoteRain module is missing!");
        }
        private void OnSettingChanged(object p_Value)
        {
            Model.EmoteKindCount    = (uint)m_KindIncrement.Value;
            Model.CountPerEmote     = (uint)m_CountIncrement.Value;
        }

        [UIAction("click-test-btn-pressed")]
        private void OnTestButton()
        {
            if (ChatPlexMod_ChatEmoteRain.ChatEmoteRain.Instance == null || !ChatPlexMod_ChatEmoteRain.ChatEmoteRain.Instance.IsEnabled)
            {
                m_ParserParams.EmitEvent("ShowEmoteRainNotEnabledModal");
                return;
            }

            CP_SDK.Unity.MTCoroutineStarter.Start(Eval(null));
        }

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            if (!ModulePresence.ChatEmoteRain)
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, ChatEmoteRain module is missing!");
                yield break;
            }

            if (ChatPlexMod_ChatEmoteRain.ChatEmoteRain.Instance != null && ChatPlexMod_ChatEmoteRain.ChatEmoteRain.Instance.IsEnabled)
            {
                CP_SDK.Unity.MTThreadInvoker.EnqueueOnThread(() =>
                {
                    var l_Emotes =
                        CP_SDK.Chat.ChatImageProvider.CachedEmoteInfo.Values.OrderBy(_ => UnityEngine.Random.Range(0, 1000)).Take((int)Model.EmoteKindCount);

                    foreach (var l_Emote in l_Emotes)
                        ChatPlexMod_ChatEmoteRain.ChatEmoteRain.Instance.EmitEnhancedImage(l_Emote, Model.CountPerEmote);
                });
            }
            else if (p_Context != null)
                p_Context.HasActionFailed = true;

            yield return null;
        }
    }

    public class EmoteRain_SubRain : Interfaces.IAction<EmoteRain_SubRain, Models.Action>
    {
        public override string Description => "Trigger a subscription rain";

        public EmoteRain_SubRain() => UIPlaceHolder = "Will trigger a subscription emote rain";

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            if (!ModulePresence.ChatEmoteRain)
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, ChatEmoteRain module is missing!");
                yield break;
            }

            if (ChatPlexMod_ChatEmoteRain.ChatEmoteRain.Instance != null && ChatPlexMod_ChatEmoteRain.ChatEmoteRain.Instance.IsEnabled)
                ChatPlexMod_ChatEmoteRain.ChatEmoteRain.Instance.StartSubRain();
            else
                p_Context.HasActionFailed = true;

            yield return null;
        }
    }
}
