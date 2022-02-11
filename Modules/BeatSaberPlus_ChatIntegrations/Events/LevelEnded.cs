﻿using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberPlus_ChatIntegrations.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BeatSaberPlus_ChatIntegrations.Events
{
    /// <summary>
    /// Level ended event
    /// </summary>
    public class LevelEnded : IEvent<LevelEnded, Models.Event>
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
        public LevelEnded()
        {
            /// Build provided values list
            ProvidedValues = new List<(IValueType, string)>()
            {
                (IValueType.Integer,  "NoteCount"),
                (IValueType.Integer,  "HitCount"),
                (IValueType.Integer,  "MissCount"),
                (IValueType.Floating, "Accuracy"),
                (IValueType.String,   "SongName"),
                (IValueType.String,   "Difficulty")
            }.AsReadOnly();

            /// Build possible list
            AvailableConditions = new List<IConditionBase>()
            {
                new Conditions.ChatRequest_QueueDuration(),
                new Conditions.ChatRequest_QueueSize(),
                new Conditions.ChatRequest_QueueStatus(),
                new Conditions.Misc_Cooldown(),
                new Conditions.Event_AlwaysFail(),
                new Conditions.GamePlay_LevelEndType()
            }
            .Union(GetInstanciatedCustomConditionList())
            .Distinct().ToList().AsReadOnly();

            /// Build possible list
            AvailableActions = new List<IActionBase>()
            {

            }
            .Union(BeatSaberPlus_ChatIntegrations.Actions.ChatBuilder.BuildFor(this))
            .Union(BeatSaberPlus_ChatIntegrations.Actions.EmoteRainBuilder.BuildFor(this))
            .Union(BeatSaberPlus_ChatIntegrations.Actions.EventBuilder.BuildFor(this))
            .Union(BeatSaberPlus_ChatIntegrations.Actions.GamePlayBuilder.BuildFor(this))
            .Union(BeatSaberPlus_ChatIntegrations.Actions.MiscBuilder.BuildFor(this))
            .Union(BeatSaberPlus_ChatIntegrations.Actions.TwitchBuilder.BuildFor(this))
            .Union(GetInstanciatedCustomActionList())
            .Distinct().ToList().AsReadOnly();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#pragma warning disable CS0414
        [UIObject("InfoBackground")]
        private GameObject m_InfoBackground = null;
#pragma warning restore CS0414

        /// <summary>
        /// Build editing UI
        /// </summary>
        /// <param name="p_Parent">Parent transform</param>
        public override sealed void BuildUI(Transform p_Parent)
        {
            string l_BSML = Utilities.GetResourceContent(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            /// Change opacity
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_InfoBackground, 0.5f);
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
            if (p_Context.Type != TriggerType.LevelEnded || p_Context.LevelCompletionData == null)
                return false;

            return true;
        }
        /// <summary>
        /// Build provided value dictionary
        /// </summary>
        /// <param name="p_Context">Event context</param>
        protected override sealed void BuildProvidedValues(Models.EventContext p_Context)
        {
            Int64  l_NoteCount  = p_Context.LevelCompletionData.Data.difficultyBeatmap.beatmapData.cuttableNotesCount;
            Int64  l_HitCount   = p_Context.LevelCompletionData.Results.goodCutsCount;
            Int64  l_MissCount  = l_NoteCount - l_HitCount;
            float  l_Accuracy   = (float)System.Math.Round(100.0f * BeatSaberPlus.SDK.Game.Levels.GetScorePercentage(BeatSaberPlus.SDK.Game.Levels.GetMaxScore((int)l_NoteCount), p_Context.LevelCompletionData.Results.rawScore), 2);
            string l_GameMode   = p_Context.LevelCompletionData.Data.difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
            string l_Difficulty = p_Context.LevelCompletionData.Data.difficultyBeatmap.difficulty.Name();

            p_Context.AddValue(IValueType.Integer,  "NoteCount",  (Int64?)l_NoteCount);
            p_Context.AddValue(IValueType.Integer,  "HitCount",   (Int64?)l_HitCount);
            p_Context.AddValue(IValueType.Integer,  "MissCount",  (Int64?)l_MissCount);
            p_Context.AddValue(IValueType.Floating, "Accuracy",   (float?)l_Accuracy);
            p_Context.AddValue(IValueType.String,   "SongName",   p_Context.LevelCompletionData.Data.difficultyBeatmap.level.songAuthorName + " - " + p_Context.LevelCompletionData.Data.difficultyBeatmap.level.songName);
            p_Context.AddValue(IValueType.String,   "Difficulty", l_GameMode + " - " + l_Difficulty);
        }
    }
}