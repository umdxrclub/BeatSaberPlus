﻿using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberPlus_ChatIntegrations.Interfaces;
using BeatSaberPlus_ChatIntegrations.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using TMPro;
using UnityEngine;

namespace BeatSaberPlus_ChatIntegrations.Events
{
    /// <summary>
    /// Chat command event
    /// </summary>
    public class ChatPointsReward : IEvent<ChatPointsReward, Models.Events.ChatPointsReward>
    {
        /// <summary>
        /// Provided values list
        /// </summary>
        public override IReadOnlyList<(IValueType, string)> ProvidedValues { get; protected set; }
        /// <summary>
        /// Available conditions list
        /// </summary>
        public override IReadOnlyList<IConditionBase> AvailableConditions { get; protected set; }
        /// <summary>
        /// Available actions list
        /// </summary>
        public override IReadOnlyList<IActionBase> AvailableActions { get; protected set; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        public ChatPointsReward()
        {
            /// Build provided values list
            ProvidedValues = new List<(IValueType, string)>()
            {
                (IValueType.Integer, "MessageNumber"),
                (IValueType.String,  "MessageContent"),
                (IValueType.String,  "UserName")
            }.AsReadOnly();

            /// Build possible list
            AvailableConditions = new List<IConditionBase>()
            {
                new Conditions.ChatRequest_QueueDuration(),
                new Conditions.ChatRequest_QueueSize(),
                new Conditions.ChatRequest_QueueStatus(),
                new Conditions.GamePlay_InMenu(),
                new Conditions.GamePlay_PlayingMap(),
                new Conditions.Misc_Cooldown(),
            }
            .Union(BeatSaberPlus_ChatIntegrations.Conditions.EventBuilder.BuildFor(this))
            .Union(BeatSaberPlus_ChatIntegrations.Conditions.OBSBuilder.BuildFor(this))
            .Union(GetInstanciatedCustomConditionList())
            .Distinct().ToList().AsReadOnly();

            /// Build possible list
            AvailableActions = new List<IActionBase>()
            {

            }
            .Union(BeatSaberPlus_ChatIntegrations.Actions.Camera2Builder.BuildFor(this))
            .Union(BeatSaberPlus_ChatIntegrations.Actions.ChatBuilder.BuildFor(this))
            .Union(BeatSaberPlus_ChatIntegrations.Actions.EmoteRainBuilder.BuildFor(this))
            .Union(BeatSaberPlus_ChatIntegrations.Actions.EventBuilder.BuildFor(this))
            .Union(BeatSaberPlus_ChatIntegrations.Actions.GamePlayBuilder.BuildFor(this))
            .Union(BeatSaberPlus_ChatIntegrations.Actions.MiscBuilder.BuildFor(this))
            .Union(BeatSaberPlus_ChatIntegrations.Actions.NoteTweakerBuilder.BuildFor(this))
            .Union(BeatSaberPlus_ChatIntegrations.Actions.OBSBuilder.BuildFor(this))
            .Union(BeatSaberPlus_ChatIntegrations.Actions.TwitchBuilder.BuildFor(this))
            .Union(BeatSaberPlus_ChatIntegrations.Actions.SongChartVisualizerBuilder.BuildFor(this))
            .Union(GetInstanciatedCustomActionList())
            .Distinct().ToList().AsReadOnly();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#pragma warning disable CS0414
        [UIObject("InfoBackground")]
        private GameObject m_InfoBackground = null;

        [UIComponent("TitleText")]
        private TextMeshProUGUI m_TitleText = null;
        [UIComponent("PromptText")]
        private TextMeshProUGUI m_PromptText = null;
        [UIComponent("CostText")]
        private TextMeshProUGUI m_CostText = null;

        [UIComponent("RequireInputToggle")]
        private ToggleSetting m_RequireInputToggle = null;
        [UIComponent("MaxPerStreamIncrement")]
        private IncrementSetting m_MaxPerStreamIncrement = null;
        [UIComponent("MaxPerUserPerStreamIncrement")]
        private IncrementSetting m_MaxPerUserPerStreamIncrement = null;
        [UIComponent("CooldownIncrement")]
        private IncrementSetting m_CooldownIncrement = null;
        [UIComponent("AutoFullfillRefund")]
        private ToggleSetting m_AutoFullfillRefund = null;
#pragma warning restore CS0414

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// BSML parser params instance
        /// </summary>
        private BSMLParserParams m_ParserParams;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Build editing UI
        /// </summary>
        /// <param name="p_Parent">Parent transform</param>
        public override sealed void BuildUI(Transform p_Parent)
        {
            string l_BSML = CP_SDK.Misc.Resources.FromPathStr(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            m_ParserParams = BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event             = new BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged),  BindingFlags.Instance | BindingFlags.NonPublic));
            var l_QuantityFormatter = new BSMLAction(this, this.GetType().GetMethod(nameof(QuantityFormatter), BindingFlags.Instance | BindingFlags.NonPublic));
            var l_CooldownFormatter = new BSMLAction(this, this.GetType().GetMethod(nameof(CooldownFormatter), BindingFlags.Instance | BindingFlags.NonPublic));

            /// Change opacity
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_InfoBackground, 0.5f);

            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_RequireInputToggle,                l_Event,                      Model.RequireInput,          true);
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_MaxPerStreamIncrement,          l_Event, l_QuantityFormatter, Model.MaxPerStream,          true);
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_MaxPerUserPerStreamIncrement,   l_Event, l_QuantityFormatter, Model.MaxPerUserPerStream,   true);
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_CooldownIncrement,              l_Event, l_CooldownFormatter, Model.Cooldown,              true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_AutoFullfillRefund,                l_Event,                      Model.AutoFullfillRefund,    true);

            m_PromptText.fontSizeMax = m_PromptText.fontSize;
            m_PromptText.fontSizeMin = 1;
            m_PromptText.enableAutoSizing = true;

            /// Update UI
            UpdateUI();
        }
        /// <summary>
        /// On setting changed
        /// </summary>
        /// <param name="p_Value">Value</param>
        private void OnSettingChanged(object p_Value)
        {
            Model.RequireInput          = m_RequireInputToggle.Value;
            Model.MaxPerStream          = (int)m_MaxPerStreamIncrement.Value;
            Model.MaxPerUserPerStream   = (int)m_MaxPerUserPerStreamIncrement.Value;
            Model.Cooldown              = (int)m_CooldownIncrement.Value;
            Model.AutoFullfillRefund    = m_AutoFullfillRefund.Value;
        }
        /// <summary>
        /// Update UI component values
        /// </summary>
        private void UpdateUI()
        {
            m_TitleText.SetText(Model.Title + " ");
            m_PromptText.SetText(Model.Prompt + " ");
            m_CostText.SetText(Model.Cost + " ");
        }
        /// <summary>
        /// Title button pressed
        /// </summary>
        [UIAction("click-title-btn-pressed")]
        private void OnTitleButton()
        {
            UI.Settings.Instance.UIShowInputKeyboard(Model.Title, (x) =>
            {
                Model.Title = x.Length > 45 ? x.Substring(0, 45) : x;
                UpdateUI();
            });
        }
        /// <summary>
        /// Prompt button pressed
        /// </summary>
        [UIAction("click-prompt-btn-pressed")]
        private void OnPromptButton()
        {
            UI.Settings.Instance.UIShowInputKeyboard(Model.Prompt, (x) =>
            {
                Model.Prompt = x.Length > 70 ? x.Substring(0, 70) : x;
                UpdateUI();
            });
        }
        /// <summary>
        /// Cost button pressed
        /// </summary>
        [UIAction("click-cost-btn-pressed")]
        private void OnCostButton()
        {
            UI.Settings.Instance.UIShowInputKeyboard(Model.Cost.ToString(), (x) =>
            {
                int l_NewValue = 0;
                if (!int.TryParse(x, out l_NewValue))
                    l_NewValue = Model.Cost;
                else
                    l_NewValue = Mathf.Clamp(l_NewValue, 0, 10000000);

                Model.Cost = l_NewValue;
                UpdateUI();
            });
        }
        /// <summary>
        /// Update reward button pressed
        /// </summary>
        [UIAction("click-update-reward-btn-pressed")]
        private void OnUpdateRewardButton()
        {
            CreateOrUpdateReward();
        }
        /// <summary>
        /// Quantity formatter
        /// </summary>
        /// <param name="p_Value">New value</param>
        /// <returns></returns>
        private string QuantityFormatter(int p_Value)
        {
            return p_Value == 0 ? "Unlimited" : p_Value.ToString();
        }
        /// <summary>
        /// Cooldown formatter
        /// </summary>
        /// <param name="p_Value">New value</param>
        /// <returns></returns>
        private string CooldownFormatter(int p_Value)
        {
            if (p_Value == 0)
                return "Unlimited";

            int l_Minutes = p_Value / 60;
            int l_Seconds = p_Value - (l_Minutes * 60);

            string l_Result = (l_Minutes != 0 ? l_Minutes : l_Seconds).ToString();
            if (l_Minutes != 0)
                l_Result += "m " + l_Seconds + "s";
            else
                l_Result += "s";

            return l_Result;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On import or clone
        /// </summary>
        /// <param name="p_IsImport">Is an import</param>
        /// <param name="p_IsClone">Is a clone</param>
        public override void OnImportOrClone(bool p_IsImport, bool p_IsClone)
        {
            base.OnImportOrClone(p_IsImport, p_IsClone);

            if (p_IsImport)
                Model.Title += " (Import)";
            if (p_IsClone)
                Model.Title += " (Clone)";

            Model.RewardID = "";
        }
        /// <summary>
        /// When the event is enabled
        /// </summary>
        public override sealed void OnEnable()
        {
            CreateOrUpdateReward();
        }
        /// <summary>
        /// When the event is successful
        /// </summary>
        /// <param name="p_Context">Event context</param>
        public override void OnSuccess(EventContext p_Context)
        {
            if (!Model.AutoFullfillRefund)
                return;

            var l_TwitchService = CP_SDK.Chat.Service.Multiplexer.Services.FirstOrDefault(x => x is CP_SDK.Chat.Services.Twitch.TwitchService);
            var l_TwitchHelix   = l_TwitchService != null ? (l_TwitchService as CP_SDK.Chat.Services.Twitch.TwitchService).HelixAPI : null;

            if (l_TwitchHelix != null)
            {
                var l_URL           = $"https://api.twitch.tv/helix/channel_points/custom_rewards/redemptions?broadcaster_id={l_TwitchHelix.BroadcasterID}&reward_id={Model.RewardID}&id={p_Context.PointsEvent.TransactionID}";
                var l_Content       = new JObject()
                {
                    ["status"] = "FULFILLED",
                };
                var l_ContentStr    = new StringContent(l_Content.ToString(), Encoding.UTF8, "application/json");

                l_TwitchHelix.APIClient.PatchAsync(l_URL, l_ContentStr, CancellationToken.None, true).ConfigureAwait(false);
            }
        }
        /// <summary>
        /// When the event failed
        /// </summary>
        /// <param name="p_Context">Event context</param>
        public override sealed void OnEventFailed(Models.EventContext p_Context)
        {
            if (!Model.AutoFullfillRefund)
                return;

            var l_TwitchService = CP_SDK.Chat.Service.Multiplexer.Services.FirstOrDefault(x => x is CP_SDK.Chat.Services.Twitch.TwitchService);
            var l_TwitchHelix   = l_TwitchService != null ? (l_TwitchService as CP_SDK.Chat.Services.Twitch.TwitchService).HelixAPI : null;

            if (l_TwitchHelix != null)
            {
                var l_URL           = $"https://api.twitch.tv/helix/channel_points/custom_rewards/redemptions?broadcaster_id={l_TwitchHelix.BroadcasterID}&reward_id={Model.RewardID}&id={p_Context.PointsEvent.TransactionID}";
                var l_Content       = new JObject()
                {
                    ["status"] = "CANCELED",
                };
                var l_ContentStr = new StringContent(l_Content.ToString(), Encoding.UTF8, "application/json");

                l_TwitchHelix.APIClient.PatchAsync(l_URL, l_ContentStr, CancellationToken.None, true).ContinueWith((x) =>
                {
                    if (p_Context.ChatService != null && p_Context.Channel != null)
                        p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.UserName} Event failed, your points were refunded!");
                }).ConfigureAwait(false);
            }
        }
        /// <summary>
        /// When the event is disabled
        /// </summary>
        public override sealed void OnDisable()
        {
            if (CP_SDK.Chat.Service.Multiplexer.Channels.Count == 0)
                return;

            var l_TwitchService = CP_SDK.Chat.Service.Multiplexer.Services.FirstOrDefault(x => x is CP_SDK.Chat.Services.Twitch.TwitchService);
            var l_TwitchHelix   = l_TwitchService != null ? (l_TwitchService as CP_SDK.Chat.Services.Twitch.TwitchService).HelixAPI : null;

            if (l_TwitchHelix != null)
            {
                var l_URL           = $"https://api.twitch.tv/helix/channel_points/custom_rewards?broadcaster_id={l_TwitchHelix.BroadcasterID}&id={Model.RewardID}";
                var l_Content       = new JObject()
                {
                    ["is_enabled"] = false
                };
                var l_ContentStr = new StringContent(l_Content.ToString(), Encoding.UTF8, "application/json");

                l_TwitchHelix.APIClient.PatchAsync(l_URL, l_ContentStr, CancellationToken.None, true).ConfigureAwait(false);
            }
        }
        /// <summary>
        /// When the event is deleted
        /// </summary>
        public override sealed void OnDelete()
        {
            DeleteReward();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Handle
        /// </summary>
        /// <param name="p_Context">Event context</param>
        protected override sealed bool CanBeExecuted(Models.EventContext p_Context)
        {
            /// Ensure that we have all data
            if (p_Context.Type != TriggerType.ChatPointsReward || p_Context.ChatService == null || p_Context.Channel == null || p_Context.User == null)
                return false;

            return p_Context.PointsEvent.RewardID == Model.RewardID;
        }
        /// <summary>
        /// Build provided value dictionary
        /// </summary>
        /// <param name="p_Context">Event context</param>
        protected override sealed void BuildProvidedValues(Models.EventContext p_Context)
        {
            p_Context.AddValue(IValueType.String, "UserName",       (string)p_Context.User.DisplayName);
            p_Context.AddValue(IValueType.String, "MessageContent", (string)p_Context.PointsEvent.UserInput);

            if (!string.IsNullOrEmpty(p_Context.PointsEvent.UserInput) && Int64.TryParse(Regex.Match(p_Context.PointsEvent.UserInput, @"-?\d+").Value, out var l_Number))
                p_Context.AddValue(IValueType.Integer, "MessageNumber",  (Int64?)l_Number);
            else
                p_Context.AddValue(IValueType.Integer, "MessageNumber",  (Int64?)null);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create or update the reward on twitch
        /// </summary>
        private void CreateOrUpdateReward()
        {
            var l_TwitchService = CP_SDK.Chat.Service.Multiplexer.Services.FirstOrDefault(x => x is CP_SDK.Chat.Services.Twitch.TwitchService);
            var l_TwitchHelix   = l_TwitchService != null ? (l_TwitchService as CP_SDK.Chat.Services.Twitch.TwitchService).HelixAPI : null;

            if (l_TwitchHelix != null)
            {
                var l_ShouldCreate  = string.IsNullOrEmpty(Model.RewardID);
                var l_URL           = $"https://api.twitch.tv/helix/channel_points/custom_rewards?broadcaster_id={l_TwitchHelix.BroadcasterID}&id={Model.RewardID}";

                if (UI.Settings.Instance != null)
                    UI.Settings.Instance.UIShowLoading();

                l_TwitchHelix.APIClient.GetAsync(l_URL, CancellationToken.None, true).ContinueWith((p_GetReply) =>
                {
                    l_ShouldCreate = p_GetReply.Result == null || !p_GetReply.Result.IsSuccessStatusCode;

                    l_URL = $"https://api.twitch.tv/helix/channel_points/custom_rewards?broadcaster_id={l_TwitchHelix.BroadcasterID}";
                    if (!l_ShouldCreate)
                        l_URL += $"&id={Model.RewardID}";

                    var l_Content = new JObject()
                    {
                        ["title"]                               = Model.Title,
                        ["is_user_input_required"]              = Model.RequireInput,
                        ["prompt"]                              = Model.Prompt,
                        ["cost"]                                = Model.Cost,
                        ["is_enabled"]                          = Model.Enabled,
                        ["is_max_per_stream_enabled"]           = Model.MaxPerStream > 0,
                        ["max_per_stream"]                      = Model.MaxPerStream,
                        ["is_max_per_user_per_stream_enabled"]  = Model.MaxPerUserPerStream > 0,
                        ["max_per_user_per_stream"]             = Model.MaxPerUserPerStream,
                        ["is_global_cooldown_enabled"]          = Model.Cooldown > 0,
                        ["global_cooldown_seconds"]             = Model.Cooldown
                    };

                    var l_ContentStr = new StringContent(l_Content.ToString(), Encoding.UTF8, "application/json");

                    var l_Task = l_ShouldCreate ?
                                    l_TwitchHelix.APIClient.PostAsync(l_URL, l_ContentStr, CancellationToken.None, true)
                                :
                                    l_TwitchHelix.APIClient.PatchAsync(l_URL, l_ContentStr, CancellationToken.None, true);

                    l_Task.ContinueWith((p_SecondReply) =>
                    {
                        if (p_SecondReply.Result != null)
                        {
                            if (p_SecondReply.Result.StatusCode == System.Net.HttpStatusCode.BadRequest)
                            {
                                if (UI.Settings.Instance != null)
                                {
                                    CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() =>
                                    {
                                        if (p_SecondReply.Result.BodyString.Contains("CREATE_CUSTOM_REWARD_DUPLICATE_REWARD"))
                                            UI.Settings.Instance.UISetPendingMessage("<color=red>Twitch error,\nA reward with the same name already exist on your channel\nPlease delete on twitch.tv any conflicting reward");
                                        else
                                            UI.Settings.Instance.UISetPendingMessage("<color=red>Twitch error,\n" + p_SecondReply.Result.BodyString);

                                        UI.Settings.Instance.UIHideLoading();
                                    });
                                }
                                return;
                            }

                            if (l_ShouldCreate)
                            {
                                JObject l_Response = JObject.Parse(p_SecondReply.Result.BodyString);
                                if (l_Response == null
                                    || !l_Response.ContainsKey("data")
                                    || l_Response["data"].Type != JTokenType.Array
                                    || (l_Response["data"] as JArray).Count != 1
                                    || !((l_Response["data"] as JArray)[0] as JObject).ContainsKey("id")
                                    )
                                {
                                    Logger.Instance.Error("[ChatIntegration][ChatPointReward.CreateOrUpdateReward] Error:");
                                    Logger.Instance.Error(p_SecondReply.Result.BodyString != null ? p_SecondReply.Result.BodyString : "empty response");

                                    if (UI.Settings.Instance != null)
                                    {
                                        CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() =>
                                        {
                                            UI.Settings.Instance.UISetPendingMessage("<color=red>Internal error,\nplease contact BS+ support!");
                                            UI.Settings.Instance.UIHideLoading();
                                        });
                                    }
                                    return;
                                }

                                Model.RewardID = l_Response["data"][0]["id"].Value<string>();
                            }

                            if (UI.Settings.Instance != null)
                                CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() => UI.Settings.Instance.UIHideLoading());
                        }
                        else
                        {
                            Logger.Instance.Error("[ChatIntegration][ChatPointReward.CreateOrUpdateReward] Error 2:");
                            Logger.Instance.Error("empty response");

                            if (UI.Settings.Instance != null)
                            {
                                CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() =>
                                {
                                    UI.Settings.Instance.UISetPendingMessage("<color=red>Internal error,\nplease contact BS+ support!");
                                    UI.Settings.Instance.UIHideLoading();
                                });
                            }
                        }
                    });
                }).ConfigureAwait(false);
            }
        }
        /// <summary>
        /// Delete the reward on twitch
        /// </summary>
        private void DeleteReward()
        {
            var l_TwitchService = CP_SDK.Chat.Service.Multiplexer.Services.FirstOrDefault(x => x is CP_SDK.Chat.Services.Twitch.TwitchService);
            var l_TwitchHelix   = l_TwitchService != null ? (l_TwitchService as CP_SDK.Chat.Services.Twitch.TwitchService).HelixAPI : null;

            if (l_TwitchHelix != null)
            {
                var l_URL           = $"https://api.twitch.tv/helix/channel_points/custom_rewards?broadcaster_id={l_TwitchHelix.BroadcasterID}&id={Model.RewardID}";

                l_TwitchHelix.APIClient.DeleteAsync(l_URL, CancellationToken.None, true).ConfigureAwait(false);
            }
        }
    }
}
