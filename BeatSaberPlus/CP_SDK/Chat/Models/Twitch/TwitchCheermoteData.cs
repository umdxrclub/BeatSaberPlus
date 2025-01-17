﻿using CP_SDK.Chat.Interfaces;
using System.Collections.Generic;

namespace CP_SDK.Chat.Models.Twitch
{
    public class CheermoteTier : IChatResourceData
    {
        public string Uri { get; internal set; }
        public int MinBits { get; internal set; }
        public string Color { get; internal set; }
        public Animation.EAnimationType Animation { get; internal set; } = CP_SDK.Animation.EAnimationType.GIF;
        public EChatResourceCategory Category { get; internal set; } = EChatResourceCategory.Cheermote;
        public string Type { get; internal set; } = "TwitchCheermote";
    }

    public class TwitchCheermoteData
    {
        public string Prefix;
        public List<CheermoteTier> Tiers = new List<CheermoteTier>();

        public CheermoteTier GetTier(int numBits)
        {
            for (int i = 1; i < Tiers.Count; i++)
            {
                if (numBits < Tiers[i].MinBits)
                    return Tiers[i - 1];
            }
            return Tiers[0];
        }
    }
}
