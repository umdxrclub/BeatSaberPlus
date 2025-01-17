﻿using Newtonsoft.Json;
using System;

namespace BeatSaberPlus_ChatIntegrations.Models
{
    /// <summary>
    /// Action data modal
    /// </summary>
    [Serializable]
    public class Action
    {
        [JsonProperty]
        public string Type = "?";
        [JsonProperty]
        public bool Enabled = false;
        [JsonProperty]
        public string BaseValue = "";
    }
}
