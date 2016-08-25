﻿using System;
using System.Collections.Generic;

namespace Oxide.Plugins
{
    [Info("ThirdPerson", "Wulf/lukespragg", "0.1.4", ResourceId = 1424)]
    [Description("Allows any player with permission to use third-person view")]

    class ThirdPerson : RustPlugin
    {
        // Do NOT edit this file, instead edit ThirdPerson.en.json in the oxide/lang directory,
        // or create a new language file for another language using the 'en' file as a default

        #region Initialization

        const string permAllow = "thirdperson.allow";

        void Init()
        {
            lang.RegisterMessages(new Dictionary<string, string> { ["NotAllowed"] = "Sorry, you can't use '{0}' right now" }, this);
            permission.RegisterPermission(permAllow, this);
        }

        #endregion

        #region Chat Command

        [ChatCommand("view")]
        void ViewCommand(BasePlayer player, string command, string[] args)
        {
            if (!HasPermission(player.UserIDString, permAllow))
            {
                SendReply(player, Lang("NotAllowed", player.UserIDString, command));
                return;
            }

            player.SetPlayerFlag(BasePlayer.PlayerFlags.ThirdPersonViewmode, !player.HasPlayerFlag(BasePlayer.PlayerFlags.ThirdPersonViewmode));
        }

        #endregion

        #region Helpers

        T GetConfig<T>(string name, T value) => Config[name] == null ? value : (T)Convert.ChangeType(Config[name], typeof(T));

        string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);

        bool HasPermission(string id, string perm) => permission.UserHasPermission(id, perm);

        #endregion
    }
}
