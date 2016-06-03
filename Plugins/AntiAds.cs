﻿using System;using System.Collections.Generic;namespace Oxide.Plugins
{
    [Info("AntiAds", "Wulf/lukespragg", "0.2.0")]
    [Description("")]

    class AntiAds : CovalencePlugin
    {
        // Do NOT edit this file, instead edit AntiAds.json in oxide/config and AntiAds.en.json in the oxide/lang directory,
        // or create a new language file for another language using the 'en' file as a default.

        #region Localization

        void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {                {"NotAllowed", "Advertising is not allowed on this server"}            }, this);
        }

        #endregion

        #region Initialization

        void Loaded()
        {
            #if !HURTWORLD && !REIGNOFKINGS && !RUST && !RUSTLEGACY
            throw new NotSupportedException("This plugin does not support this game");
            #endif

            LoadDefaultMessages();
        }

        #endregion        object IsAdvertising(string userId, string message)        {
            //text:match("(%d+.%d+.%d+.%d+:%d+)")
            return null;        }

        #region

        #if HURTWORLD
        void OnPlayerChat(PlayerSession session, string message) => IsAdvertising(session.SteamId.ToString(), message);
        #endif

        #if REIGNOFKINGS
        void OnPlayerChat(PlayerEvent evt) =>
        #endif

        #if RUST
        void OnPlayerChat(ConsoleSystem.Arg arg) => IsAdvertising(arg.connection.userid.ToString(), arg:GetString(0, "text"))
        #endif

        #if RUSTLEGACY
        void OnPlayerChat(NetUser netuser, string message) => IsAdvertising(netUser.UserID.ToString(), message)
        #endif

        #endregion

        #region Helper Methods

        string GetMessage(string key, string userId = null) => lang.GetMessage(key, this, userId);

        #endregion
    }
}
