﻿using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace BeatSaberPlus_ChatIntegrations.Interfaces
{
    /// <summary>
    /// ICondition generic class
    /// </summary>
    public interface IConditionBase
    {
        /// <summary>
        /// Event instance
        /// </summary>
        IEventBase Event { get; set; }
        /// <summary>
        /// Condition description
        /// </summary>
        string Description { get; }
        /// <summary>
        /// UI PlaceHolder description
        /// </summary>
        string UIPlaceHolder { get; }
        /// <summary>
        /// Is enabled
        /// </summary>
        bool IsEnabled { get; set; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get type name
        /// </summary>
        /// <returns></returns>
        string GetTypeName();
        /// <summary>
        /// Get type name
        /// </summary>
        /// <returns></returns>
        string GetTypeNameShort();
        /// <summary>
        /// Serialize
        /// </summary>
        /// <returns></returns>
        JObject Serialize();
        /// <summary>
        /// Unserialize
        /// </summary>
        /// <param name="p_Serialized"></param>
        bool Unserialize(JObject p_Serialized);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Build editing UI
        /// </summary>
        /// <param name="p_Parent">Parent transform</param>
        void BuildUI(Transform p_Parent);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Handle
        /// </summary>
        /// <param name="p_Context">Event context</param>
        bool Eval(Models.EventContext p_Context);
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// ICondition generic class
    /// </summary>
    public abstract class ICondition<T, M> : IConditionBase
        where T : ICondition<T, M>, new()
        where M : Models.Condition, new()
    {
        /// <summary>
        /// Event instance
        /// </summary>
        public IEventBase Event { get; set; }
        /// <summary>
        /// Condition description
        /// </summary>
        public abstract string Description { get; }
        /// <summary>
        /// UI PlaceHolder description
        /// </summary>
        [UIValue("BSPCIUIPlaceHolder")]
        public string UIPlaceHolder { get; protected set; } = "<alpha=#AA><i>No available settings...</i>";
        /// <summary>
        /// Is enabled
        /// </summary>
        public bool IsEnabled { get => Model.Enabled; set { Model.Enabled = value; } }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Model
        /// </summary>
        public M Model { get; protected set; } = new M();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get type name
        /// </summary>
        /// <returns></returns>
        public string GetTypeName()
        {
            return string.Join(".", typeof(T).Namespace, typeof(T).Name);
        }
        /// <summary>
        /// Get type name
        /// </summary>
        /// <returns></returns>
        public string GetTypeNameShort()
        {
            return typeof(T).Name;
        }
        /// <summary>
        /// Serialize
        /// </summary>
        /// <returns></returns>
        public JObject Serialize()
        {
            Model.Type = GetTypeName();
            return JObject.FromObject(Model);
        }
        /// <summary>
        /// Unserialize
        /// </summary>
        /// <param name="p_Serialized"></param>
        public bool Unserialize(JObject p_Serialized)
        {
            if (!p_Serialized.ContainsKey(nameof(Models.Condition.Type)) || p_Serialized[nameof(Models.Condition.Type)].Value<string>() != GetTypeName())
                return false;

            Model = p_Serialized.ToObject<M>();
            Model.OnDeserialized(p_Serialized);

            return true;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Build editing UI
        /// </summary>
        /// <param name="p_Parent">Parent transform</param>
        public virtual void BuildUI(Transform p_Parent)
        {
            BSMLParser.instance.Parse("<text text=\"~BSPCIUIPlaceHolder\" align=\"Center\" />", p_Parent.gameObject, this);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Handle
        /// </summary>
        /// <param name="p_Context">Event context</param>
        public abstract bool Eval(Models.EventContext p_Context);
    }
}
