﻿using BeatSaberMarkupLanguage.Attributes;
using System.Diagnostics;
using UnityEngine;

namespace BeatSaberPlus_NoteTweaker.UI
{
    /// <summary>
    /// Chat request settings left screen
    /// </summary>
    internal class SettingsLeft : BeatSaberPlus.SDK.UI.ResourceViewController<SettingsLeft>
    {
        private static readonly string s_InformationsStr = "<line-height=125%><b><u>Note Tweaker</u></b>"
            + "\n" + "<i><color=#CCCCCCFF>This module allow you to customize the default notes</color></i>"
            + "\n"
            + "\n"
            + "\n"
            + "\n"
            + "\n"
            + "\n"
            + "\n"
            + "\n";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#pragma warning disable CS0414
        [UIObject("Background")]
        private GameObject m_Background = null;
        [UIComponent("Informations")]
        private HMUI.TextPageScrollView m_Informations = null;
#pragma warning restore CS0414

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_Background, 0.5f);
            m_Informations.SetText(s_InformationsStr);
            m_Informations.UpdateVerticalScrollIndicator(0);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Documentation button
        /// </summary>
        [UIAction("click-documentation-btn-pressed")]
        private void OnDocumentationButton()
        {
            ShowMessageModal("URL opened in your desktop browser.");
            Process.Start("https://github.com/hardcpp/BeatSaberPlus/wiki#note-tweaker");
        }
    }
}
