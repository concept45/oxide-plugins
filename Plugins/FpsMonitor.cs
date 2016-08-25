﻿using System;
using System.Collections.Generic;

namespace Oxide.Plugins
{
    [Info("FpsMonitor", "Wulf/lukespragg", "0.1.0")]
    [Description("Automatically restarts server if FPS is below amount for period of time.")]

    class FpsMonitor : CovalencePlugin
    {
        // Do NOT edit this file, instead edit FpsMonitor.json in oxide/config

        #region Localization

        void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {

            }, this);
        }

        #endregion

        #region Configuration

        int CheckEverySeconds => GetConfig("CheckEverySeconds", 5);
        bool ShowRestartWarning => GetConfig("ShowRestartWarning", true);

        protected override void LoadDefaultConfig()
        {
            Config["CheckEverySeconds"] = CheckEverySeconds;
            Config["ShowRestartWarning"] = ShowRestartWarning;
            SaveConfig();
        }

        #endregion

        #region Initialization

        void Init()
        {
            #if !HURTWORLD && !REIGNOFKINGS && !RUST
            throw new NotSupportedException("This plugin does not support this game");
            #endif

            LoadDefaultConfig();
        }

        void OnServerInitialized() => timer.Repeat(CheckEverySeconds, 0, FpsCheck);

        #endregion

        #region FPS Checking

        void FpsCheck()
        {
            //PrintWarning(GetFps().ToString());
            // TODO
        }

        int GetFps()
        {
            #if HURTWORLD || REIGNOFKINGS || RUST || RUSTLEGACY
            return UnityEngine.Mathf.RoundToInt(1f / UnityEngine.Time.smoothDeltaTime);
            #endif
        }

        #endregion

        #region Helper Methods

        T GetConfig<T>(string name, T defaultValue)
        {
            if (Config[name] == null) return defaultValue;
            return (T) Convert.ChangeType(Config[name], typeof(T));
        }

        #endregion
    }
}
