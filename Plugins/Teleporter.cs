﻿// Reference: UnityEngine.UI

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Teleporter", "Wulf/lukespragg", "0.0.2")]
    [Description("Basic player-to-player and player-to-location teleportation.")]

    class Teleporter : HurtworldPlugin
    {
        // Do NOT edit this file, instead edit Teleporter.json in oxide/config and Teleporter.en.json in oxide/lang,
        // or create a language file for another language using the 'en' file as a default.

        #region Localization

        readonly Dictionary<string, string> messages = new Dictionary<string, string>();

        void LoadDefaultMessages()
        {
            messages.Add("NoPermission", "Sorry, you can't use teleporter right now");
            lang.RegisterMessages(messages, this);
        }

        #endregion

        #region Initialization

        void Loaded()
        {
            #if !HURTWORLD
            throw new NotSupportedException("This plugin does not support this game");
            #endif

            LoadDefaultMessages();
            permission.RegisterPermission("teleporter.player", this);
            permission.RegisterPermission("teleporter.coords", this);
            permission.RegisterPermission("teleporter.request", this);
        }

        #endregion

        #region Teleport Player

        #region Hurtworld
        #if HURTWORLD

        [ChatCommand("tp")]
        void TeleportToPlayer(PlayerSession session, string command, string[] args)
        {
            var steamId = session.SteamId.ToString();
            if (!session.IsAdmin && !HasPermission(steamId, "teleporter.all") && !HasPermission(steamId, "teleporter.player"))
            {
                hurt.SendChatMessage(session, GetMessage("NoPermission", steamId));
                return;
            }

            //session.ConnectedNetworkPlayer.Value
            // TODO: Check if teleporting self

            var sessions = GameManager.Instance.GetSessions();
            var playerMap = sessions.FirstOrDefault(x => string.Equals(x.Value.Name, args[0]));
            var targetMap = sessions.FirstOrDefault(x => string.Equals(x.Value.Name, args[1]));
            if (playerMap.Value == null || targetMap.Value == null) return;

            var playerEntity = GameManager.GetPlayerEntity(playerMap.Key);
            var targetEntity = GameManager.GetPlayerEntity(targetMap.Key);
            if (playerEntity != null && targetEntity != null) playerEntity.transform.position = targetEntity.transform.position;
        }

        [ChatCommand("tpa")]
        void TeleportAccept(PlayerSession session, string command, string[] args)
        {
            // TODO
        }

        [ChatCommand("tpc")]
        void TeleportToCoords(PlayerSession session, string command, string[] args)
        {
            var steamId = session.SteamId.ToString();
            if (!session.IsAdmin && !HasPermission(steamId, "teleporter.all") && !HasPermission(steamId, "teleporter.coords"))
            {
                hurt.SendChatMessage(session, GetMessage("NoPermission", steamId));
                return;
            }

            var player = GameManager.GetPlayerEntity(session.Player);
            player.transform.position = new Vector3(Convert.ToSingle(args[0]), Convert.ToSingle(args[1]), Convert.ToSingle(args[2]));
        }

        [ChatCommand("tpr")]
        void TeleportRequest(PlayerSession session, string command, string[] args)
        {
            var steamId = session.SteamId.ToString();
            if (!session.IsAdmin && !HasPermission(steamId, "teleporter.all") && !HasPermission(steamId, "teleporter.request"))
            {
                hurt.SendChatMessage(session, GetMessage("NoPermission", steamId));
                return;
            }

            // TODO
        }

        #endif
        #endregion

        #endregion

        #region Helper Methods

        T GetConfig<T>(string name, T defaultValue)
        {
            if (Config[name] == null) return defaultValue;
            return (T)Convert.ChangeType(Config[name], typeof(T));
        }

        string GetMessage(string key, string steamId = null) => lang.GetMessage(key, this, steamId);

        bool HasPermission(string steamId, string perm) => permission.UserHasPermission(steamId, perm);

        #endregion
    }
}