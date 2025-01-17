﻿namespace BeatSaberPlus.UI
{
    /// <summary>
    /// UI flow coordinator
    /// </summary>
    public class MainViewFlowCoordinator : SDK.UI.ViewFlowCoordinator<MainViewFlowCoordinator>
    {
        /// <summary>
        /// Title
        /// </summary>
        public override string Title => "Beat Saber Plus V5.0.1";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Info view
        /// </summary>
        private InfoView m_InfoView;
        /// <summary>
        /// Main view
        /// </summary>
        private MainView m_MainView;
        /// <summary>
        /// Settings view
        /// </summary>
        private SettingsView m_SettingsView;
        /// <summary>
        /// Settings left view
        /// </summary>
        private SettingsLeftView m_SettingsLeftView;
        /// <summary>
        /// Settings right view
        /// </summary>
        private SettingsRightView m_SettingsRightView;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        public MainViewFlowCoordinator()
        {
            m_InfoView          = CreateViewController<InfoView>();
            m_MainView          = CreateViewController<MainView>();
            m_SettingsView      = CreateViewController<SettingsView>();
            m_SettingsLeftView  = CreateViewController<SettingsLeftView>();
            m_SettingsRightView = CreateViewController<SettingsRightView>();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get initial views controller
        /// </summary>
        /// <returns>(Middle, Left, Right)</returns>
        protected override sealed (HMUI.ViewController, HMUI.ViewController, HMUI.ViewController) GetInitialViewsController() => (m_MainView, m_InfoView, null);
        /// <summary>
        /// On back button pressed
        /// </summary>
        /// <param name="p_TopViewController">Current top view controller</param>
        /// <returns>True if the event is catched, false if we should dismiss the flow coordinator</returns>
        protected override sealed bool OnBackButtonPressed(HMUI.ViewController p_TopViewController)
        {
            if (topViewController != m_MainView)
            {
                SwitchToMainView();
                return true;
            }

            return false;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Switch to main view
        /// </summary>
        public void SwitchToMainView() => ChangeView(m_MainView, m_InfoView, null);
        /// <summary>
        /// Switch to settings view
        /// </summary>
        public void SwitchToSettingsView() => ChangeView(m_SettingsView, m_SettingsLeftView, m_SettingsRightView);
    }
}
