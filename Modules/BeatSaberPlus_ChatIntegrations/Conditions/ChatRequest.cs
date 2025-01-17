﻿using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BeatSaberPlus_ChatIntegrations.Conditions
{
    public class ChatRequest_QueueDuration : Interfaces.ICondition<ChatRequest_QueueDuration, Models.Conditions.ChatRequest_QueueDuration>
    {
        public override string Description => "Add conditions on chat request queue duration!";

#pragma warning disable CS0414
        [UIComponent("CheckTypeList")]
        private ListSetting m_CheckTypeList = null;
        [UIValue("CheckTypeList_Choices")]
        private List<object> m_CheckTypeList_Choices = new List<object>() { "Greater than", "Less than" };
        [UIValue("CheckTypeList_Value")]
        private string m_CheckTypeList_Value;
        [UIComponent("DurationSlider")]
        private SliderSetting m_DurationSlider = null;
        [UIComponent("SendMessageOnFailToggle")]
        private ToggleSetting m_SendMessageOnFailToggle = null;
#pragma warning restore CS0414

        public override sealed void BuildUI(Transform p_Parent)
        {
            m_CheckTypeList_Value = (string)m_CheckTypeList_Choices.ElementAt(Model.IsGreaterThan ? 0 : 1);

            string l_BSML = CP_SDK.Misc.Resources.FromPathStr(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            BeatSaberPlus.SDK.UI.ListSetting.Setup(m_CheckTypeList,               l_Event,                                                                                  false);
            BeatSaberPlus.SDK.UI.SliderSetting.Setup(m_DurationSlider,            l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.Time, Model.Duration,                 true, true, new Vector2(0.08f, 0.10f), new Vector2(0.93f, 0.90f));
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_SendMessageOnFailToggle,   l_Event,                                                  Model.SendChatMessageOnFail,    false);

            OnSettingChanged(null);

            if (!ModulePresence.ChatRequest)
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: ChatRequest module is missing!");
        }
        private void OnSettingChanged(object p_Value)
        {
            Model.IsGreaterThan         = m_CheckTypeList_Choices.Select(x => (string)x).ToList().IndexOf(m_CheckTypeList.Value) == 0;
            Model.Duration              = (uint)m_DurationSlider.slider.value;
            Model.SendChatMessageOnFail = m_SendMessageOnFailToggle.Value;
        }

        public override bool Eval(Models.EventContext p_Context)
        {
            if (!ModulePresence.ChatRequest)
            {
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, ChatRequest module is missing!");
                return false;
            }

            var l_ChatRequest = BeatSaberPlus_ChatRequest.ChatRequest.Instance;

            if (l_ChatRequest == null || !l_ChatRequest.IsEnabled)
            {
                if (Model.SendChatMessageOnFail && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                    p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} chat request is not enabled");

                return false;
            }

            if (Model.IsGreaterThan)
            {
                if (l_ChatRequest.QueueDuration > Model.Duration)
                    return true;

                if (Model.SendChatMessageOnFail && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                    p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} queue is too short");
            }
            else
            {
                if (l_ChatRequest.QueueDuration < Model.Duration)
                    return true;

                if (Model.SendChatMessageOnFail && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                    p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} queue is too long");
            }

            return false;
        }
    }

    public class ChatRequest_QueueSize : Interfaces.ICondition<ChatRequest_QueueSize, Models.Conditions.ChatRequest_QueueSize>
    {
        public override string Description => "Add conditions on chat request queue size!";

#pragma warning disable CS0414
        [UIComponent("CheckTypeList")]
        private ListSetting m_CheckTypeList = null;
        [UIValue("CheckTypeList_Choices")]
        private List<object> m_CheckTypeList_Choices = new List<object>() { "Greater than", "Less than" };
        [UIValue("CheckTypeList_Value")]
        private string m_CheckTypeList_Value;
        [UIComponent("CountSlider")]
        private SliderSetting m_CountSlider = null;
        [UIComponent("SendMessageOnFailToggle")]
        private ToggleSetting m_SendMessageOnFailToggle = null;
#pragma warning restore CS0414

        public override sealed void BuildUI(Transform p_Parent)
        {
            m_CheckTypeList_Value = (string)m_CheckTypeList_Choices.ElementAt(Model.IsGreaterThan ? 0 : 1);

            string l_BSML = CP_SDK.Misc.Resources.FromPathStr(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            BeatSaberPlus.SDK.UI.ListSetting.Setup(m_CheckTypeList,               l_Event,                                    false);
            BeatSaberPlus.SDK.UI.SliderSetting.Setup(m_CountSlider,               l_Event, null, Model.Count,                 true, true, new Vector2(0.08f, 0.10f), new Vector2(0.93f, 0.90f));
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_SendMessageOnFailToggle,   l_Event,       Model.SendChatMessageOnFail, false);

            OnSettingChanged(null);

            if (!ModulePresence.ChatRequest)
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: ChatRequest module is missing!");
        }
        private void OnSettingChanged(object p_Value)
        {
            Model.IsGreaterThan         = m_CheckTypeList_Choices.Select(x => (string)x).ToList().IndexOf(m_CheckTypeList.Value) == 0;
            Model.Count                 = (uint)m_CountSlider.slider.value;
            Model.SendChatMessageOnFail = m_SendMessageOnFailToggle.Value;
        }

        public override bool Eval(Models.EventContext p_Context)
        {
            if (!ModulePresence.ChatRequest)
            {
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, ChatRequest module is missing!");
                return false;
            }

            var l_ChatRequest = BeatSaberPlus_ChatRequest.ChatRequest.Instance;

            if (l_ChatRequest == null || !l_ChatRequest.IsEnabled)
            {
                if (Model.SendChatMessageOnFail && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                    p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} chat request is not enabled");

                return false;
            }

            int l_QueueSize = l_ChatRequest.SongQueueCount;

            if (Model.IsGreaterThan)
            {
                if (l_QueueSize > Model.Count)
                    return true;

                if (Model.SendChatMessageOnFail && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                    p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} queue is too small");
            }
            else
            {
                if (l_QueueSize < Model.Count)
                    return true;

                if (Model.SendChatMessageOnFail && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                    p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} queue is too big");
            }

            return false;
        }
    }

    public class ChatRequest_QueueStatus : Interfaces.ICondition<ChatRequest_QueueStatus, Models.Conditions.ChatRequest_QueueStatus>
    {
        public override string Description => "Add conditions on chat request queue!";

#pragma warning disable CS0414
        [UIComponent("StatusList")]
        private ListSetting m_StatusList = null;
        [UIValue("StatusList_Choices")]
        private List<object> m_StatusList_Choices = new List<object>() { "Open", "Closed" };
        [UIValue("StatusList_Value")]
        private string m_StatusList_Value;
        [UIComponent("SendMessageOnFailToggle")]
        private ToggleSetting m_SendMessageOnFailToggle = null;
#pragma warning restore CS0414

        public override sealed void BuildUI(Transform p_Parent)
        {
            m_StatusList_Value = (string)m_StatusList_Choices.ElementAt(Model.IsOpen ? 0 : 1);

            string l_BSML = CP_SDK.Misc.Resources.FromPathStr(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            BeatSaberPlus.SDK.UI.ListSetting.Setup(m_StatusList,                  l_Event,                                false);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_SendMessageOnFailToggle,   l_Event, Model.SendChatMessageOnFail,   false);

            OnSettingChanged(null);

            if (!ModulePresence.ChatRequest)
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: ChatRequest module is missing!");
        }
        private void OnSettingChanged(object p_Value)
        {
            Model.IsOpen                = m_StatusList_Choices.Select(x => (string)x).ToList().IndexOf(m_StatusList.Value) == 0;
            Model.SendChatMessageOnFail = m_SendMessageOnFailToggle.Value;
        }

        public override bool Eval(Models.EventContext p_Context)
        {
            if (!ModulePresence.ChatRequest)
            {
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, ChatRequest module is missing!");
                return false;
            }

            var l_ChatRequest = BeatSaberPlus_ChatRequest.ChatRequest.Instance;

            if (l_ChatRequest == null || !l_ChatRequest.IsEnabled)
            {
                if (Model.SendChatMessageOnFail && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                    p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} chat request is not enabled");

                return false;
            }

            if (Model.IsOpen && !l_ChatRequest.QueueOpen)
            {
                if (Model.SendChatMessageOnFail && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                    p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} queue is closed");

                return false;
            }
            else if (!Model.IsOpen && l_ChatRequest.QueueOpen)
            {
                if (Model.SendChatMessageOnFail && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                    p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} queue is open");

                return false;
            }

            return true;
        }
    }
}
